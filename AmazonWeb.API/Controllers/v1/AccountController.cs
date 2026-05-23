using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.DTO.AccountDTO;
using AmazonWeb.Core.ServiceContracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IFileService _fileService;

        public AccountController(SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IFileService fileService)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _fileService = fileService;
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            if (registerDTO.UserRole == Role.Admin)
            {
                ModelState.AddModelError("UserRole", "Administrative accounts cannot be self-registered publicly.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if (registerDTO.UserRole != Role.User)
            {
                ModelState.AddModelError("UserRole", "Please select a valid user role.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Gender = registerDTO.Gender,
                DateOfBirth = registerDTO.DateOfBirth
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateEmail" || error.Code == "DuplicateUserName")
                        ModelState.AddModelError("Email", "Email address is already registered.");
                    else
                        ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            string targetRole = Role.User.ToString();
            if (await _roleManager.FindByNameAsync(targetRole) == null)
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = targetRole });
            }

            await _userManager.AddToRoleAsync(user, targetRole);
            await _signInManager.SignInAsync(user, isPersistent: registerDTO.stayLoggedIn);

            var response = new SignInDTO()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                UserRole = Role.User,
                stayLoggedIn = registerDTO.stayLoggedIn
            };



            return Ok(response);
        }

        // 🔐 1. USER LOGIN METHOD
        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Authenticate via Identity SignInManager (handles hashing & checking checks)
            var result = await _signInManager.PasswordSignInAsync(
                loginDTO.Email,
                loginDTO.Password,
                isPersistent: loginDTO.RememberMe,
                lockoutOnFailure: true // Protects against brute-force attacks
            );

            if (result.Succeeded)
            {
                // Fetch the logged-in user profile to return metadata to React
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);

                // Fetch user roles dynamically
                var roles = await _userManager.GetRolesAsync(user!);
                string primaryRole = roles.Count > 0 ? roles[0] : Role.User.ToString();
                Enum.TryParse<Role>(primaryRole, out Role userRoleEnum);

                var response = new SignInDTO()
                {
                    Email = user!.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    UserRole = userRoleEnum,
                    stayLoggedIn = loginDTO.RememberMe
                };

                return Ok(response);
            }

            // Provide defensive security messaging (Don't reveal if the email or password specifically was wrong)
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked out due to too many failed attempts. Try again later.");
            }
            else
            {
                ModelState.AddModelError("Password", "Invalid email address or password configuration.");
            }

            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        // 🔄 2. PASSWORD UPDATE METHOD (Authenticated Users)
        [HttpPost]
        [Authorize] // 👈 Requires an active authentication token/cookie session
        [Route("[Action]")]
        public async Task<ActionResult> UpdatePassword(UpdatePasswordDTO updatePasswordDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Extract the User ID cleanly out of the active HttpContext ClaimsPrincipal context
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            ApplicationUser? user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return NotFound("User account records could not be resolved.");

            // Identity handles verifying the old password before applying the change
            var result = await _userManager.ChangePasswordAsync(
                user,
                updatePasswordDTO.CurrentPassword,
                updatePasswordDTO.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    // Target validation message placement on your React forms
                    if (error.Code.Contains("Password"))
                        ModelState.AddModelError("NewPassword", error.Description);
                    else
                        ModelState.AddModelError("CurrentPassword", error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Refresh the user's login cookie/session state so they don't get kicked out for updating credentials
            await _signInManager.RefreshSignInAsync(user);

            return Ok(new { Message = "Password updated successfully." });
        }

        [HttpPost]
        [Authorize] // 👈 Requires an active authentication token/cookie session
        [Route("[Action]")]
        public async Task<ActionResult> UpdateProfile([FromForm] UserUpdateRequest request)
        {
            // Note: We use [FromForm] so .NET can read both string fields and files simultaneously
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // 1. Secure extraction out of the identity passport token
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            ApplicationUser? user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
                return NotFound("User account records could not be resolved.");

            // 2. Mutate address parameters safely (falling back to existing text if fields are null)
            user.Address = request.Address ?? user.Address;
            user.City = request.City ?? user.City;
            user.State = request.State ?? user.State;
            user.PostalCode = request.PostalCode ?? user.PostalCode;
            user.Country = request.Country ?? user.Country;

            // 3. File upload logic if a new profile picture is attached
            if (request.ProfileImage != null && request.ProfileImage.Length > 0)
            {
                // Leverage your decoupled file service interface to dump the avatar into disk storage securely
                user.ProfileImageUrl = await _fileService.UploadThumbnailAsync(request.ProfileImage, user.Id);
            }

            // 4. Update the record within ASP.NET Identity
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // 5. Send clean confirmation response state back to React
            return Ok(new
            {
                Message = "Profile updated successfully.",
                ProfileImageUrl = user.ProfileImageUrl,
                Address = user.Address,
                City = user.City
            });
        }
    }
}