using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.DTO.AddDTO;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.UpdateDTO
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid Id { get; set; }   // Primary key for identifying the user

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string UserName { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [Url(ErrorMessage = "Invalid profile image URL")]
        public string? ProfileImageUrl { get; set; }
    }
}
