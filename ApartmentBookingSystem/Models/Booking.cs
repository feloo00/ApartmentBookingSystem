using System.ComponentModel.DataAnnotations;

namespace ApartmentBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public int TotalNights { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public string PaymentMethod { get; set; }  // InstaPay / VodafoneCash

        [Required]
        public string PaymentStatus { get; set; }
        // Pending / WaitingForConfirmation / Confirmed / Rejected

        [Required]
        public string BookingStatus { get; set; }
        // Pending / Approved / Cancelled
        public string? PaymentProofImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? AdminNotes { get; set; }
    }
}