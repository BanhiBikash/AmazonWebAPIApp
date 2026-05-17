using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.DeleteDTO
{
    public class UserDeleteRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "IsDeleted flag is required")]
        public bool IsDeleted { get; set; }
    }
}
