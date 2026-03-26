using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.Models.Entities;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;
        private const int PageSize = 12;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> Products { get; set; } = new();
        public List<Product> PagedProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? CurrentSearch { get; set; }
        public int? CurrentCategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(string? search, int? categoryId, int page = 1)
        {
            CurrentSearch = search;
            CurrentCategoryId = categoryId;
            CurrentPage = page < 1 ? 1 : page;

            Products = await _productService.GetProductsAsync(search, categoryId);
            Categories = await _productService.GetCategoriesAsync();

            TotalPages = (int)Math.Ceiling(Products.Count / (double)PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            PagedProducts = Products
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
