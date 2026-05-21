using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using AmazonWeb.Core.DTO.AccountDTO;
using Microsoft.AspNetCore.Mvc;
using AmazonWeb.Core.Domain.Identities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // FIX 1: Enforce security validation BEFORE altering database state
            // Prevent public endpoints from self-assigning Admin roles
            if (registerDTO.UserRole == Role.Admin)
            {
                ModelState.AddModelError("UserRole", "Administrative accounts cannot be self-registered publicly.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Fallback assertion check for enums
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

            // Create user with password (automatic duplicate email handling happens here)
            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    // Check for Identity's native duplicate email error code to append custom error location
                    if (error.Code == "DuplicateEmail" || error.Code == "DuplicateUserName")
                        ModelState.AddModelError("Email", "Email address is already registered.");
                    else
                        ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // FIX 2: Ensure default base role exists in DB safely
            string targetRole = Role.User.ToString();
            if (await _roleManager.FindByNameAsync(targetRole) == null)
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = targetRole });
            }

            // Assign standard role safely
            await _userManager.AddToRoleAsync(user, targetRole);

            // Establish sign-in persistence securely
            await _signInManager.SignInAsync(user, isPersistent: registerDTO.stayLoggedIn);

            // Construct response DTO
            var response = new SignInDTO()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                UserRole = Role.User, // Explicitly safe assignment
                stayLoggedIn = registerDTO.stayLoggedIn
            };

            return Ok(response);
        }
    }
}