using System.Diagnostics;
using ECommerce.Models;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _context = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(FilterProductVM filterProductVM, int page = 1)
        {
            const decimal discount = 50;
            var products = _context.Products/*.AsNoTracking()*/.Include(e=>e.Category).AsQueryable();

            // Add Filter 
            if(filterProductVM.name is not null)
            {
                products = products.Where(e => e.Name.Contains(filterProductVM.name));
                ViewBag.name = filterProductVM.name;
            }

            if(filterProductVM.minPrice is not null)
            {
                products = products.Where(e => e.Price - (e.Price * e.Discount / 100) > filterProductVM.minPrice);
                ViewBag.minPrice = filterProductVM.minPrice;
            }

            if (filterProductVM.maxPrice is not null)
            {
                products = products.Where(e => e.Price - (e.Price * e.Discount / 100) < filterProductVM.maxPrice);
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

            // Pagination
            ViewBag.TotalPages = Math.Ceiling(products.Count() / 8.0);
            ViewBag.CurrentPage = page;
            products = products.Skip(((page - 1) * 8)).Take(8); // 0 .. 8

            return View(products.AsEnumerable());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ViewResult Welcome()
        {
            return View();
        }

        public ViewResult PersonalInfo(int id)
        {
            List<Person> persons = new List<Person>();
            persons.AddRange(new()
            {
                Id = 1,
                Name = "Mohamed",
                Salary = 1000,
                Address = "Caio"
            }, new()
            {
                Id = 2,
                Name = "Ali",
                Salary = 2000,
                Address = "Alex"
            });

            // Logic

            var personsInDB = persons.AsQueryable();
            var totalPersons = persons.Count();

            // Add Filter
            personsInDB = personsInDB.Where(e => e.Id == id);

            return View(new PersonVM
            {
                Persons = personsInDB.ToList(),
                TotalPerson = totalPersons
            });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
