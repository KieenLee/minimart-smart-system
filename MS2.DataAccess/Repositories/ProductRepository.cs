using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;

namespace MS2.DataAccess.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(MS2DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Barcode == barcode && p.IsActive);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string keyword)
    {
        return await _dbSet
            .Where(p => (p.Name.Contains(keyword) ||
                        (p.Description != null && p.Description.Contains(keyword)) ||
                        (p.Barcode != null && p.Barcode.Contains(keyword)))
                        && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _dbSet
            .Where(p => p.Stock <= threshold && p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return await _dbSet.AnyAsync(p => p.Barcode == barcode && p.Id != excludeId.Value);
        }
        return await _dbSet.AnyAsync(p => p.Barcode == barcode);
    }
}