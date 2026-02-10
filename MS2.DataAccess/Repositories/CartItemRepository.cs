using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;

namespace MS2.DataAccess.Repositories;

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(MS2DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p.Category)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CartItem?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _dbSet
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
    }

    public async Task DeleteByUserIdAsync(int userId)
    {
        var cartItems = await _dbSet
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _dbSet.RemoveRange(cartItems);
    }

    public async Task<int> GetCartItemCountAsync(int userId)
    {
        return await _dbSet.CountAsync(c => c.UserId == userId);
    }
}