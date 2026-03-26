using MS2.Models.Entities;

namespace MS2.WebApp.Services
{
    public interface IProductService
    {
        /// <summary>Get all active products (IsActive=true), optionally filtered by search keyword and category.</summary>
        Task<List<Product>> GetProductsAsync(string? search = null, int? categoryId = null);

        /// <summary>Get a single product by ID. Returns null if not found.</summary>
        Task<Product?> GetProductByIdAsync(int productId);

        /// <summary>Get all categories (for filter dropdown).</summary>
        Task<List<Category>> GetCategoriesAsync();

        /// <summary>Get root categories only (ParentCategoryId == null).</summary>
        Task<List<Category>> GetRootCategoriesAsync();
    }
}
