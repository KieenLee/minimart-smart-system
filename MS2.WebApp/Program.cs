using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

// ===== BUSINESS SERVICES =====
// Sẽ tạo sau trong Services folder
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IProductService, ProductService>();
// builder.Services.AddScoped<ICartService, CartService>();
// builder.Services.AddScoped<IOrderService, OrderService>();

// ===== SESSION CONFIGURATION =====
builder.Services.AddDistributedMemoryCache(); // In-memory cache for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true; // Bảo mật, không cho JS truy cập
    options.Cookie.IsEssential = true; // Bắt buộc phải có
});

// ===== HTTP CONTEXT ACCESSOR =====
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(); // Serve wwwroot files (CSS, JS, images)

app.UseRouting();

// ===== SESSION MIDDLEWARE =====
app.UseSession();

app.UseAuthorization();

// ===== DEFAULT ROUTE =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
