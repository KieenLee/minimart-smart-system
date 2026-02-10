namespace MS2.Models.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public string OrderType { get; set; } = null!;
    public string? Notes { get; set; }
    public List<OrderDetailDto> OrderDetails { get; set; } = new();
}