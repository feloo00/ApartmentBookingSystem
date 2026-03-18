using ApartmentBookingSystem.Models;

namespace ApartmentBookingSystem.ViewModels
{
    public class BookingAdminViewModel
    {
        public List<Booking> PendingBookings { get; set; } = new();
        public List<Booking> ApprovedBookings { get; set; } = new();
        public List<Booking> RejectedBookings { get; set; } = new();
    }
}