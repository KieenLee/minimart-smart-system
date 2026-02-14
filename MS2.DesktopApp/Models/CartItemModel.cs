using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace MS2.DesktopApp.Models;

/// <summary>
/// Local model cho Cart Item (không phải Entity)
/// </summary>
public partial class CartItemModel : ObservableObject
{
    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName = null!;

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private int quantity;

    [JsonIgnore]
    public decimal Subtotal => UnitPrice * Quantity;

    // Notify Subtotal khi Quantity hoặc UnitPrice thay đổi
    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }
}
