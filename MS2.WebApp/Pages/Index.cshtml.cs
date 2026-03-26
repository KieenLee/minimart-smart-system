using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.Models.Entities;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }

        public async Task OnGetAsync()
        {
            var allProducts = await _productService.GetProductsAsync();
            FeaturedProducts = allProducts.Take(8).ToList();
            ProductCount = allProducts.Count;

            Categories = await _productService.GetRootCategoriesAsync();
            CategoryCount = Categories.Count;
        }
    }
}
