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

        // Tìm TextBox số lượng trong cùng Grid
        var grid = button.Parent as Grid;
        if (grid == null)
            return;

        var quantityTextBox = grid.Children.OfType<TextBox>().FirstOrDefault();
        if (quantityTextBox == null)
            return;

        // Lấy product từ Tag của TextBox
        var product = quantityTextBox.Tag as ProductDto;
        if (product == null)
            return;

        // Parse số lượng
        if (!int.TryParse(quantityTextBox.Text, out int quantity) || quantity <= 0)
        {
            quantityTextBox.Text = "1";
            return;
        }

        // Gọi ViewModel để thêm vào giỏ với số lượng
        if (DataContext is PosViewModel viewModel)
        {
            viewModel.AddToCartWithQuantityCommand.Execute((product, quantity));

            // Reset số lượng về 1 sau khi thêm thành công
            quantityTextBox.Text = "1";
        }
    }
}
