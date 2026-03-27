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
    private int maxQuantity;

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            int newVal = value;
            if (MaxQuantity > 0 && newVal > MaxQuantity) newVal = MaxQuantity;
            if (newVal < 1) newVal = 1;
            
            if (SetProperty(ref _quantity, newVal))
            {
                OnPropertyChanged(nameof(Subtotal));
                QuantityChangedCallback?.Invoke();
            }
        }
    }

    [JsonIgnore]
    public Action? QuantityChangedCallback { get; set; }

    [JsonIgnore]
    public decimal Subtotal => UnitPrice * Quantity;

    // Notify Subtotal khi UnitPrice thay đổi

    partial void OnUnitPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(Subtotal));
    }
}
