using ApartmentBookingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentBookingSystem.Areas.Admin.Controllers
{
    //[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalApartments = _context.Apartments.Count();
            var totalBookings = _context.Bookings.Count();
            var totalUsers = _context.Users.Count();

            ViewBag.TotalApartments = totalApartments;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }
    }
}