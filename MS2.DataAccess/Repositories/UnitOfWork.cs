using Microsoft.EntityFrameworkCore.Storage;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;

namespace MS2.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MS2DbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization for repositories
    private IProductRepository? _products;
    private IOrderRepository? _orders;
    private IUserRepository? _users;
    private ICategoryRepository? _categories;
    private ICartItemRepository? _cartItems;

    public UnitOfWork(MS2DbContext context)
    {
        _context = context;
    }

    public IProductRepository Products
    {
        get { return _products ??= new ProductRepository(_context); }
    }

    public IOrderRepository Orders
    {
        get { return _orders ??= new OrderRepository(_context); }
    }

    public IUserRepository Users
    {
        get { return _users ??= new UserRepository(_context); }
    }

    public ICategoryRepository Categories
    {
        get { return _categories ??= new CategoryRepository(_context); }
    }

    public ICartItemRepository CartItems
    {
        get { return _cartItems ??= new CartItemRepository(_context); }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}