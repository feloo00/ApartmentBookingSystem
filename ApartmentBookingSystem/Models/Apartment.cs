using System.ComponentModel.DataAnnotations;

namespace ApartmentBookingSystem.Models
{
    public class Apartment
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal PricePerNight { get; set; }

        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public int MaxGuests { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string Area { get; set; }

        [Required]
        [MaxLength(300)]
        public string Address { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? VideoUrl { get; set; }

        public double ApartmentSize { get; set; }   
        public string? LocationUrl { get; set; } 

        public ICollection<ApartmentImage> Images { get; set; } = new List<ApartmentImage>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review>? Reviews { get; set; }
    }
}