using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Mapster;

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
            return View(new CreateBrandVM());
        }

        [HttpPost]
        public IActionResult Create(CreateBrandVM createBrandVM)
        {
            if (!ModelState.IsValid)
            {
                return View(createBrandVM);
            }

            //Brand brand = new()
            //{
            //    Name = createBrandVM.Name,
            //    Status = createBrandVM.Status,
            //    Description = createBrandVM.Description,
            //};

            Brand brand = createBrandVM.Adapt<Brand>();

            if (createBrandVM.Img is not null && createBrandVM.Img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBrandVM.Img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                using(var stream = System.IO.File.Create(filePath))
                {
                    createBrandVM.Img.CopyTo(stream);
                }

                // Save Img in db
                brand.Img = fileName;
            }

            // Save brand in db
            _context.Brands.Add(brand);
            _context.SaveChanges();

            //Response.Cookies.Append("success-notification", "Add Brand Successfully");
            TempData["success-notification"] = "Add Brand Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            //return View(new UpdateBrandVM()
            //{
            //    Id = brand.Id,
            //    Name = brand.Name,
            //    Description = brand.Description,
            //    Status = brand.Status,
            //    Img = brand.Img,
            //});

            return View(brand.Adapt<UpdateBrandVM>());
        }

        [HttpPost]
        public IActionResult Edit(UpdateBrandVM updateBrandVM)
        {
            if (!ModelState.IsValid)
            {
                return View(updateBrandVM);
            }

            var brandInDb = _context.Brands.AsNoTracking().FirstOrDefault(e => e.Id == updateBrandVM.Id);
            if(brandInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            //Brand brand = new()
            //{
            //    Id = updateBrandVM.Id,
            //    Name = updateBrandVM.Name,
            //    Status = updateBrandVM.Status,
            //    Description = updateBrandVM.Description,
            //};

            Brand brand = updateBrandVM.Adapt<Brand>();

            if (updateBrandVM.NewImg is not null)
            {
                if(updateBrandVM.NewImg.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateBrandVM.NewImg.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        updateBrandVM.NewImg.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", brandInDb.Img);
                    if(System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    brand.Img = fileName;
                }
            }
            else
            {
                brand.Img = brandInDb.Img;
            }

            _context.Brands.Update(brand);
            _context.SaveChanges();

            TempData["success-notification"] = "Update Brand Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var brand = _context.Brands.FirstOrDefault(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", brand.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Brands.Remove(brand);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Brand Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
