using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Interfaces;
using MS2.WebApp.Models;

namespace MS2.WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            const int pageSize = 12;

            // Lấy tất cả sản phẩm active
            var query = (await _unitOfWork.Products.GetAllAsync())
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filter theo search keyword
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Filter theo category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Pagination
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Lấy categories cho filter
            var categories = await _unitOfWork.Categories.GetAllAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Categories = categories,
                SearchKeyword = search,
                SelectedCategoryId = categoryId,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id.Value);

            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            // Lấy category
            var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

            // Lấy sản phẩm liên quan (cùng category, max 4 sản phẩm)
            var relatedProducts = (await _unitOfWork.Products.GetAllAsync())
                .Where(p => p.CategoryId == product.CategoryId
                         && p.Id != product.Id
                         && p.IsActive)
                .Take(4)
                .ToList();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Category = category,
                RelatedProducts = relatedProducts
            };

            return View(viewModel);
        }

        // GET: Products/Search
        public async Task<IActionResult> Search(string keyword)
        {
            return RedirectToAction("Index", new { search = keyword });
        }
    }
}
