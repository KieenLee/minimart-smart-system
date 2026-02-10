using MS2.Models.Entities;

namespace MS2.DataAccess.Interfaces;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId);
    Task<CartItem?> GetByUserAndProductAsync(int userId, int productId);
    Task DeleteByUserIdAsync(int userId);
    Task<int> GetCartItemCountAsync(int userId);
}