using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        ApplicationDbContext _context = new();

        public IActionResult Index(FilterProductVM filterProductVM, int page = 1)
        {
            const decimal discount = 50;
            var products = _context.Products.AsNoTracking().AsQueryable();

            // Add Filter
            products = products.Include(e => e.Category).Include(e => e.Brand);

            #region Filter Product
            // Add Filter 
            if (filterProductVM.name is not null)
            {
                products = products.Where(e => e.Name.Contains(filterProductVM.name.Trim()));
                ViewBag.name = filterProductVM.name;
            }

            if (filterProductVM.minPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discount / 100 > filterProductVM.minPrice);
                ViewBag.minPrice = filterProductVM.minPrice;
            }

            if (filterProductVM.maxPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discount / 100 < filterProductVM.maxPrice);
                ViewBag.maxPrice = filterProductVM.maxPrice;
            }

            if (filterProductVM.categoryId is not null)
            {
                products = products.Where(e => e.CategoryId == filterProductVM.categoryId);
                ViewBag.categoryId = filterProductVM.categoryId;
            }

            if (filterProductVM.brandId is not null)
            {
                products = products.Where(e => e.BrandId == filterProductVM.brandId);
                ViewBag.brandId = filterProductVM.brandId;
            }

            if (filterProductVM.isHot)
            {
                products = products.Where(e => e.Discount > discount);
                ViewBag.isHot = filterProductVM.isHot;
            }

            // Categories
            var categories = _context.Categories;
            //ViewData["categories"] = categories.AsEnumerable();
            ViewBag.categories = categories.AsEnumerable();

            // Brands
            var brands = _context.Brands;
            ViewData["brands"] = brands.AsEnumerable();
            #endregion

            #region Pagination
            // Pagination
            ViewBag.TotalPages = Math.Ceiling(products.Count() / 8.0);
            ViewBag.CurrentPage = page;
            products = products.Skip((page - 1) * 8).Take(8); // 0 .. 8 
            #endregion

            return View(products.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Categories
            var categories = _context.Categories;
            // Brands
            var brands = _context.Brands;

            return View(new ProductVM
            {
                Categories = categories.AsEnumerable(),
                Brands = brands.AsEnumerable(),
            });
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile img, List<IFormFile>? subImgs)
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
                product.MainImg = fileName;
            }

            // Save product in db
            var productCreated = _context.Products.Add(product);
            _context.SaveChanges();

            if(subImgs is not null && subImgs.Count > 0)
            {
                foreach (var item in subImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    _context.ProductSubImages.Add(new()
                    {
                        Img = fileName,
                        ProductId = productCreated.Entity.Id,
                    });
                }

                _context.SaveChanges();
            }

            //Response.Cookies.Append("Notification", "Add Product Successfully");
            TempData["Notification"] = "Add Product Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(e => e.Id == id);

            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Categories
            var categories = _context.Categories;
            // Brands
            var brands = _context.Brands;

            return View(new ProductVM
            {
                Categories = categories.AsEnumerable(),
                Brands = brands.AsEnumerable(),
                Product = product,
            });
        }

        [HttpPost]
        public IActionResult Edit(Product product, IFormFile? img)
        {
            var productInDb = _context.Products.AsNoTracking().FirstOrDefault(e => e.Id == product.Id);
            if(productInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (img is not null)
            {
                if(img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", productInDb.MainImg);
                    if(System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    product.MainImg = fileName;
                }
            }
            else
            {
                product.MainImg = productInDb.MainImg;
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            TempData["Notification"] = "Update Product Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.FirstOrDefault(e => e.Id == id);

            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["Notification"] = "Delete Product Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
