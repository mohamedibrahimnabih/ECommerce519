using ECommerce.Models;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        ApplicationDbContext _context = new();
        ProductRepository _productRepository = new();
        Repository<Category> _categoryRepository = new();
        Repository<Brand> _brandRepository = new();
        Repository<ProductSubImage> _productSubImageRepository = new();
        ProductColorRepository _productColorRepository = new();

        public async Task<IActionResult> Index(FilterProductVM filterProductVM, CancellationToken cancellationToken, int page = 1)
        {
            const decimal discount = 50;
            var products = await _productRepository.GetAsync(includes: [e => e.Category, e => e.Brand], tracked: false, cancellationToken: cancellationToken);

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

            if (filterProductVM.lessQuantity)
            {
                products = products.OrderBy(e=>e.Quantity);
                ViewBag.lessQuantity = filterProductVM.lessQuantity;
            }

            // Categories
            var categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            //ViewData["categories"] = categories.AsEnumerable();
            ViewBag.categories = categories.AsEnumerable();

            // Brands
            var brands = await _brandRepository.GetAsync(cancellationToken: cancellationToken);
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
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
        {
            // Categories
            var categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            // Brands
            var brands = await _brandRepository.GetAsync(cancellationToken: cancellationToken);

            return View(new ProductVM
            {
                Categories = categories.AsEnumerable(),
                Brands = brands.AsEnumerable(),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile img, List<IFormFile>? subImgs, string[] colors, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                if (img is not null && img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Save Img in db
                    product.MainImg = fileName;
                }

                // Save product in db
                var productCreated =  await _productRepository.AddAsync(product, cancellationToken: cancellationToken);
                await _productRepository.CommitAsync(cancellationToken);

                if (subImgs is not null && subImgs.Count > 0)
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

                        await _productSubImageRepository.AddAsync(new()
                        {
                            Img = fileName,
                            ProductId = productCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }

                    await _productSubImageRepository.CommitAsync(cancellationToken);
                }

                if (colors.Any())
                {
                    foreach (var item in colors)
                    {
                        await _productColorRepository.AddAsync(new()
                        {
                            Color = item,
                            ProductId = productCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }

                    await _productColorRepository.CommitAsync(cancellationToken);
                }

                //Response.Cookies.Append("success-notification", "Add Product Successfully");
                TempData["success-notification"] = "Add Product Successfully";

                transaction.Commit();
            }
            catch(Exception ex)
            {
                // Logging
                TempData["error-notification"] = "Error While Saving the product";

                transaction.Rollback();

                // Validation
            }

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.productSubImages, e => e.ProductColors], tracked: false, cancellationToken: cancellationToken);

            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Categories
            var categories = await _categoryRepository.GetAsync(cancellationToken: cancellationToken);
            // Brands
            var brands = await _brandRepository.GetAsync(cancellationToken: cancellationToken);

            return View(new ProductVM
            {
                Categories = categories.AsEnumerable(),
                Brands = brands.AsEnumerable(),
                Product = product,
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? img, List<IFormFile>? subImgs, string[] colors, CancellationToken cancellationToken)
        {
            var productInDb = await _productRepository.GetOneAsync(e => e.Id == product.Id, includes: [e => e.ProductColors], tracked: false, cancellationToken: cancellationToken);
            if (productInDb is null)
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

            await _productRepository.AddAsync(product, cancellationToken: cancellationToken);
            await _productRepository.CommitAsync(cancellationToken);

            if (subImgs is not null && subImgs.Count > 0)
            {
                product.productSubImages = new List<ProductSubImage>();

                foreach (var item in subImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    product.productSubImages.Add(new()
                    {
                        Img = fileName,
                        ProductId = product.Id,
                    });
                }

                await _productSubImageRepository.CommitAsync(cancellationToken);
            }


            if (colors.Any())
            {
                _productColorRepository.RemoveRange(productInDb.ProductColors);

                product.ProductColors = new List<ProductColor>();

                foreach (var item in colors)
                {
                    product.ProductColors.Add(new()
                    {
                        Color = item,
                        ProductId = product.Id,
                    });
                }

                await _productColorRepository.CommitAsync(cancellationToken);
            }

            TempData["success-notification"] = "Update Product Successfully";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.productSubImages, e => e.ProductColors], tracked: false, cancellationToken: cancellationToken);

            if (product is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            foreach (var item in product.productSubImages)
            {
                var subImgOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                {
                    System.IO.File.Delete(subImgOldPath);
                }
            }


            _productRepository.Delete(product);
            await _productRepository.CommitAsync(cancellationToken);

            TempData["success-notification"] = "Delete Product Successfully";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteSubImg(int productId, string Img, CancellationToken cancellationToken)
        {
            var productSubImgInDb = await _productSubImageRepository.GetOneAsync(e => e.ProductId == productId && e.Img == Img);

            if(productSubImgInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", productSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _productSubImageRepository.Delete(productSubImgInDb);
            await _productSubImageRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Edit), new { id = productId });
        }
    }
}
