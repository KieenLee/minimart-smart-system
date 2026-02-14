using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.DataAccess.Repositories;
using MS2.ServerApp.Business.Interfaces;
using MS2.ServerApp.Business.Services;
using MS2.ServerApp.Models;
using MS2.ServerApp.Network;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 2. Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// 3. TcpSettings
var tcpSettings = builder.Configuration.GetSection("TcpSettings").Get<TcpSettings>()
    ?? throw new Exception("TcpSettings not found in appsettings.json");
builder.Services.AddSingleton(tcpSettings);

// 4. DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found");
builder.Services.AddDbContext<MS2DbContext>(options =>
    options.UseSqlServer(connectionString));

// 5. Repositories & UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 6. Business Services
builder.Services.AddSingleton<ISessionManager, SessionManager>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();

// 7. Network Layer
builder.Services.AddSingleton<TcpMessageRouter>();
builder.Services.AddSingleton<TcpServer>();

var host = builder.Build();

// 8. Start TCP Server
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var tcpServer = host.Services.GetRequiredService<TcpServer>();

logger.LogInformation("==============================================");
logger.LogInformation(" MS2 TCP SERVER STARTING...");
logger.LogInformation("==============================================");

var cts = new CancellationTokenSource();

// Handle Ctrl+C for graceful shutdown
Console.CancelKeyPress += async (sender, e) =>
{
    e.Cancel = true;
    logger.LogInformation(" Shutdown signal received...");
    cts.Cancel();
    await tcpServer.StopAsync();
    logger.LogInformation(" Server stopped gracefully");
};

try
{
    await tcpServer.StartAsync(cts.Token);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error in TCP Server");
}
finally
{
    await host.StopAsync();
}
