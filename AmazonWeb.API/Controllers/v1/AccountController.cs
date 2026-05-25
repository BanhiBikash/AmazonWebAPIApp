using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.DTO.AccountDTO;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.ServiceContracts.TokenContracts;
using AmazonWeb.Infrastructure.Migrations;
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
        private readonly IJWTTokenservice _jwtTokenService;
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration,SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IFileService fileService, IJWTTokenservice jWTTokenservice)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _fileService = fileService;
            _jwtTokenService = jWTTokenservice;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            //if (registerDTO.UserRole == Role.Admin)
            //{
            //    ModelState.AddModelError("UserRole", "Administrative accounts cannot be self-registered publicly.");
            //    return BadRequest(new ValidationProblemDetails(ModelState));
            //}

            if (registerDTO.UserRole != Role.User && registerDTO.UserRole != Role.Admin)
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
                DateOfBirth = registerDTO.DateOfBirth,
                UserRole = registerDTO.UserRole
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

            string targetRole = user.UserRole.ToString();
            if (await _roleManager.FindByNameAsync(targetRole) == null)
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = targetRole });
            }

            await _userManager.AddToRoleAsync(user, targetRole);
            //remove this if the sync causes issues , but it should be fine since we are not doing any other operations after this that would require the user to be fully signed in
            await _signInManager.SignInAsync(user, isPersistent: registerDTO.stayLoggedIn);

            //create jwt and refresh token  
            string userID = Convert.ToString(user.Id);
            string jwtToken = _jwtTokenService.CreateJWTToken(user.Email,$"{user.FirstName}{user.LastName}".Trim(),userID,targetRole);
            string refreshToken = _jwtTokenService.CreateRefreshToken();

            //storing the refresh token in the database
            int daysToExpire = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(daysToExpire);
            await _userManager.UpdateAsync(user);
            
            var response = new SignInDTO()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                UserRole = user.UserRole,
                stayLoggedIn = registerDTO.stayLoggedIn,
                JWTToken = jwtToken,
                RefreshToken = refreshToken
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

                //remove this if the sync causes issues , but it should be fine since we are not doing any other operations after this that would require the user to be fully signed in
                await _signInManager.SignInAsync(user, isPersistent: loginDTO.RememberMe);

                //create jwt and refresh token  
                string userID = Convert.ToString(user.Id);
                string jwtToken = _jwtTokenService.CreateJWTToken(user.Email, $"{user.FirstName}{user.LastName}".Trim(), userID, user.UserRole.ToString());
                string refreshToken = _jwtTokenService.CreateRefreshToken();

                //storing the refresh token in the database
                int daysToExpire = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(daysToExpire);
                await _userManager.UpdateAsync(user);

                var response = new SignInDTO()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    UserRole = user.UserRole,
                    stayLoggedIn = loginDTO.RememberMe,
                    JWTToken = jwtToken,
                    RefreshToken = refreshToken
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

        [HttpPost]
        [Route("[Action]")]
        public async Task<ActionResult> Refresh(TokenRequestDTO tokenRequestDTO)
        {
            if (tokenRequestDTO == null || string.IsNullOrEmpty(tokenRequestDTO.Token) || string.IsNullOrEmpty(tokenRequestDTO.RefreshToken))
            {
                return BadRequest("Invalid client request token parameters.");
            }

            try
            {
                // 1. Decode the expired token to find out who this user claims to be
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(tokenRequestDTO.Token);
                var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                    return Unauthorized("Token context claims mapping missing basic identifiers.");

                // 2. Look up the user record in your SQL Database
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Unauthorized("User record corresponding to token context not found.");

                // 3. 🛡️ CRITICAL SECURITY CHECK: Verify the Refresh Token match and lifecycle expiration dates
                if (user.RefreshToken != tokenRequestDTO.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    // Token is either completely invalid, tampered with, or expired in DB. Kick them out!
                    return Unauthorized("Refresh token has expired or is invalid. Please sign in again.");
                }

                // 4. Generate a fresh new pair of tokens
                var role = user.UserRole;
                var fullName = $"{user.FirstName} {user.LastName}".Trim();

                string newAccessToken = _jwtTokenService.CreateJWTToken(user.Email!, fullName, user.Id.ToString(), role.ToString());
                string newRefreshToken = _jwtTokenService.CreateRefreshToken();

                //storing the refresh token in the database
                int daysToExpire = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(daysToExpire);
                await _userManager.UpdateAsync(user);

                // 6. Return them back to the caller
                return Ok(new
                {
                    jwtToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                return Unauthorized("Token parsing failure state: " + ex.Message);
            }
        }
    }
}