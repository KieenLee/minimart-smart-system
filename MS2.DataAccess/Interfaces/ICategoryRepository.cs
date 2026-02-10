using MS2.Models.Entities;

namespace MS2.DataAccess.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId);
    Task<Category?> GetWithProductsAsync(int categoryId);
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}