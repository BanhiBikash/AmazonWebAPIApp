using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using AmazonWeb.Core.DTO.AccountDTO;
using Microsoft.AspNetCore.Mvc;
using AmazonWeb.Core.Domain.Identities;
using Microsoft.AspNetCore.Identity;

namespace AmazonWeb.API.Controllers.v1
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost] 
        [Route("[Action]")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            // Validate DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Create ApplicationUser object
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

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email address is already registered.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Create user with password
            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            //registration failed, return errors
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            if(registerDTO.UserRole == Role.User || registerDTO.UserRole == Role.Admin)
            {
                //create role if it doesn't exist
                if (( await _roleManager.FindByNameAsync(registerDTO.UserRole.ToString()))==null)
                {
                    await _roleManager.CreateAsync(new ApplicationRole { Name = registerDTO.UserRole.ToString() });
                }
                // Assign role (default User role from DTO)
                await _userManager.AddToRoleAsync(user, registerDTO.UserRole.ToString());

                // Optionally sign in immediately if stayLoggedIn is true
                await _signInManager.SignInAsync(user, isPersistent: registerDTO.stayLoggedIn);

                //create sign in dto to send back to client
                return Ok(new SignInDTO() { Email=user.Email,FirstName=user.FirstName,LastName=user.LastName,DateOfBirth=user.DateOfBirth, Gender=user.Gender, UserRole = user.UserRole, stayLoggedIn=registerDTO.stayLoggedIn});
            }
            else
            {
                ModelState.AddModelError("UserRole","Choose a correct user role.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
        }

    }
}
