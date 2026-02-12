using System.Windows;
using System.Windows.Controls;

namespace MS2.DesktopApp.Presentation;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    // PasswordBox không hỗ trợ Binding trực tiếp, phải dùng code-behind
    private void TxtPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is Models.LoginViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
