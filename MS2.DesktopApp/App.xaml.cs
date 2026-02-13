using System.Windows;
using System.IO;
using Microsoft.Extensions.Configuration;
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
    public ServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Setup Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Setup DI Container
        var services = new ServiceCollection();

        // Register Configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Register TcpClientSettings from appsettings.json
        var tcpSettings = configuration.GetSection("TcpClient").Get<TcpClientSettings>()
                          ?? new TcpClientSettings();
        services.AddSingleton(tcpSettings);

        // Register Network Service (Singleton - dùng chung 1 connection)
        services.AddSingleton<TcpClientService>();

        // Register ViewModels (Transient - mỗi lần tạo mới)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();

        // Register Views (Transient)
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // Show LoginWindow với DataContext
        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        var loginViewModel = ServiceProvider.GetRequiredService<LoginViewModel>();
        loginWindow.DataContext = loginViewModel;
        loginWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanup
        ServiceProvider?.Dispose();
        base.OnExit(e);
    }
}

