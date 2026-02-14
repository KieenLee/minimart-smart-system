using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.DataAccess.Repositories;
using MS2.ServerApp.Business.Interfaces;
using MS2.ServerApp.Business.Services;
using MS2.ServerApp.Models;
using MS2.ServerApp.Network;

var builder = new HostBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        // 1. Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. TcpSettings
        var tcpSettings = configuration.GetSection("TcpSettings").Get<TcpSettings>()
            ?? throw new Exception("TcpSettings not found in appsettings.json");
        services.AddSingleton(tcpSettings);

        // 3. DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string not found");
        services.AddDbContext<MS2DbContext>(options =>
            options.UseSqlServer(connectionString));

        // 4. Repositories & UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Business Services
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();

        // 6. Network Layer
        services.AddSingleton<TcpMessageRouter>();
        services.AddSingleton<TcpServer>();
    });

    var host = builder.Build();

    // 7. Start TCP Server
    var tcpServer = host.Services.GetRequiredService<TcpServer>();

    Console.WriteLine("==============================================");
    Console.WriteLine(" MS2 SERVER STARTING...");
    Console.WriteLine("==============================================");

    var cts = new CancellationTokenSource();

    // Handle Ctrl+C for graceful shutdown
    Console.CancelKeyPress += async (sender, e) =>
    {
        e.Cancel = true;
        Console.WriteLine("Shutdown signal received...");
        cts.Cancel();
        await tcpServer.StopAsync();
        Console.WriteLine("Server stopped gracefully");
    };

    try
    {
        await tcpServer.StartAsync(cts.Token);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in TCP Server: {ex.Message}");
    }
    finally
    {
        await host.StopAsync();
    }
