using ECommerce.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class BrandController : Controller
    {
        //ApplicationDbContext _context = new();
        private readonly IRepository<Brand> _brandRepository;// = new();

        public BrandController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

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
        public async Task<IActionResult> Create(CreateBrandVM createBrandVM, CancellationToken cancellationToken)
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
            await _brandRepository.AddAsync(brand, cancellationToken);
            await _brandRepository.CommitAsync(cancellationToken);

            //Response.Cookies.Append("success-notification", "Add Brand Successfully");
            TempData["success-notification"] = "Add Brand Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

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
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(UpdateBrandVM updateBrandVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(updateBrandVM);
            }

            var brandInDb = await _brandRepository.GetOneAsync(e => e.Id == updateBrandVM.Id, tracked: false);
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

            _brandRepository.Update(brand);
            await _brandRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Update Brand Successfully";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brand is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", brand.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _brandRepository.Delete(brand);
            await _brandRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Delete Brand Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
