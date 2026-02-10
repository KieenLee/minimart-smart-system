namespace MS2.Models.DTOs.Order;

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public string OrderType { get; set; } = "POS"; // "POS" or "Online"
    public string? Notes { get; set; }
    public List<CreateOrderDetailDto> OrderDetails { get; set; } = new();
}