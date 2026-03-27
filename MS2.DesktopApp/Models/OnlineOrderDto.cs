namespace MS2.DesktopApp.Models;

/// <summary>
/// DTO phẳng nhận từ SignalR Hub - tránh Circular Reference khi deserialize
/// </summary>
public class OnlineOrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
    public string CustomerName { get; set; } = "";
    public string? ApproverName { get; set; }
    public int? ApproverEmpId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
