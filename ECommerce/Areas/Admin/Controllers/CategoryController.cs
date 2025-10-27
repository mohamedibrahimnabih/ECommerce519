﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE},{SD.EMPLOYEE_ROLE}")]
    public class CategoryController : Controller
    {
        //ApplicationDbContext _context = new();
        //Repository<Category> _categoryRepository = new();
        private readonly IRepository<Category> _categoryRepository;// = new Repository<Category>();

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

            // Add Filter

            return View(categories.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _categoryRepository.CommitAsync(cancellationToken);

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(Category category, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                //ModelState.AddModelError(string.Empty, "Any More Errors");

                return View(category);
            }

            _categoryRepository.Update(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);

            if (category is null)
                return RedirectToAction("NotFoundPage", "Home");

            _categoryRepository.Delete(category);
            await _categoryRepository.CommitAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }
    }
}
