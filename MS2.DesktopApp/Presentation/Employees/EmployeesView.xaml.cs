using MS2.DesktopApp.Models;
using MS2.Models.DTOs.Auth;
using System.Windows;
using System.Windows.Controls;

namespace MS2.DesktopApp.Presentation.Employees;

public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();
    }

    private void CreateEmployeeButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not EmployeesViewModel viewModel)
            return;

        // Create dialog window
        var dialog = new Window
        {
            Title = "Tạo Nhân Viên Mới",
            Width = 500,
            Height = 550,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        // Create form
        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Username
        var lblUsername = new TextBlock { Text = "Username *", Margin = new Thickness(0, 0, 0, 5) };
        var txtUsername = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtUsername.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewUsername") { Mode = System.Windows.Data.BindingMode.TwoWay });
        Grid.SetRow(lblUsername, 0);
        Grid.SetRow(txtUsername, 1);

        // Password
        var lblPassword = new TextBlock { Text = "Password *", Margin = new Thickness(0, 0, 0, 5) };
        var txtPassword = new PasswordBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtPassword.PasswordChanged += (s, args) => viewModel.NewPassword = txtPassword.Password;
        Grid.SetRow(lblPassword, 2);
        Grid.SetRow(txtPassword, 3);

        // Full Name
        var lblFullName = new TextBlock { Text = "Họ và tên *", Margin = new Thickness(0, 0, 0, 5) };
        var txtFullName = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtFullName.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewFullName") { Mode = System.Windows.Data.BindingMode.TwoWay });
        Grid.SetRow(lblFullName, 4);
        Grid.SetRow(txtFullName, 5);

        // Email
        var lblEmail = new TextBlock { Text = "Email *", Margin = new Thickness(0, 0, 0, 5) };
        var txtEmail = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtEmail.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewEmail") { Mode = System.Windows.Data.BindingMode.TwoWay });
        Grid.SetRow(lblEmail, 6);
        Grid.SetRow(txtEmail, 7);

        // Phone
        var lblPhone = new TextBlock { Text = "Số điện thoại", Margin = new Thickness(0, 0, 0, 5) };
        var txtPhone = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtPhone.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewPhone") { Mode = System.Windows.Data.BindingMode.TwoWay });

        // Address
        var lblAddress = new TextBlock { Text = "Địa chỉ", Margin = new Thickness(0, 0, 0, 5) };
        var txtAddress = new TextBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        txtAddress.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("NewAddress") { Mode = System.Windows.Data.BindingMode.TwoWay });

        // Role
        var lblRole = new TextBlock { Text = "Vai trò *", Margin = new Thickness(0, 0, 0, 5) };
        var cboRole = new ComboBox { Height = 30, Margin = new Thickness(0, 0, 0, 15) };
        cboRole.Items.Add(new ComboBoxItem { Content = "Employee" });
        cboRole.Items.Add(new ComboBoxItem { Content = "Admin" });
        cboRole.SelectedIndex = 0;
        cboRole.SelectionChanged += (s, args) =>
        {
            if (cboRole.SelectedItem is ComboBoxItem item)
                viewModel.NewRole = item.Content.ToString() ?? "Employee";
        };

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };
        Grid.SetRow(buttonPanel, 9);

        var btnCreate = new Button
        {
            Content = "TẠO MỚI",
            Width = 100,
            Height = 35,
            Margin = new Thickness(0, 0, 10, 0),
            Background = System.Windows.Media.Brushes.Green,
            Foreground = System.Windows.Media.Brushes.White,
            FontWeight = FontWeights.Bold
        };
        btnCreate.Click += async (s, args) =>
        {
            await viewModel.CreateEmployeeCommand.ExecuteAsync(null);
            dialog.Close();
        };

        var btnCancel = new Button
        {
            Content = "HỦY",
            Width = 100,
            Height = 35,
            Background = System.Windows.Media.Brushes.Gray,
            Foreground = System.Windows.Media.Brushes.White
        };
        btnCancel.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(btnCreate);
        buttonPanel.Children.Add(btnCancel);

        // Add all controls to grid
        grid.Children.Add(lblUsername);
        grid.Children.Add(txtUsername);
        grid.Children.Add(lblPassword);
        grid.Children.Add(txtPassword);
        grid.Children.Add(lblFullName);
        grid.Children.Add(txtFullName);
        grid.Children.Add(lblEmail);
        grid.Children.Add(txtEmail);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;
        dialog.ShowDialog();
    }
}
