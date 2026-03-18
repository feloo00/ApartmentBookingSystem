using ApartmentBookingSystem.Data;
using ApartmentBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ApartmentBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var apartments = await _context.Apartments
                .Include(a => a.Images)
                .Where(a => a.IsAvailable)
                .ToListAsync();

            return View(apartments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var apartment = await _context.Apartments
                .Include(a => a.Images)
                .Include(a => a.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
                return NotFound();

            return View(apartment);
        }


        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}