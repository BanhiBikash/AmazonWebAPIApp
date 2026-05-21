using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.DTO.AddDTO;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AccountDTO
{
    public class UserUpdateRequest
    {
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

        [Required(ErrorMessage = "Product thumbnail is required")]
        public IFormFile ProfileImage { get; set; }
    }
}
