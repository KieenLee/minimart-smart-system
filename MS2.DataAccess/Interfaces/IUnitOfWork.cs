namespace MS2.DataAccess.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    ICartItemRepository CartItems { get; }

    // Transaction methods
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}