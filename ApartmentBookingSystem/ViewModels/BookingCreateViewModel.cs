using ApartmentBookingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace ApartmentBookingSystem.ViewModels
{
    public class BookingCreateViewModel
    {
        public int ApartmentId { get; set; }

        public Apartment Apartment { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public IFormFile? PaymentProofImage { get; set; }

        public decimal PricePerNight { get; set; }

        public int NumberOfNights { get; set; }

        public decimal TotalPrice { get; set; }
    }
}