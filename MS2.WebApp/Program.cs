using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.DataAccess.Repositories;
using MS2.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== RAZOR PAGES =====
builder.Services.AddRazorPages();

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

// ===== RAZOR PAGES ROUTING =====
app.MapRazorPages();

app.Run();
