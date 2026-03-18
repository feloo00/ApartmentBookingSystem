using ApartmentBookingSystem.Data;
using ApartmentBookingSystem.Models;
using ApartmentBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApartmentBookingSystem.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookingsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpGet]
        public async Task<IActionResult> Create(int apartmentId)
        {
            var apartment = await _context.Apartments
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == apartmentId);

            if (apartment == null)
                return NotFound();

            var vm = new BookingCreateViewModel
            {
                ApartmentId = apartment.Id,
                Apartment = apartment,
                PricePerNight = apartment.PricePerNight
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel vm)
        {
            var apartment = await _context.Apartments
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == vm.ApartmentId);

            if (apartment == null)
                return NotFound();

            vm.Apartment = apartment;
            vm.PricePerNight = apartment.PricePerNight;

            if (!vm.CheckInDate.HasValue || !vm.CheckOutDate.HasValue)
            {
                ModelState.AddModelError("", "Please select check-in and check-out dates.");
                return View(vm);
            }

            if (vm.CheckInDate.Value.Date >= vm.CheckOutDate.Value.Date)
            {
                ModelState.AddModelError("", "Check-out date must be after check-in date.");
                return View(vm);
            }

            if (string.IsNullOrEmpty(vm.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Please choose a payment method.");
                return View(vm);
            }

            if (vm.PaymentMethod != "VodafoneCash" && vm.PaymentMethod != "InstaPay")
            {
                ModelState.AddModelError("PaymentMethod", "Invalid payment method.");
                return View(vm);
            }

            if (vm.PaymentProofImage == null || vm.PaymentProofImage.Length == 0)
            {
                ModelState.AddModelError("PaymentProofImage", "Please upload payment proof image.");
                return View(vm);
            }

            bool hasConflict = await _context.Bookings.AnyAsync(b =>
                b.ApartmentId == vm.ApartmentId &&
                b.BookingStatus != "Cancelled" &&
                vm.CheckInDate.Value.Date < b.CheckOutDate.Date &&
                vm.CheckOutDate.Value.Date > b.CheckInDate.Date
            );

            if (hasConflict)
            {
                ModelState.AddModelError("", "Selected dates are not available.");
                return View(vm);
            }

            int nights = (vm.CheckOutDate.Value - vm.CheckInDate.Value).Days;
            decimal total = nights * apartment.PricePerNight;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "payments");
            Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(vm.PaymentProofImage.FileName).ToLower();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("PaymentProofImage", "Only JPG, JPEG, PNG, and WEBP files are allowed.");
                return View(vm);
            }

            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await vm.PaymentProofImage.CopyToAsync(stream);
            }

            string imageUrl = $"/uploads/payments/{fileName}";

            var user = await _userManager.GetUserAsync(User);

            var booking = new Booking
            {
                ApartmentId = vm.ApartmentId,
                UserId = user.Id,
                CheckInDate = vm.CheckInDate.Value,
                CheckOutDate = vm.CheckOutDate.Value,
                TotalPrice = total,
                PaymentMethod = vm.PaymentMethod,
                PaymentStatus = "WaitingForConfirmation",
                BookingStatus = "Pending",
                PaymentProofImageUrl = imageUrl,
                CreatedAt = DateTime.Now,
                TotalNights = nights,
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking has been submitted successfully and is waiting for payment confirmation.";

            return RedirectToAction("Success");
        }



        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard(string? status, string? search, int page = 1)
        {
            int pageSize = 6;

            var query = _context.Bookings
                .Include(b => b.Apartment)
                    .ThenInclude(a => a.Images)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    (b.Apartment.Title != null && b.Apartment.Title.Contains(search)) ||
                    (b.Apartment.City != null && b.Apartment.City.Contains(search)) ||
                    (b.Apartment.Area != null && b.Apartment.Area.Contains(search)) ||
                    (b.User.FullName != null && b.User.FullName.Contains(search)) ||
                    (b.User.Email != null && b.User.Email.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        query = query.Where(b => b.PaymentStatus == "WaitingForConfirmation" || b.BookingStatus == "Pending");
                        break;

                    case "approved":
                        query = query.Where(b => b.PaymentStatus == "Confirmed" && b.BookingStatus == "Approved");
                        break;

                    case "rejected":
                        query = query.Where(b => b.PaymentStatus == "Rejected" || b.BookingStatus == "Cancelled");
                        break;
                }
            }

            var filteredBookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            int totalItems = filteredBookings.Count;
            var pagedBookings = filteredBookings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new BookingAdminViewModel
            {
                PendingBookings = pagedBookings
                    .Where(b => b.PaymentStatus == "WaitingForConfirmation" || b.BookingStatus == "Pending")
                    .ToList(),

                ApprovedBookings = pagedBookings
                    .Where(b => b.PaymentStatus == "Confirmed" && b.BookingStatus == "Approved")
                    .ToList(),

                RejectedBookings = pagedBookings
                    .Where(b => b.PaymentStatus == "Rejected" || b.BookingStatus == "Cancelled")
                    .ToList()
            };

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.PendingCount = filteredBookings.Count(b =>
                b.PaymentStatus == "WaitingForConfirmation" || b.BookingStatus == "Pending");

            ViewBag.ApprovedCount = filteredBookings.Count(b =>
                b.PaymentStatus == "Confirmed" && b.BookingStatus == "Approved");

            ViewBag.RejectedCount = filteredBookings.Count(b =>
                b.PaymentStatus == "Rejected" || b.BookingStatus == "Cancelled");

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            booking.BookingStatus = "Approved";
            booking.PaymentStatus = "Confirmed";

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Booking approved successfully.";
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string adminNotes)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound();

            booking.BookingStatus = "Cancelled";
            booking.PaymentStatus = "Rejected";
            booking.AdminNotes = adminNotes;

            await _context.SaveChangesAsync();

            TempData["AdminMessage"] = "Booking rejected successfully.";
            return RedirectToAction(nameof(AdminDashboard));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings(string? status, string? search, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 6;

            var query = _context.Bookings
                .Include(b => b.Apartment)
                    .ThenInclude(a => a.Images)
                .Where(b => b.UserId == user.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Apartment.Title.Contains(search) ||
                    b.Apartment.City.Contains(search) ||
                    b.Apartment.Area.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status.ToLower())
                {
                    case "pending":
                        query = query.Where(b => b.BookingStatus == "Pending");
                        break;

                    case "approved":
                        query = query.Where(b => b.BookingStatus == "Approved");
                        break;

                    case "rejected":
                        query = query.Where(b => b.BookingStatus == "Cancelled" || b.PaymentStatus == "Rejected");
                        break;
                }
            }

            int totalItems = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(bookings);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            if (booking.BookingStatus != "Pending")
            {
                TempData["BookingMessage"] = "You can only cancel pending bookings.";
                return RedirectToAction(nameof(MyBookings));
            }

            booking.BookingStatus = "Cancelled";
            booking.PaymentStatus = "Rejected";

            await _context.SaveChangesAsync();

            TempData["BookingMessage"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(MyBookings));
        }

        [HttpGet]
        public async Task<IActionResult> GetBookingCalendarData(int apartmentId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.ApartmentId == apartmentId && b.BookingStatus != "Cancelled")
                .Select(b => new
                {
                    checkIn = b.CheckInDate,
                    checkOut = b.CheckOutDate
                })
                .ToListAsync();

            var unavailableDates = new HashSet<string>();
            var startDates = new HashSet<string>();
            var endDates = new HashSet<string>();

            foreach (var booking in bookings)
            {
                var checkIn = booking.checkIn.Date;
                var checkOut = booking.checkOut.Date;

                startDates.Add(checkIn.ToString("yyyy-MM-dd"));
                endDates.Add(checkOut.ToString("yyyy-MM-dd"));

                for (var date = checkIn; date < checkOut; date = date.AddDays(1))
                {
                    unavailableDates.Add(date.ToString("yyyy-MM-dd"));
                }
            }

            return Json(new
            {
                unavailableDates,
                startDates,
                endDates
            });
        
        }
    }

}
    
    