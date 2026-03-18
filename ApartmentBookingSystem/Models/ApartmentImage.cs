using System.ComponentModel.DataAnnotations;

namespace ApartmentBookingSystem.Models
{
    public class ApartmentImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int ApartmentId { get; set; }

        public Apartment Apartment { get; set; }
    }
}