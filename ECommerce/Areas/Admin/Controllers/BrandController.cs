using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var brands = _context.Brands.AsNoTracking().AsQueryable();

            // Add Filter

            return View(brands.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status,
            }).AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Brand brand, IFormFile img)
        {
            if(img is not null && img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                using(var stream = System.IO.File.Create(filePath))
                {
                    img.CopyTo(stream);
                }

                // Save Img in db
                brand.Img = fileName;
            }

            // Save brand in db
            _context.Brands.Add(brand);
            _context.SaveChanges();

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(brand);
        }

        [HttpPost]
        public IActionResult Edit(Brand brand)
        {
            _context.Brands.Update(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            _context.Brands.Remove(brand);
            _context.SaveChanges();
                
            return RedirectToAction(nameof(Index));
        }
    }
}
