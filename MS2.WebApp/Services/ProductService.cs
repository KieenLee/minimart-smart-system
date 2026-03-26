using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;

namespace MS2.WebApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Product>> GetProductsAsync(string? search = null, int? categoryId = null)
        {
            var products = await _unitOfWork.Products.GetAllAsync();

            // Only active products with images or not – show all active ones
            var query = products.Where(p => p.IsActive == true).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(keyword)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(keyword)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            return query.OrderBy(p => p.Name).ToList();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _unitOfWork.Products.GetByIdAsync(productId);
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var cats = await _unitOfWork.Categories.GetAllAsync();
            return cats.OrderBy(c => c.Name).ToList();
        }

        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            var cats = await _unitOfWork.Categories.GetRootCategoriesAsync();
            return cats.OrderBy(c => c.Name).ToList();
        }
    }
}
