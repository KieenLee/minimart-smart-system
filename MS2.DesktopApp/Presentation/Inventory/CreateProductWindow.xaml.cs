using System.Windows;

namespace MS2.DesktopApp.Presentation.Inventory;

public partial class CreateProductWindow : Window
{
    public string ProductName => TxtName.Text.Trim();
    public decimal Price => decimal.TryParse(TxtPrice.Text, out var p) ? p : 0;
    public int Stock => int.TryParse(TxtStock.Text, out var s) ? s : 0;
    public string Barcode => TxtBarcode.Text.Trim();
    public bool IsConfirmed { get; private set; }

    // DataContext cho Categories & SelectedCategoryId được bind từ ViewModel
    private readonly Models.InventoryViewModel _vm;

    public CreateProductWindow(Models.InventoryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            ShowError("Vui lòng nhập tên sản phẩm!");
            return;
        }
        if (Price <= 0)
        {
            ShowError("Giá bán phải lớn hơn 0!");
            return;
        }
        if (_vm.SelectedCategoryId == 0)
        {
            ShowError("Vui lòng chọn danh mục!");
            return;
        }

        IsConfirmed = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }

    private void ShowError(string msg)
    {
        TxtError.Text = msg;
        ErrorBorder.Visibility = Visibility.Visible;
    }
}
