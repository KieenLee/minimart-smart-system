using System.Windows;
using System.Windows.Controls;
using MS2.DesktopApp.Models;

namespace MS2.DesktopApp.Presentation.Profile;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel viewModel)
        {
            viewModel.NewPassword = NewPasswordBox.Password;
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel viewModel)
        {
            viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }
}
