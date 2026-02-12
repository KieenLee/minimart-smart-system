using MS2.Models.TCP;

namespace MS2.ServerApp.Business.Interfaces
{
    public interface IProductService
    {
        // Lấy tất cả sản phẩm
        Task<TcpResponse> GetProductsAsync(TcpMessage message);

        // Tìm kiếm sản phẩm theo keyword
        Task<TcpResponse> SearchProductsAsync(TcpMessage message);

        // Lấy sản phẩm theo barcode (cho máy quét)
        Task<TcpResponse> GetProductByBarcodeAsync(TcpMessage message);

        // Cập nhật giá sản phẩm
        Task<TcpResponse> UpdateProductPriceAsync(TcpMessage message);

        // Cập nhật tồn kho
        Task<TcpResponse> UpdateProductStockAsync(TcpMessage message);

        // Lấy danh sách sản phẩm tồn kho thấp
        Task<TcpResponse> GetLowStockProductsAsync(TcpMessage message);
    }
}