namespace ApartmentBookingSystem.Models
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(14, MinimumLength = 14)]
        public string NationalId { get; set; }
    }
}
