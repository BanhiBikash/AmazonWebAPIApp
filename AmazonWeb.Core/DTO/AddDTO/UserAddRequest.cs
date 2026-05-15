using AmazonWeb.Core.Domain.Identities;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class UserAddRequest
    {

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        public static ApplicationUser ToApplicationUser(UserAddRequest userAddRequest)
        {
            return new ApplicationUser()
            {
                UserName = userAddRequest.Email,
                Email = userAddRequest.Email,
                FirstName = userAddRequest.FirstName,
                LastName = userAddRequest.LastName,
                DateOfBirth = userAddRequest.DateOfBirth,
                Gender = userAddRequest.Gender
            };
        }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}
