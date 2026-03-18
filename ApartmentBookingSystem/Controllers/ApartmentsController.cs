using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmentBookingSystem.Data;
using ApartmentBookingSystem.Models;

namespace ApartmentBookingSystem.Areas.Admin.Controllers
{
    //[Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ApartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Apartments
        public async Task<IActionResult> Index()
        {
            var apartments = await _context.Apartments
                .Include(a => a.Images)
                .ToListAsync();

            return View(apartments);
        }

        // GET: Admin/Apartments/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Apartment apartment, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // حفظ الشقة الأول
                _context.Add(apartment);
                await _context.SaveChangesAsync();

                // رفع الصور
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var apartmentImage = new ApartmentImage
                            {
                                ApartmentId = apartment.Id,
                                ImageUrl = "/images/" + fileName
                            };

                            _context.Add(apartmentImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(apartment);
        }


        // GET: Admin/Apartments/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var apartment = await _context.Apartments
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
                return NotFound();

            return View(apartment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Apartment apartment, List<IFormFile> imageFiles, List<int> deletedImageIds)
        {
            if (id != apartment.Id)
                return NotFound();

            var existingApartment = await _context.Apartments
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingApartment == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingApartment.Title = apartment.Title;
                    existingApartment.Description = apartment.Description;
                    existingApartment.PricePerNight = apartment.PricePerNight;
                    existingApartment.ApartmentSize = apartment.ApartmentSize;
                    existingApartment.Rooms = apartment.Rooms;
                    existingApartment.Bathrooms = apartment.Bathrooms;
                    existingApartment.MaxGuests = apartment.MaxGuests;
                    existingApartment.City = apartment.City;
                    existingApartment.Area = apartment.Area;
                    existingApartment.Address = apartment.Address;
                    existingApartment.LocationUrl = apartment.LocationUrl;
                    existingApartment.VideoUrl = apartment.VideoUrl;

                    if (deletedImageIds != null && deletedImageIds.Count > 0)
                    {
                        var imagesToDelete = existingApartment.Images
                            .Where(img => deletedImageIds.Contains(img.Id))
                            .ToList();

                        foreach (var image in imagesToDelete)
                        {
                            var filePath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                image.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                            );

                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            _context.Set<ApartmentImage>().Remove(image);
                        }
                    }

                    if (imageFiles != null && imageFiles.Count > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        foreach (var file in imageFiles)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                var apartmentImage = new ApartmentImage
                                {
                                    ApartmentId = existingApartment.Id,
                                    ImageUrl = "/images/" + fileName
                                };

                                _context.Set<ApartmentImage>().Add(apartmentImage);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    existingApartment.Images = await _context.Set<ApartmentImage>()
                        .Where(i => i.ApartmentId == existingApartment.Id)
                        .ToListAsync();

                    return View(existingApartment);
                }
            }

            apartment.Images = await _context.Set<ApartmentImage>()
                .Where(i => i.ApartmentId == apartment.Id)
                .ToListAsync();

            return View(apartment);
        }

        // GET: Admin/Apartments/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var apartment = await _context.Apartments
                .FirstOrDefaultAsync(m => m.Id == id);

            if (apartment == null)
                return NotFound();

            return View(apartment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var apartment = await _context.Apartments
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
                return NotFound();

            // ✅ check لو فيه bookings
            bool hasBookings = await _context.Bookings
                .AnyAsync(b => b.ApartmentId == id);

            if (hasBookings)
            {
                TempData["Error"] = "لا يمكن حذف الشقة لأنها تحتوي على حجوزات.";
                return RedirectToAction(nameof(Index));
            }

            // 🧹 حذف الصور من السيرفر
            foreach (var image in apartment.Images)
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot" + image.ImageUrl
                );

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Apartments.Remove(apartment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الشقة بنجاح.";
            return RedirectToAction(nameof(Index));
        }


    }
}