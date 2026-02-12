using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MS2.DesktopApp.Models;
using MS2.DesktopApp.Network;
using MS2.DesktopApp.Presentation;

namespace MS2.DesktopApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup DI Container
        var services = new ServiceCollection();

        // Register Network Service (Singleton - dùng chung 1 connection)
        services.AddSingleton<TcpClientService>();

        // Register ViewModels (Transient - mỗi lần tạo mới)
        services.AddTransient<LoginViewModel>();

        // Register Views (Transient)
        services.AddTransient<LoginWindow>();

        _serviceProvider = services.BuildServiceProvider();

        // Show LoginWindow với DataContext
        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        loginWindow.DataContext = loginViewModel;
        loginWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanup
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

