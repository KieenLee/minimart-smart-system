using MS2.Models.Entities;

namespace MS2.DataAccess.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<Product?> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Product>> SearchAsync(string keyword);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);
}