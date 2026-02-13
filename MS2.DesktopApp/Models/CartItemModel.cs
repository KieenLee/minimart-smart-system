using System.Text.Json.Serialization;

namespace MS2.DesktopApp.Models;

/// <summary>
/// Local model cho Cart Item (không phải Entity)
/// </summary>
public class CartItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    [JsonIgnore]
    public decimal Subtotal => UnitPrice * Quantity;
}
