namespace MS2.Models.DTOs.Order;

public class CreateOrderDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }  // Nullable - nếu null sẽ lấy giá từ Product
}