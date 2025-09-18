using System.Diagnostics;
using ECommerce.Models;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public ViewResult Welcome()
        {
            return View();
        }

        public ViewResult PersonalInfo()
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
