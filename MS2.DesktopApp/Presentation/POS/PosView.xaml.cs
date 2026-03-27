using MS2.DesktopApp.Models;
using MS2.Models.DTOs.Product;
using System.Windows;
using System.Windows.Controls;

namespace MS2.DesktopApp.Presentation.POS;

public partial class PosView : UserControl
{
    public PosView()
    {
        InitializeComponent();
    }

    private void AddToCartButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;

        // B1: Lấy quantity từ Tag (mặc định 1 nếu không có)
        if (!int.TryParse(button.Tag?.ToString(), out int quantity))
            quantity = 1;

        // B2: Lấy ProductDto từ DataContext của nút (được Grid truyền vào)
        if (button.DataContext is ProductDto product && DataContext is PosViewModel viewModel)
        {
            viewModel.AddToCartWithQuantityCommand.Execute((product, quantity));
        }
    }
}
