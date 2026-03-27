using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.DataAccess.Repositories;
using MS2.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== RAZOR PAGES & SIGNALR =====
builder.Services.AddRazorPages();
builder.Services.AddSignalR(); // Đăng ký SignalR

// ===== DATABASE CONFIGURATION =====
builder.Services.AddDbContext<MS2DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== REPOSITORY PATTERN - DEPENDENCY INJECTION =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();

// ===== BUSINESS SERVICES - DEPENDENCY INJECTION =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ===== BACKGROUND WORKER =====
builder.Services.AddHostedService<OrderTimeoutWorker>();

// ===== SESSION CONFIGURATION =====
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".MS2.Session";
});

// ===== HTTP CONTEXT ACCESSOR =====
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== SESSION MIDDLEWARE =====
app.UseSession();

app.UseAuthorization();

// ===== ROUTING & ENDPOINTS =====
app.MapRazorPages();
app.MapHub<MS2.WebApp.Hubs.MiniMartHub>("/minimartHub"); // Expose Hub endpoint

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
    try
    {
        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(db.Database, "ALTER TABLE Orders DROP CONSTRAINT CK__Orders__Status__52593CB8;");
    }
    catch { /* Ignore if constraint already dropped */ }
    try
    {
        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(db.Database, "ALTER TABLE Orders ADD CONSTRAINT CK_Orders_Status CHECK (Status IN ('Pending', 'Shipping', 'Completed', 'Cancelled'));");
    }
    catch { /* Ignore if constraint already exists */ }
}

app.Run();
