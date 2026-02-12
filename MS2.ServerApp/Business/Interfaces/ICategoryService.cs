using MS2.Models.TCP;

namespace MS2.ServerApp.Business.Interfaces
{
    public interface ICategoryService
    {
        /// Lấy tất cả categories
        Task<TcpResponse> GetCategoriesAsync(TcpMessage message);

        // Lấy root categories (ParentCategoryId = null)
        Task<TcpResponse> GetRootCategoriesAsync(TcpMessage message);

        // Lấy sub categories theo ParentId
        Task<TcpResponse> GetSubCategoriesAsync(TcpMessage message);
    }
}