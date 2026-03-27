using MS2.DesktopApp.Models;
using System.Windows;

namespace MS2.DesktopApp.Presentation.Orders;

public partial class OrderDetailWindow : Window
{
    public OrderDetailWindow(OnlineOrderDto order)
    {
        InitializeComponent();
        DataContext = order;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
