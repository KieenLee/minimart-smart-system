# KẾ HOẠCH TRIỂN KHAI DỰ ÁN MS2 - MINIMART SMART SYSTEM

**Phiên bản:** 1.0  
**Ngày tạo:** 08/02/2026  
**Kiến trúc:** Dual-Path Architecture (Web API + TCP Network)

---

## TỔNG QUAN

Dự án được chia thành **2 flows độc lập** có thể triển khai song song:

- **Flow A (Public Path):** Web Application cho khách hàng - Blazor + Web API
- **Flow B (Internal Path):** Desktop Application cho nhân viên/admin - WPF + TCP Server

**Foundation chung** phải hoàn thành trước khi triển khai 2 flows.

---

## GIAI ĐOẠN 0: FOUNDATION - CƠ SỞ HẠ TẦNG CHUNG

> **Mục tiêu:** Thiết lập solution structure, database, và shared libraries cho cả 2 flows

### **Task 0.1: Khởi tạo Solution và Projects**

**Checklist:**

- [ ] Tạo solution `MS2.sln`
- [ ] Tạo project `MS2.Models` (Class Library .NET 8)
- [ ] Tạo project `MS2.DataAccess` (Class Library .NET 8)
- [ ] Cấu hình project references: `MS2.DataAccess` reference `MS2.Models`
- [ ] Setup `.gitignore` cho .NET projects
- [ ] Commit initial structure

**Công cụ:** Visual Studio 2022 hoặc .NET CLI

**Lệnh CLI:**

```bash
dotnet new sln -n MS2
dotnet new classlib -n MS2.Models -f net8.0
dotnet new classlib -n MS2.DataAccess -f net8.0
dotnet sln add MS2.Models/MS2.Models.csproj
dotnet sln add MS2.DataAccess/MS2.DataAccess.csproj
dotnet add MS2.DataAccess reference MS2.Models
```

---

### **Task 0.2: Cài đặt NuGet Packages**

**Checklist:**

**MS2.Models:**

- [ ] `System.ComponentModel.Annotations` (cho Data Annotations)

**MS2.DataAccess:**

- [ ] `Microsoft.EntityFrameworkCore` (8.0.x)
- [ ] `Microsoft.EntityFrameworkCore.SqlServer` (8.0.x)
- [ ] `Microsoft.EntityFrameworkCore.Tools` (8.0.x)
- [ ] `Microsoft.EntityFrameworkCore.Design` (8.0.x)
- [ ] `System.Text.Json` (8.0.x)

**Lệnh CLI:**

```bash
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.SqlServer
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.Tools
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.Design
dotnet add MS2.DataAccess package System.Text.Json
```

---

### **Task 0.3: Thiết kế và Implement Entities**

**Checklist:**

**Tạo các entity classes trong `MS2.Models/Entities/`:**

- [ ] `BaseEntity.cs` (Id, CreatedAt, UpdatedAt, IsDeleted)
- [ ] `User.cs` (Id, Username, PasswordHash, Email, Role, CreatedAt)
- [ ] `Customer.cs` (Id, UserId, FullName, Phone, Address, Points)
- [ ] `Employee.cs` (Id, UserId, FullName, Position, HireDate, Salary)
- [ ] `Category.cs` (Id, Name, Description, ParentCategoryId)
- [ ] `Product.cs` (Id, CategoryId, Name, Description, Price, Stock, Barcode, ImageUrl)
- [ ] `Order.cs` (Id, CustomerId, EmployeeId, OrderDate, TotalAmount, Status, OrderType)
- [ ] `OrderDetail.cs` (Id, OrderId, ProductId, Quantity, UnitPrice, Subtotal)

**Navigation Properties cần có:**

- `Order` → `List<OrderDetail>`, `Customer`, `Employee`
- `OrderDetail` → `Order`, `Product`
- `Product` → `Category`, `List<OrderDetail>`
- `Category` → `List<Product>`, `ParentCategory`, `List<SubCategories>`
- `Customer` → `User`, `List<Order>`
- `Employee` → `User`, `List<Order>`

**Data Annotations:**

- `[Key]`, `[Required]`, `[MaxLength]`, `[ForeignKey]`

**Ví dụ code cơ bản:**

```csharp
public class Product : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; }

    [MaxLength(50)]
    public string Barcode { get; set; }

    public int CategoryId { get; set; }

    // Navigation
    public Category Category { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
}
```

---

### **Task 0.4: Tạo DbContext**

**Checklist:**

- [ ] Tạo `MS2DbContext.cs` trong `MS2.DataAccess/`
- [ ] Khai báo DbSet cho tất cả entities
- [ ] Cấu hình relationships trong `OnModelCreating()`
- [ ] Setup cascade delete rules
- [ ] Implement soft delete với query filters
- [ ] Tạo `appsettings.json` với connection string

**File: `MS2.DataAccess/MS2DbContext.cs`**

```csharp
public class MS2DbContext : DbContext
{
    public MS2DbContext(DbContextOptions<MS2DbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete filter
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);

        // Add more configurations...
    }
}
```

**Connection String trong `appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MS2Database;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

### **Task 0.5: Chạy EF Core Migrations**

**Checklist:**

- [ ] Tạo migration đầu tiên: `Add-Migration InitialCreate`
- [ ] Review migration code
- [ ] Apply migration: `Update-Database`
- [ ] Verify database được tạo trong SQL Server Management Studio (SSMS)
- [ ] Kiểm tra tất cả tables đã được tạo đúng

**Lệnh CLI:**

```bash
# Từ thư mục MS2.DataAccess
dotnet ef migrations add InitialCreate --startup-project ../MS2.DataAccess
dotnet ef database update --startup-project ../MS2.DataAccess
```

**Visual Studio Package Manager Console:**

```powershell
Add-Migration InitialCreate -Project MS2.DataAccess
Update-Database -Project MS2.DataAccess
```

---

### **Task 0.6: Implement Repository Pattern**

**Checklist:**

**Tạo Interfaces trong `MS2.DataAccess/Interfaces/`:**

- [ ] `IRepository<T>.cs` - Generic repository interface
- [ ] `IProductRepository.cs`
- [ ] `IOrderRepository.cs`
- [ ] `IUserRepository.cs`
- [ ] `IEmployeeRepository.cs`
- [ ] `ICustomerRepository.cs`
- [ ] `IUnitOfWork.cs`

**Implement trong `MS2.DataAccess/Repositories/`:**

- [ ] `Repository<T>.cs` - Generic repository implementation
- [ ] `ProductRepository.cs`
- [ ] `OrderRepository.cs`
- [ ] `UserRepository.cs`
- [ ] `EmployeeRepository.cs`
- [ ] `CustomerRepository.cs`
- [ ] `UnitOfWork.cs`

**Interface ví dụ:**

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

**Implementation ví dụ:**

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MS2DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(MS2DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    // Implement other methods...
}
```

**Specific Repository ví dụ:**

```csharp
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<Product> GetByBarcodeAsync(string barcode);
    Task<IEnumerable<Product>> SearchAsync(string keyword);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(MS2DbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product> GetByBarcodeAsync(string barcode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    // Implement other methods...
}
```

---

### **Task 0.7: Implement Unit of Work Pattern**

**Checklist:**

- [ ] Tạo `IUnitOfWork` interface
- [ ] Implement `UnitOfWork` class
- [ ] Expose tất cả repositories qua UnitOfWork
- [ ] Implement `SaveChangesAsync()` và transaction support

**Code:**

```csharp
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IUserRepository Users { get; }
    IEmployeeRepository Employees { get; }
    ICustomerRepository Customers { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly MS2DbContext _context;
    private IDbContextTransaction _transaction;

    public UnitOfWork(MS2DbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Orders = new OrderRepository(_context);
        Users = new UserRepository(_context);
        Employees = new EmployeeRepository(_context);
        Customers = new CustomerRepository(_context);
    }

    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public IUserRepository Users { get; }
    public IEmployeeRepository Employees { get; }
    public ICustomerRepository Customers { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _transaction?.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _transaction?.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
```

---

### **Task 0.8: Seed Sample Data**

**Checklist:**

- [ ] Tạo `DataSeeder.cs` trong `MS2.DataAccess/Seeders/`
- [ ] Seed 5-10 Categories
- [ ] Seed 30-50 Products với barcode
- [ ] Seed 1 Admin user
- [ ] Seed 3-5 Employee users
- [ ] Seed 5-10 Customer users
- [ ] Gọi seeder trong `Program.cs` hoặc migration

**Code:**

```csharp
public static class DataSeeder
{
    public static async Task SeedAsync(MS2DbContext context)
    {
        if (await context.Users.AnyAsync()) return; // Already seeded

        // Seed Admin
        var admin = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Email = "admin@ms2.com",
            Role = "Admin",
            CreatedAt = DateTime.Now
        };
        context.Users.Add(admin);

        // Seed Categories
        var beverages = new Category { Name = "Đồ uống", Description = "Nước giải khát" };
        var snacks = new Category { Name = "Snack", Description = "Đồ ăn vặt" };
        context.Categories.AddRange(beverages, snacks);

        await context.SaveChangesAsync();

        // Seed Products
        var products = new List<Product>
        {
            new Product
            {
                Name = "Coca Cola 330ml",
                Description = "Nước ngọt có ga",
                Price = 10000,
                Stock = 100,
                Barcode = "8934588000001",
                CategoryId = beverages.Id
            },
            // Add more products...
        };
        context.Products.AddRange(products);

        await context.SaveChangesAsync();
    }
}
```

---

### **✅ Checkpoint Foundation:**

Sau khi hoàn thành Phase 0, bạn phải có:

- ✅ Solution structure hoàn chỉnh
- ✅ Database với tất cả tables
- ✅ Repository pattern hoạt động
- ✅ Sample data đã được seed
- ✅ Unit tests cơ bản cho repositories (optional nhưng khuyến nghị)

**Có thể bắt đầu Flow A và Flow B song song từ đây!**

---

---

# FLOW A: PUBLIC PATH (WEB APPLICATION)

> **Target Users:** Khách hàng online  
> **Tech Stack:** Blazor + ASP.NET Core Web API + JWT

---

## PHASE A1: XÂY DỰNG WEB API BACKEND

### **Task A1.1: Setup Web API Project**

**Checklist:**

- [ ] Tạo project `MS2.WebAPI` (ASP.NET Core Web API .NET 8)
- [ ] Reference `MS2.Models` và `MS2.DataAccess`
- [ ] Cài packages: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`
- [ ] Setup `appsettings.json` với JWT settings và connection string
- [ ] Add project vào solution

**Lệnh CLI:**

```bash
dotnet new webapi -n MS2.WebAPI -f net8.0
dotnet sln add MS2.WebAPI/MS2.WebAPI.csproj
dotnet add MS2.WebAPI reference MS2.Models
dotnet add MS2.WebAPI reference MS2.DataAccess
dotnet add MS2.WebAPI package Microsoft.AspNetCore.Authentication.JwtBearer
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MS2Database;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "MS2WebAPI",
    "Audience": "MS2BlazorApp",
    "ExpirationMinutes": 60
  }
}
```

---

### **Task A1.2: Configure JWT Authentication**

**Checklist:**

- [ ] Tạo `JwtSettings` model trong `MS2.WebAPI/Models/`
- [ ] Configure JWT authentication trong `Program.cs`
- [ ] Tạo `JwtTokenService` trong `MS2.WebAPI/Services/`
- [ ] Test JWT generation

**File: `MS2.WebAPI/Models/JwtSettings.cs`**

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
}
```

**File: `MS2.WebAPI/Services/JwtTokenService.cs`**

```csharp
public interface IJwtTokenService
{
    string GenerateToken(User user);
    ClaimsPrincipal ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Implement ValidateToken...
}
```

**Configure trong `Program.cs`:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
        };
    });

builder.Services.AddAuthorization();

// Register DbContext
builder.Services.AddDbContext<MS2DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

---

### **Task A1.3: Implement Authentication Controller**

**Checklist:**

- [ ] Tạo DTOs: `LoginRequestDto`, `RegisterRequestDto`, `AuthResponseDto`
- [ ] Tạo `AuthController` trong `MS2.WebAPI/Controllers/`
- [ ] Implement `POST /api/auth/login`
- [ ] Implement `POST /api/auth/register`
- [ ] Test với Postman/Swagger

**DTOs:**

```csharp
public class LoginRequestDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}

public class RegisterRequestDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    public string FullName { get; set; }
    public string Phone { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

**Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.Now.AddMinutes(60)
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // Check if user exists
        if (await _unitOfWork.Users.GetByUsernameAsync(request.Username) != null)
        {
            return BadRequest(new { message = "Username already exists" });
        }

        // Create user
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Create customer profile
        var customer = new Customer
        {
            UserId = user.Id,
            FullName = request.FullName,
            Phone = request.Phone,
            Points = 0
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.Now.AddMinutes(60)
        });
    }
}
```

---

### **Task A1.4: Implement Products Controller**

**Checklist:**

- [ ] Tạo DTOs: `ProductDto`, `CreateProductDto`, `UpdateProductDto`
- [ ] Tạo `ProductsController`
- [ ] `GET /api/products` - Get all products
- [ ] `GET /api/products/{id}` - Get product by id
- [ ] `GET /api/products/category/{categoryId}` - Get by category
- [ ] `GET /api/products/search?keyword=` - Search products
- [ ] `POST /api/products` - Create (Admin only)
- [ ] `PUT /api/products/{id}` - Update (Admin only)
- [ ] `DELETE /api/products/{id}` - Delete (Admin only)

**Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
        return Ok(products);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var products = await _unitOfWork.Products.SearchAsync(keyword);
        return Ok(products);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            Barcode = dto.Barcode,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // Implement PUT and DELETE...
}
```

---

### **Task A1.5: Implement Orders Controller**

**Checklist:**

- [ ] Tạo DTOs: `OrderDto`, `CreateOrderDto`, `OrderDetailDto`
- [ ] Tạo `OrdersController`
- [ ] `GET /api/orders` - Get customer's orders (Authenticated)
- [ ] `GET /api/orders/{id}` - Get order details
- [ ] `POST /api/orders` - Create order
- [ ] `PUT /api/orders/{id}/status` - Update status (Admin/Employee)
- [ ] Implement order validation logic

**Controller outline:**

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public OrdersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customer.Id);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var customer = await _unitOfWork.Customers.GetByUserIdAsync(userId);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Create order
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderType = "Online"
            };

            decimal totalAmount = 0;

            // Add order details
            foreach (var item in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                if (product == null || product.Stock < item.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return BadRequest(new { message = $"Product {item.ProductId} not available" });
                }

                var orderDetail = new OrderDetail
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * item.Quantity
                };

                totalAmount += orderDetail.Subtotal;

                // Update stock
                product.Stock -= item.Quantity;
                await _unitOfWork.Products.UpdateAsync(product);

                order.OrderDetails.Add(orderDetail);
            }

            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // Implement other endpoints...
}
```

---

### **Task A1.6: Configure CORS and Swagger**

**Checklist:**

- [ ] Configure CORS policy trong `Program.cs`
- [ ] Enable Swagger UI
- [ ] Configure Swagger to support JWT authentication
- [ ] Test all endpoints với Swagger

**Program.cs additions:**

```csharp
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins("https://localhost:7001") // Blazor app URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MS2 Web API", Version = "v1" });

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

### **✅ Checkpoint Phase A1:**

- ✅ Web API running trên https://localhost:7000
- ✅ Swagger UI accessible
- ✅ JWT authentication working
- ✅ All CRUD endpoints tested
- ✅ Authorization working properly

---

## PHASE A2: XÂY DỰNG BLAZOR WEB APPLICATION

### **Task A2.1: Setup Blazor Project**

**Checklist:**

- [ ] Tạo project `MS2.BlazorApp` (Blazor Server hoặc WebAssembly)
- [ ] Cài packages: `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (nếu WASM)
- [ ] Reference `MS2.Models`
- [ ] Setup base URL cho API trong `appsettings.json` hoặc `wwwroot/appsettings.json`
- [ ] Add project vào solution

**Lệnh CLI (Blazor WebAssembly):**

```bash
dotnet new blazorwasm -n MS2.BlazorApp -f net8.0
dotnet sln add MS2.BlazorApp/MS2.BlazorApp.csproj
dotnet add MS2.BlazorApp reference MS2.Models
```

**wwwroot/appsettings.json:**

```json
{
  "ApiBaseUrl": "https://localhost:7000"
}
```

---

### **Task A2.2: Create HTTP Services**

**Checklist:**

- [ ] Tạo `Services/` folder
- [ ] Implement `AuthService` với login/register/logout
- [ ] Implement `ProductService` với CRUD methods
- [ ] Implement `OrderService` với order operations
- [ ] Implement `LocalStorageService` để lưu JWT token
- [ ] Configure HttpClient với JWT interceptor

**File: `Services/AuthService.cs`**

```csharp
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task LogoutAsync();
    Task<string> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            await _localStorage.SetItemAsync("authToken", authResponse.Token);
            return authResponse;
        }

        throw new Exception("Login failed");
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            await _localStorage.SetItemAsync("authToken", authResponse.Token);
            return authResponse;
        }

        throw new Exception("Registration failed");
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
    }

    public async Task<string> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
```

**File: `Services/ProductService.cs`**

```csharp
public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product> GetProductByIdAsync(int id);
    Task<List<Product>> SearchProductsAsync(string keyword);
    Task<List<Product>> GetProductsByCategoryAsync(int categoryId);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Product>>("api/products");
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Product>($"api/products/{id}");
    }

    public async Task<List<Product>> SearchProductsAsync(string keyword)
    {
        return await _httpClient.GetFromJsonAsync<List<Product>>($"api/products/search?keyword={keyword}");
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _httpClient.GetFromJsonAsync<List<Product>>($"api/products/category/{categoryId}");
    }
}
```

**Configure HttpClient trong `Program.cs`:**

```csharp
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7000")
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddBlazoredLocalStorage();

// Add JWT to HTTP requests
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
```

---

### **Task A2.3: Implement Authentication State Provider**

**Checklist:**

- [ ] Tạo `CustomAuthStateProvider.cs`
- [ ] Parse JWT token để lấy claims
- [ ] Implement `GetAuthenticationStateAsync()`
- [ ] Notify authentication state changed

**Code:**

```csharp
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(IAuthService authService, ILogger<CustomAuthStateProvider> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
```

---

### **Task A2.4: Create Pages and Components**

**Checklist:**

**Pages cần tạo:**

- [ ] `Login.razor` - Đăng nhập
- [ ] `Register.razor` - Đăng ký
- [ ] `Products.razor` - Danh sách sản phẩm
- [ ] `ProductDetail.razor` - Chi tiết sản phẩm
- [ ] `Cart.razor` - Giỏ hàng
- [ ] `Checkout.razor` - Thanh toán
- [ ] `Orders.razor` - Lịch sử đơn hàng
- [ ] `OrderDetail.razor` - Chi tiết đơn hàng

**Components cần tạo:**

- [ ] `ProductCard.razor` - Card hiển thị sản phẩm
- [ ] `CartItem.razor` - Item trong giỏ hàng
- [ ] `NavMenu.razor` - Navigation menu

**Example: `Pages/Login.razor`**

```razor
@page "/login"
@inject IAuthService AuthService
@inject NavigationManager Navigation
@inject AuthenticationStateProvider AuthStateProvider

<div class="login-container">
    <h3>Đăng nhập</h3>

    <EditForm Model="loginModel" OnValidSubmit="HandleLogin">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="form-group">
            <label>Username:</label>
            <InputText @bind-Value="loginModel.Username" class="form-control" />
        </div>

        <div class="form-group">
            <label>Password:</label>
            <InputText type="password" @bind-Value="loginModel.Password" class="form-control" />
        </div>

        @if (!string.IsNullOrEmpty(errorMessage))
        {
            <div class="alert alert-danger">@errorMessage</div>
        }

        <button type="submit" class="btn btn-primary">Đăng nhập</button>
        <a href="/register" class="btn btn-link">Đăng ký tài khoản mới</a>
    </EditForm>
</div>

@code {
    private LoginRequestDto loginModel = new();
    private string errorMessage;

    private async Task HandleLogin()
    {
        try
        {
            var response = await AuthService.LoginAsync(loginModel);

            ((CustomAuthStateProvider)AuthStateProvider).NotifyUserAuthentication(response.Token);

            Navigation.NavigateTo("/products");
        }
        catch (Exception ex)
        {
            errorMessage = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
        }
    }
}
```

**Example: `Pages/Products.razor`**

```razor
@page "/products"
@inject IProductService ProductService

<h3>Sản phẩm</h3>

<div class="search-bar">
    <input type="text" @bind="searchKeyword" @bind:event="oninput" placeholder="Tìm kiếm sản phẩm..." class="form-control" />
    <button @onclick="SearchProducts" class="btn btn-primary">Tìm kiếm</button>
</div>

@if (products == null)
{
    <p>Đang tải...</p>
}
else if (products.Count == 0)
{
    <p>Không tìm thấy sản phẩm nào.</p>
}
else
{
    <div class="product-grid">
        @foreach (var product in products)
        {
            <ProductCard Product="product" OnAddToCart="HandleAddToCart" />
        }
    </div>
}

@code {
    private List<Product> products;
    private string searchKeyword;

    protected override async Task OnInitializedAsync()
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        products = await ProductService.GetAllProductsAsync();
    }

    private async Task SearchProducts()
    {
        if (string.IsNullOrWhiteSpace(searchKeyword))
        {
            await LoadProducts();
        }
        else
        {
            products = await ProductService.SearchProductsAsync(searchKeyword);
        }
    }

    private void HandleAddToCart(Product product)
    {
        // Implement cart logic
    }
}
```

**Example: `Components/ProductCard.razor`**

```razor
<div class="product-card">
    <img src="@Product.ImageUrl" alt="@Product.Name" />
    <h4>@Product.Name</h4>
    <p>@Product.Description</p>
    <p class="price">@Product.Price.ToString("C0")</p>
    <p class="stock">Còn lại: @Product.Stock</p>

    <button @onclick="() => OnAddToCart.InvokeAsync(Product)"
            class="btn btn-success"
            disabled="@(Product.Stock == 0)">
        Thêm vào giỏ
    </button>
</div>

@code {
    [Parameter]
    public Product Product { get; set; }

    [Parameter]
    public EventCallback<Product> OnAddToCart { get; set; }
}
```

---

### **Task A2.5: Implement Cart Functionality**

**Checklist:**

- [ ] Tạo `CartService` để quản lý giỏ hàng (local state)
- [ ] Implement add/remove/update cart items
- [ ] Persist cart trong LocalStorage
- [ ] Calculate cart total
- [ ] Create `Cart.razor` page

**File: `Services/CartService.cs`**

```csharp
public interface ICartService
{
    Task<List<CartItem>> GetCartItemsAsync();
    Task AddToCartAsync(Product product, int quantity = 1);
    Task RemoveFromCartAsync(int productId);
    Task UpdateQuantityAsync(int productId, int quantity);
    Task ClearCartAsync();
    Task<decimal> GetTotalAsync();
}

public class CartService : ICartService
{
    private readonly ILocalStorageService _localStorage;
    private const string CART_KEY = "cart";

    public CartService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var cart = await _localStorage.GetItemAsync<List<CartItem>>(CART_KEY);
        return cart ?? new List<CartItem>();
    }

    public async Task AddToCartAsync(Product product, int quantity = 1)
    {
        var cart = await GetCartItemsAsync();
        var existingItem = cart.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            });
        }

        await _localStorage.SetItemAsync(CART_KEY, cart);
    }

    public async Task RemoveFromCartAsync(int productId)
    {
        var cart = await GetCartItemsAsync();
        cart.RemoveAll(x => x.ProductId == productId);
        await _localStorage.SetItemAsync(CART_KEY, cart);
    }

    public async Task UpdateQuantityAsync(int productId, int quantity)
    {
        var cart = await GetCartItemsAsync();
        var item = cart.FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            item.Quantity = quantity;
            await _localStorage.SetItemAsync(CART_KEY, cart);
        }
    }

    public async Task ClearCartAsync()
    {
        await _localStorage.RemoveItemAsync(CART_KEY);
    }

    public async Task<decimal> GetTotalAsync()
    {
        var cart = await GetCartItemsAsync();
        return cart.Sum(x => x.Price * x.Quantity);
    }
}

public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; }
}
```

---

### **Task A2.6: Implement Checkout Flow**

**Checklist:**

- [ ] Tạo `Checkout.razor` page
- [ ] Validate cart không empty
- [ ] Collect delivery information
- [ ] Call OrderService để tạo order
- [ ] Clear cart sau khi order thành công
- [ ] Navigate to order confirmation page

**Example: `Pages/Checkout.razor`**

```razor
@page "/checkout"
@attribute [Authorize]
@inject ICartService CartService
@inject IOrderService OrderService
@inject NavigationManager Navigation

<h3>Thanh toán</h3>

<div class="checkout-container">
    <div class="cart-summary">
        <h4>Đơn hàng của bạn</h4>

        @if (cartItems == null || cartItems.Count == 0)
        {
            <p>Giỏ hàng trống</p>
        }
        else
        {
            @foreach (var item in cartItems)
            {
                <div class="cart-item">
                    <span>@item.ProductName</span>
                    <span>x @item.Quantity</span>
                    <span>@((item.Price * item.Quantity).ToString("C0"))</span>
                </div>
            }

            <div class="total">
                <strong>Tổng cộng: @totalAmount.ToString("C0")</strong>
            </div>
        }
    </div>

    <div class="delivery-info">
        <h4>Thông tin giao hàng</h4>

        <EditForm Model="deliveryModel" OnValidSubmit="HandleCheckout">
            <DataAnnotationsValidator />
            <ValidationSummary />

            <div class="form-group">
                <label>Họ tên:</label>
                <InputText @bind-Value="deliveryModel.FullName" class="form-control" />
            </div>

            <div class="form-group">
                <label>Số điện thoại:</label>
                <InputText @bind-Value="deliveryModel.Phone" class="form-control" />
            </div>

            <div class="form-group">
                <label>Địa chỉ:</label>
                <InputTextArea @bind-Value="deliveryModel.Address" class="form-control" rows="3" />
            </div>

            <div class="form-group">
                <label>Ghi chú:</label>
                <InputTextArea @bind-Value="deliveryModel.Notes" class="form-control" rows="2" />
            </div>

            @if (!string.IsNullOrEmpty(errorMessage))
            {
                <div class="alert alert-danger">@errorMessage</div>
            }

            <button type="submit" class="btn btn-success" disabled="@isProcessing">
                @(isProcessing ? "Đang xử lý..." : "Đặt hàng")
            </button>
        </EditForm>
    </div>
</div>

@code {
    private List<CartItem> cartItems;
    private decimal totalAmount;
    private DeliveryInfoModel deliveryModel = new();
    private string errorMessage;
    private bool isProcessing;

    protected override async Task OnInitializedAsync()
    {
        cartItems = await CartService.GetCartItemsAsync();
        totalAmount = await CartService.GetTotalAsync();
    }

    private async Task HandleCheckout()
    {
        if (cartItems == null || cartItems.Count == 0)
        {
            errorMessage = "Giỏ hàng trống";
            return;
        }

        isProcessing = true;

        try
        {
            var orderDto = new CreateOrderDto
            {
                Items = cartItems.Select(x => new OrderItemDto
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList(),
                DeliveryAddress = deliveryModel.Address,
                Phone = deliveryModel.Phone,
                Notes = deliveryModel.Notes
            };

            var orderId = await OrderService.CreateOrderAsync(orderDto);

            await CartService.ClearCartAsync();

            Navigation.NavigateTo($"/orders/{orderId}");
        }
        catch (Exception ex)
        {
            errorMessage = "Đặt hàng thất bại. Vui lòng thử lại.";
        }
        finally
        {
            isProcessing = false;
        }
    }

    public class DeliveryInfoModel
    {
        [Required] public string FullName { get; set; }
        [Required] public string Phone { get; set; }
        [Required] public string Address { get; set; }
        public string Notes { get; set; }
    }
}
```

---

### **✅ Checkpoint Phase A2:**

- ✅ Blazor app running
- ✅ Login/Register working
- ✅ Products display correctly
- ✅ Cart functionality working
- ✅ Checkout flow complete
- ✅ Orders history visible

---

## PHASE A3: TESTING & DEPLOYMENT FLOW A

### **Task A3.1: Integration Testing**

**Checklist:**

- [ ] Test user registration flow
- [ ] Test login/logout flow
- [ ] Test product browsing and search
- [ ] Test add to cart functionality
- [ ] Test checkout and order creation
- [ ] Test order history viewing
- [ ] Test authorization (protected pages)

---

### **Task A3.2: UI/UX Improvements**

**Checklist:**

- [ ] Add loading spinners
- [ ] Add error messages/toasts
- [ ] Responsive design cho mobile
- [ ] Optimize images
- [ ] Add pagination cho products
- [ ] Add filters và sorting

---

### **Task A3.3: Deployment**

**Checklist:**

- [ ] Publish Web API lên IIS/Azure App Service
- [ ] Configure production database connection string
- [ ] Apply EF migrations lên production DB
- [ ] Publish Blazor app
- [ ] Configure HTTPS certificates
- [ ] Test production deployment
- [ ] Setup monitoring và logging

---

---

# FLOW B: INTERNAL PATH (DESKTOP APPLICATION)

> **Target Users:** Nhân viên, Admin  
> **Tech Stack:** WPF + TCP Server + Socket Communication

---

## PHASE B1: XÂY DỰNG TCP SERVER

### **Task B1.1: Setup TCP Server Project**

**Checklist:**

- [ ] Tạo project `MS2.ServerApp` (Console App .NET 8)
- [ ] Reference `MS2.Models` và `MS2.DataAccess`
- [ ] Cài packages: `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`
- [ ] Setup `appsettings.json` với TCP port và connection string
- [ ] Add project vào solution

**Lệnh CLI:**

```bash
dotnet new console -n MS2.ServerApp -f net8.0
dotnet sln add MS2.ServerApp/MS2.ServerApp.csproj
dotnet add MS2.ServerApp reference MS2.Models
dotnet add MS2.ServerApp reference MS2.DataAccess
dotnet add MS2.ServerApp package Microsoft.Extensions.DependencyInjection
dotnet add MS2.ServerApp package Microsoft.Extensions.Hosting
dotnet add MS2.ServerApp package Microsoft.Extensions.Configuration
dotnet add MS2.ServerApp package Microsoft.Extensions.Configuration.Json
```

**appsettings.json:**

```json
{
  "TcpSettings": {
    "Host": "127.0.0.1",
    "Port": 5000,
    "MaxConnections": 50
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MS2Database;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "MS2ServerApp",
    "Audience": "MS2DesktopApp",
    "ExpirationMinutes": 480
  }
}
```

---

### **Task B1.2: Design TCP Message Protocol**

**Checklist:**

- [ ] Tạo `TcpMessage` class trong `MS2.Models/TCP/`
- [ ] Define message structure: Action, Data, Token
- [ ] Implement JSON serialization/deserialization
- [ ] Implement Byte[] conversion methods
- [ ] Define response format

**File: `MS2.Models/TCP/TcpMessage.cs`**

```csharp
public class TcpMessage
{
    public string Action { get; set; }
    public object Data { get; set; }
    public string Token { get; set; }
    public string RequestId { get; set; }

    public static TcpMessage Create(string action, object data, string token = null)
    {
        return new TcpMessage
        {
            Action = action,
            Data = data,
            Token = token,
            RequestId = Guid.NewGuid().ToString()
        };
    }

    public byte[] ToBytes()
    {
        var json = JsonSerializer.Serialize(this);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Prefix with message length (4 bytes)
        var lengthPrefix = BitConverter.GetBytes(bytes.Length);
        return lengthPrefix.Concat(bytes).ToArray();
    }

    public static TcpMessage FromBytes(byte[] bytes)
    {
        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<TcpMessage>(json);
    }
}

public class TcpResponse
{
    public bool Success { get; set; }
    public object Data { get; set; }
    public string Message { get; set; }
    public string RequestId { get; set; }

    public static TcpResponse CreateSuccess(object data, string requestId)
    {
        return new TcpResponse
        {
            Success = true,
            Data = data,
            RequestId = requestId
        };
    }

    public static TcpResponse CreateError(string message, string requestId)
    {
        return new TcpResponse
        {
            Success = false,
            Message = message,
            RequestId = requestId
        };
    }

    public byte[] ToBytes()
    {
        var json = JsonSerializer.Serialize(this);
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthPrefix = BitConverter.GetBytes(bytes.Length);
        return lengthPrefix.Concat(bytes).ToArray();
    }
}
```

**Defined Actions:**

- `LOGIN` - Đăng nhập
- `GET_PRODUCTS` - Lấy danh sách sản phẩm
- `SEARCH_PRODUCTS` - Tìm kiếm sản phẩm
- `GET_PRODUCT_BY_BARCODE` - Lấy sản phẩm theo barcode
- `CREATE_ORDER` - Tạo đơn hàng
- `UPDATE_PRODUCT_PRICE` - Cập nhật giá sản phẩm
- `UPDATE_PRODUCT_STOCK` - Cập nhật tồn kho
- `GET_SALES_REPORT` - Lấy báo cáo doanh thu
- `GET_EMPLOYEES` - Lấy danh sách nhân viên
- `GET_INVENTORY` - Lấy thông tin kho

---

### **Task B1.3: Implement TCP Server Core**

**Checklist:**

- [ ] Tạo `TcpServer` class trong `MS2.ServerApp/Services/`
- [ ] Implement `TcpListener` initialization
- [ ] Implement async client acceptance loop
- [ ] Implement multi-client handling (each on separate Task)
- [ ] Implement graceful shutdown
- [ ] Add logging

**File: `MS2.ServerApp/Services/TcpServer.cs`**

```csharp
public class TcpServer
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TcpServer> _logger;
    private TcpListener _listener;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly List<Task> _clientTasks = new();

    public TcpServer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<TcpServer> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        var host = _configuration["TcpSettings:Host"];
        var port = int.Parse(_configuration["TcpSettings:Port"]);

        _listener = new TcpListener(IPAddress.Parse(host), port);
        _listener.Start();

        _cancellationTokenSource = new CancellationTokenSource();

        _logger.LogInformation($"TCP Server started on {host}:{port}");

        await AcceptClientsAsync(_cancellationTokenSource.Token);
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _logger.LogInformation($"Client connected from {client.Client.RemoteEndPoint}");

                var clientTask = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                _clientTasks.Add(clientTask);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting client");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    // Read message length (4 bytes)
                    var lengthBuffer = new byte[4];
                    var bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4, cancellationToken);

                    if (bytesRead == 0) break;

                    var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // Read message data
                    var messageBuffer = new byte[messageLength];
                    var totalRead = 0;

                    while (totalRead < messageLength)
                    {
                        bytesRead = await stream.ReadAsync(
                            messageBuffer,
                            totalRead,
                            messageLength - totalRead,
                            cancellationToken);

                        if (bytesRead == 0) break;

                        totalRead += bytesRead;
                    }

                    // Deserialize message
                    var message = TcpMessage.FromBytes(messageBuffer);
                    _logger.LogInformation($"Received action: {message.Action}");

                    // Process message
                    var response = await ProcessMessageAsync(message);

                    // Send response
                    var responseBytes = response.ToBytes();
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client");
            }
            finally
            {
                _logger.LogInformation($"Client disconnected from {client.Client.RemoteEndPoint}");
            }
        }
    }

    private async Task<TcpResponse> ProcessMessageAsync(TcpMessage message)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<ITcpMessageHandler>();
            return await handler.HandleAsync(message);
        }
    }

    public async Task StopAsync()
    {
        _cancellationTokenSource?.Cancel();
        _listener?.Stop();

        await Task.WhenAll(_clientTasks);

        _logger.LogInformation("TCP Server stopped");
    }
}
```

---

### **Task B1.4: Implement Message Handlers**

**Checklist:**

- [ ] Tạo `ITcpMessageHandler` interface
- [ ] Implement `TcpMessageHandler` với routing logic
- [ ] Implement handlers cho từng action
- [ ] Implement JWT validation
- [ ] Add error handling

**File: `MS2.ServerApp/Handlers/ITcpMessageHandler.cs`**

```csharp
public interface ITcpMessageHandler
{
    Task<TcpResponse> HandleAsync(TcpMessage message);
}

public class TcpMessageHandler : ITcpMessageHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<TcpMessageHandler> _logger;

    public TcpMessageHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<TcpMessageHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<TcpResponse> HandleAsync(TcpMessage message)
    {
        try
        {
            // Validate JWT (except for LOGIN action)
            if (message.Action != "LOGIN")
            {
                if (string.IsNullOrEmpty(message.Token))
                {
                    return TcpResponse.CreateError("Token required", message.RequestId);
                }

                var principal = _jwtTokenService.ValidateToken(message.Token);
                if (principal == null)
                {
                    return TcpResponse.CreateError("Invalid token", message.RequestId);
                }
            }

            // Route to appropriate handler
            return message.Action switch
            {
                "LOGIN" => await HandleLoginAsync(message),
                "GET_PRODUCTS" => await HandleGetProductsAsync(message),
                "SEARCH_PRODUCTS" => await HandleSearchProductsAsync(message),
                "GET_PRODUCT_BY_BARCODE" => await HandleGetProductByBarcodeAsync(message),
                "CREATE_ORDER" => await HandleCreateOrderAsync(message),
                "UPDATE_PRODUCT_PRICE" => await HandleUpdateProductPriceAsync(message),
                "UPDATE_PRODUCT_STOCK" => await HandleUpdateProductStockAsync(message),
                "GET_SALES_REPORT" => await HandleGetSalesReportAsync(message),
                "GET_EMPLOYEES" => await HandleGetEmployeesAsync(message),
                "GET_INVENTORY" => await HandleGetInventoryAsync(message),
                _ => TcpResponse.CreateError($"Unknown action: {message.Action}", message.RequestId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling action: {message.Action}");
            return TcpResponse.CreateError(ex.Message, message.RequestId);
        }
    }

    private async Task<TcpResponse> HandleLoginAsync(TcpMessage message)
    {
        var loginData = JsonSerializer.Deserialize<LoginRequestDto>(
            message.Data.ToString());

        var user = await _unitOfWork.Users.GetByUsernameAsync(loginData.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash))
        {
            return TcpResponse.CreateError("Invalid credentials", message.RequestId);
        }

        if (user.Role != "Employee" && user.Role != "Admin")
        {
            return TcpResponse.CreateError("Access denied", message.RequestId);
        }

        var token = _jwtTokenService.GenerateToken(user);

        var response = new
        {
            Token = token,
            User = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role
            }
        };

        return TcpResponse.CreateSuccess(response, message.RequestId);
    }

    private async Task<TcpResponse> HandleGetProductsAsync(TcpMessage message)
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return TcpResponse.CreateSuccess(products, message.RequestId);
    }

    private async Task<TcpResponse> HandleSearchProductsAsync(TcpMessage message)
    {
        var searchData = JsonSerializer.Deserialize<Dictionary<string, string>>(
            message.Data.ToString());

        var keyword = searchData["keyword"];
        var products = await _unitOfWork.Products.SearchAsync(keyword);

        return TcpResponse.CreateSuccess(products, message.RequestId);
    }

    private async Task<TcpResponse> HandleGetProductByBarcodeAsync(TcpMessage message)
    {
        var barcodeData = JsonSerializer.Deserialize<Dictionary<string, string>>(
            message.Data.ToString());

        var barcode = barcodeData["barcode"];
        var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode);

        if (product == null)
        {
            return TcpResponse.CreateError("Product not found", message.RequestId);
        }

        return TcpResponse.CreateSuccess(product, message.RequestId);
    }

    private async Task<TcpResponse> HandleCreateOrderAsync(TcpMessage message)
    {
        var orderData = JsonSerializer.Deserialize<CreateOrderDto>(
            message.Data.ToString());

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                EmployeeId = orderData.EmployeeId,
                OrderDate = DateTime.Now,
                Status = "Completed",
                OrderType = "POS"
            };

            decimal totalAmount = 0;

            foreach (var item in orderData.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                if (product == null || product.Stock < item.Quantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return TcpResponse.CreateError(
                        $"Product {item.ProductId} not available",
                        message.RequestId);
                }

                var orderDetail = new OrderDetail
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = product.Price * item.Quantity
                };

                totalAmount += orderDetail.Subtotal;

                product.Stock -= item.Quantity;
                await _unitOfWork.Products.UpdateAsync(product);

                order.OrderDetails.Add(orderDetail);
            }

            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return TcpResponse.CreateSuccess(new { OrderId = order.Id, TotalAmount = totalAmount }, message.RequestId);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<TcpResponse> HandleUpdateProductPriceAsync(TcpMessage message)
    {
        var updateData = JsonSerializer.Deserialize<UpdateProductPriceDto>(
            message.Data.ToString());

        var product = await _unitOfWork.Products.GetByIdAsync(updateData.ProductId);

        if (product == null)
        {
            return TcpResponse.CreateError("Product not found", message.RequestId);
        }

        product.Price = updateData.NewPrice;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return TcpResponse.CreateSuccess(product, message.RequestId);
    }

    private async Task<TcpResponse> HandleUpdateProductStockAsync(TcpMessage message)
    {
        var updateData = JsonSerializer.Deserialize<UpdateProductStockDto>(
            message.Data.ToString());

        var product = await _unitOfWork.Products.GetByIdAsync(updateData.ProductId);

        if (product == null)
        {
            return TcpResponse.CreateError("Product not found", message.RequestId);
        }

        product.Stock = updateData.NewStock;
        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return TcpResponse.CreateSuccess(product, message.RequestId);
    }

    private async Task<TcpResponse> HandleGetSalesReportAsync(TcpMessage message)
    {
        var reportData = JsonSerializer.Deserialize<Dictionary<string, string>>(
            message.Data.ToString());

        var fromDate = DateTime.Parse(reportData["fromDate"]);
        var toDate = DateTime.Parse(reportData["toDate"]);

        var orders = await _unitOfWork.Orders.GetByDateRangeAsync(fromDate, toDate);

        var report = new
        {
            TotalOrders = orders.Count(),
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            AverageOrderValue = orders.Average(o => o.TotalAmount),
            Orders = orders
        };

        return TcpResponse.CreateSuccess(report, message.RequestId);
    }

    private async Task<TcpResponse> HandleGetEmployeesAsync(TcpMessage message)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        return TcpResponse.CreateSuccess(employees, message.RequestId);
    }

    private async Task<TcpResponse> HandleGetInventoryAsync(TcpMessage message)
    {
        var products = await _unitOfWork.Products.GetAllAsync();

        var inventory = products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Barcode,
            p.Stock,
            p.Price,
            TotalValue = p.Stock * p.Price
        });

        return TcpResponse.CreateSuccess(inventory, message.RequestId);
    }
}
```

---

### **Task B1.5: Setup Dependency Injection & Program.cs**

**Checklist:**

- [ ] Configure DI container
- [ ] Register DbContext
- [ ] Register repositories
- [ ] Register services
- [ ] Setup logging
- [ ] Implement graceful shutdown

**File: `MS2.ServerApp/Program.cs`**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// DbContext
builder.Services.AddDbContext<MS2DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ITcpMessageHandler, TcpMessageHandler>();
builder.Services.AddSingleton<TcpServer>();

// JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var host = builder.Build();

// Start TCP Server
var tcpServer = host.Services.GetRequiredService<TcpServer>();

Console.WriteLine("===========================================");
Console.WriteLine("   MS2 TCP Server");
Console.WriteLine("===========================================");
Console.WriteLine();

var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    await tcpServer.StartAsync();

    Console.WriteLine();
    Console.WriteLine("Press Ctrl+C to stop the server...");

    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Stopping server...");
}
finally
{
    await tcpServer.StopAsync();
    Console.WriteLine("Server stopped.");
}
```

---

### **Task B1.6: Test TCP Server**

**Checklist:**

- [ ] Tạo simple test client (Console app)
- [ ] Test LOGIN action
- [ ] Test GET_PRODUCTS action
- [ ] Test CREATE_ORDER action
- [ ] Test error handling
- [ ] Test concurrent connections

**Simple Test Client:**

```csharp
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var client = new TcpClient();
await client.ConnectAsync("127.0.0.1", 5000);

var stream = client.GetStream();

// Test LOGIN
var loginMessage = TcpMessage.Create("LOGIN", new
{
    Username = "admin",
    Password = "admin123"
});

await SendMessageAsync(stream, loginMessage);
var loginResponse = await ReceiveResponseAsync(stream);

Console.WriteLine($"Login: {loginResponse.Success}");
if (loginResponse.Success)
{
    var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(
        loginResponse.Data.ToString());
    var token = responseData["Token"].ToString();
    Console.WriteLine($"Token: {token}");

    // Test GET_PRODUCTS
    var productsMessage = TcpMessage.Create("GET_PRODUCTS", null, token);
    await SendMessageAsync(stream, productsMessage);
    var productsResponse = await ReceiveResponseAsync(stream);

    Console.WriteLine($"Products count: {productsResponse.Success}");
}

client.Close();

async Task SendMessageAsync(NetworkStream stream, TcpMessage message)
{
    var bytes = message.ToBytes();
    await stream.WriteAsync(bytes, 0, bytes.Length);
    await stream.FlushAsync();
}

async Task<TcpResponse> ReceiveResponseAsync(NetworkStream stream)
{
    var lengthBuffer = new byte[4];
    await stream.ReadAsync(lengthBuffer, 0, 4);
    var messageLength = BitConverter.ToInt32(lengthBuffer, 0);

    var messageBuffer = new byte[messageLength];
    var totalRead = 0;

    while (totalRead < messageLength)
    {
        var bytesRead = await stream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead);
        totalRead += bytesRead;
    }

    var json = Encoding.UTF8.GetString(messageBuffer);
    return JsonSerializer.Deserialize<TcpResponse>(json);
}
```

---

### **✅ Checkpoint Phase B1:**

- ✅ TCP Server running on port 5000
- ✅ Multi-client support working
- ✅ All actions implemented and tested
- ✅ JWT authentication working
- ✅ Database operations successful
- ✅ Error handling robust

---

## PHASE B2: XÂY DỰNG WPF DESKTOP APPLICATION

### **Task B2.1: Setup WPF Project**

**Checklist:**

- [ ] Tạo project `MS2.DesktopApp` (WPF .NET 8)
- [ ] Reference `MS2.Models`
- [ ] Cài packages: `CommunityToolkit.Mvvm`, `System.Text.Json`
- [ ] Setup project structure: Views/, ViewModels/, Services/, Models/
- [ ] Add project vào solution

**Lệnh CLI:**

```bash
dotnet new wpf -n MS2.DesktopApp -f net8.0
dotnet sln add MS2.DesktopApp/MS2.DesktopApp.csproj
dotnet add MS2.DesktopApp reference MS2.Models
dotnet add MS2.DesktopApp package CommunityToolkit.Mvvm
dotnet add MS2.DesktopApp package System.Text.Json
```

**Project structure:**

```
MS2.DesktopApp/
├── Views/
│   ├── LoginWindow.xaml
│   ├── MainWindow.xaml
│   ├── POSView.xaml
│   ├── InventoryView.xaml
│   ├── ReportsView.xaml
│   └── EmployeeManagementView.xaml
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── MainViewModel.cs
│   ├── POSViewModel.cs
│   ├── InventoryViewModel.cs
│   ├── ReportsViewModel.cs
│   └── EmployeeManagementViewModel.cs
├── Services/
│   ├── TcpNetworkService.cs
│   ├── AuthService.cs
│   └── NavigationService.cs
├── Models/
│   └── AppSettings.cs
└── App.xaml
```

---

### **Task B2.2: Implement TCP Network Service**

**Checklist:**

- [ ] Tạo `TcpNetworkService` class
- [ ] Implement `ConnectAsync()`
- [ ] Implement `SendMessageAsync<T>()`
- [ ] Implement `ReceiveResponseAsync<T>()`
- [ ] Implement auto-reconnect logic
- [ ] Handle connection errors

**File: `Services/TcpNetworkService.cs`**

```csharp
public interface ITcpNetworkService
{
    Task<bool> ConnectAsync();
    Task DisconnectAsync();
    Task<TcpResponse> SendMessageAsync(string action, object data, string token = null);
    bool IsConnected { get; }
}

public class TcpNetworkService : ITcpNetworkService
{
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly string _host = "127.0.0.1";
    private readonly int _port = 5000;
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public bool IsConnected => _client?.Connected ?? false;

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Connection error: {ex.Message}");
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        _stream?.Close();
        _client?.Close();
        _stream = null;
        _client = null;
    }

    public async Task<TcpResponse> SendMessageAsync(string action, object data, string token = null)
    {
        if (!IsConnected)
        {
            var connected = await ConnectAsync();
            if (!connected)
            {
                return TcpResponse.CreateError("Not connected to server", null);
            }
        }

        await _sendLock.WaitAsync();

        try
        {
            // Send message
            var message = TcpMessage.Create(action, data, token);
            var messageBytes = message.ToBytes();

            await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            await _stream.FlushAsync();

            // Receive response
            var lengthBuffer = new byte[4];
            var bytesRead = await _stream.ReadAsync(lengthBuffer, 0, 4);

            if (bytesRead == 0)
            {
                await DisconnectAsync();
                return TcpResponse.CreateError("Connection closed", message.RequestId);
            }

            var messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            var messageBuffer = new byte[messageLength];
            var totalRead = 0;

            while (totalRead < messageLength)
            {
                bytesRead = await _stream.ReadAsync(messageBuffer, totalRead, messageLength - totalRead);
                if (bytesRead == 0) break;
                totalRead += bytesRead;
            }

            var json = Encoding.UTF8.GetString(messageBuffer);
            return JsonSerializer.Deserialize<TcpResponse>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Send error: {ex.Message}");
            await DisconnectAsync();
            return TcpResponse.CreateError(ex.Message, null);
        }
        finally
        {
            _sendLock.Release();
        }
    }
}
```

---

### **Task B2.3: Implement Authentication Service**

**Checklist:**

- [ ] Tạo `AuthService` class
- [ ] Implement `LoginAsync()`
- [ ] Store JWT token in memory
- [ ] Store user info
- [ ] Implement `LogoutAsync()`

**File: `Services/AuthService.cs`**

```csharp
public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    void Logout();
    string GetToken();
    User GetCurrentUser();
    bool IsAuthenticated { get; }
}

public class AuthService : IAuthService
{
    private readonly ITcpNetworkService _networkService;
    private string _token;
    private User _currentUser;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public AuthService(ITcpNetworkService networkService)
    {
        _networkService = networkService;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _networkService.SendMessageAsync("LOGIN", new
        {
            Username = username,
            Password = password
        });

        if (response.Success)
        {
            var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                response.Data.ToString());

            _token = responseData["Token"].ToString();

            var userJson = responseData["User"].ToString();
            _currentUser = JsonSerializer.Deserialize<User>(userJson);

            return true;
        }

        return false;
    }

    public void Logout()
    {
        _token = null;
        _currentUser = null;
    }

    public string GetToken()
    {
        return _token;
    }

    public User GetCurrentUser()
    {
        return _currentUser;
    }
}
```

---

### **Task B2.4: Implement Login Window**

**Checklist:**

- [ ] Tạo `LoginWindow.xaml` và `LoginViewModel.cs`
- [ ] Design login UI
- [ ] Implement login command
- [ ] Show loading indicator
- [ ] Handle login errors
- [ ] Navigate to MainWindow on success

**XAML: `Views/LoginWindow.xaml`**

```xml
<Window x:Class="MS2.DesktopApp.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MS2 - Đăng nhập" Height="400" Width="350"
        WindowStartupLocation="CenterScreen"
        ResizeMode="NoResize">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Border Grid.Row="0" Background="#2196F3" Padding="20">
            <StackPanel>
                <TextBlock Text="MINIMART SMART SYSTEM"
                           FontSize="20"
                           FontWeight="Bold"
                           Foreground="White"
                           HorizontalAlignment="Center"/>
                <TextBlock Text="Point of Sale"
                           FontSize="12"
                           Foreground="White"
                           HorizontalAlignment="Center"
                           Margin="0,5,0,0"/>
            </StackPanel>
        </Border>

        <!-- Login Form -->
        <StackPanel Grid.Row="1" Margin="30" VerticalAlignment="Center">
            <TextBlock Text="Tên đăng nhập:" FontWeight="SemiBold" Margin="0,0,0,5"/>
            <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
                     Height="35"
                     FontSize="14"
                     Padding="5"/>

            <TextBlock Text="Mật khẩu:" FontWeight="SemiBold" Margin="0,15,0,5"/>
            <PasswordBox x:Name="PasswordBox"
                         Height="35"
                         FontSize="14"
                         Padding="5"
                         PasswordChanged="PasswordBox_PasswordChanged"/>

            <TextBlock Text="{Binding ErrorMessage}"
                       Foreground="Red"
                       Margin="0,10,0,0"
                       TextWrapping="Wrap"
                       Visibility="{Binding HasError, Converter={StaticResource BoolToVisibilityConverter}}"/>

            <Button Content="{Binding LoginButtonText}"
                    Command="{Binding LoginCommand}"
                    Height="40"
                    Margin="0,20,0,0"
                    Background="#2196F3"
                    Foreground="White"
                    FontSize="14"
                    FontWeight="SemiBold"
                    IsEnabled="{Binding IsNotLoading}"/>

            <ProgressBar IsIndeterminate="True"
                         Height="3"
                         Margin="0,10,0,0"
                         Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}"/>
        </StackPanel>
    </Grid>
</Window>
```

**ViewModel: `ViewModels/LoginViewModel.cs`**

```csharp
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ITcpNetworkService _networkService;

    [ObservableProperty]
    private string username;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    public bool IsNotLoading => !IsLoading;
    public string LoginButtonText => IsLoading ? "Đang đăng nhập..." : "Đăng nhập";

    public LoginViewModel(IAuthService authService, ITcpNetworkService networkService)
    {
        _authService = authService;
        _networkService = networkService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
            HasError = true;
            return;
        }

        IsLoading = true;

        try
        {
            // Connect to server
            var connected = await _networkService.ConnectAsync();
            if (!connected)
            {
                ErrorMessage = "Không thể kết nối tới server";
                HasError = true;
                return;
            }

            // Login
            var success = await _authService.LoginAsync(Username, Password);

            if (success)
            {
                // Navigate to main window
                var mainWindow = new MainWindow();
                mainWindow.Show();

                // Close login window
                Application.Current.Windows
                    .OfType<LoginWindow>()
                    .FirstOrDefault()?.Close();
            }
            else
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            HasError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

### **Task B2.5: Implement Main Window & Navigation**

**Checklist:**

- [ ] Tạo `MainWindow.xaml` với navigation menu
- [ ] Implement ContentControl cho view switching
- [ ] Create navigation commands
- [ ] Display user info
- [ ] Implement logout

**XAML: `Views/MainWindow.xaml`**

```xml
<Window x:Class="MS2.DesktopApp.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="MS2 - Point of Sale System"
        Height="750"
        Width="1200"
        WindowStartupLocation="CenterScreen"
        WindowState="Maximized">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="250"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Sidebar -->
        <Border Grid.Column="0" Background="#263238" Padding="0">
            <StackPanel>
                <!-- User Info -->
                <Border Background="#1C2529" Padding="15" Margin="0,0,0,10">
                    <StackPanel>
                        <TextBlock Text="{Binding CurrentUser.Username}"
                                   Foreground="White"
                                   FontSize="16"
                                   FontWeight="Bold"/>
                        <TextBlock Text="{Binding CurrentUser.Role}"
                                   Foreground="#B0BEC5"
                                   FontSize="12"
                                   Margin="0,5,0,0"/>
                    </StackPanel>
                </Border>

                <!-- Menu Items -->
                <Button Content="🛒 Bán hàng (POS)"
                        Command="{Binding NavigateToPOSCommand}"
                        Style="{StaticResource MenuButtonStyle}"/>

                <Button Content="📦 Quản lý kho"
                        Command="{Binding NavigateToInventoryCommand}"
                        Style="{StaticResource MenuButtonStyle}"/>

                <Button Content="📊 Báo cáo"
                        Command="{Binding NavigateToReportsCommand}"
                        Style="{StaticResource MenuButtonStyle}"/>

                <Button Content="👥 Nhân viên"
                        Command="{Binding NavigateToEmployeesCommand}"
                        Style="{StaticResource MenuButtonStyle}"
                        Visibility="{Binding IsAdmin, Converter={StaticResource BoolToVisibilityConverter}}"/>

                <Separator Background="#37474F" Margin="10,20"/>

                <Button Content="🚪 Đăng xuất"
                        Command="{Binding LogoutCommand}"
                        Style="{StaticResource MenuButtonStyle}"/>
            </StackPanel>
        </Border>

        <!-- Content Area -->
        <ContentControl Grid.Column="1" Content="{Binding CurrentView}"/>
    </Grid>
</Window>
```

---

### **Task B2.6: Implement POS View (Point of Sale)**

**Checklist:**

- [ ] Tạo `POSView.xaml` và `POSViewModel.cs`
- [ ] Design POS UI: product search, cart, payment
- [ ] Implement barcode scanning (TextBox simulation)
- [ ] Implement add/remove cart items
- [ ] Calculate total
- [ ] Implement checkout command
- [ ] Print receipt

**XAML outline: `Views/POSView.xaml`**

```xml
<UserControl x:Class="MS2.DesktopApp.Views.POSView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="2*"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>

        <!-- Left: Product Search & Selection -->
        <Grid Grid.Column="0" Margin="20">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>

            <!-- Search Bar -->
            <StackPanel Grid.Row="0">
                <TextBlock Text="Quét mã hoặc tìm kiếm sản phẩm" FontSize="16" FontWeight="SemiBold"/>
                <TextBox Text="{Binding SearchKeyword, UpdateSourceTrigger=PropertyChanged}"
                         Height="40"
                         FontSize="16"
                         Margin="0,10,0,0"
                         KeyDown="SearchBox_KeyDown"/>
            </StackPanel>

            <!-- Products Grid -->
            <DataGrid Grid.Row="1"
                      ItemsSource="{Binding Products}"
                      AutoGenerateColumns="False"
                      IsReadOnly="True"
                      SelectionMode="Single"
                      MouseDoubleClick="ProductsGrid_MouseDoubleClick"
                      Margin="0,20,0,0">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="Mã" Binding="{Binding Barcode}" Width="100"/>
                    <DataGridTextColumn Header="Tên sản phẩm" Binding="{Binding Name}" Width="*"/>
                    <DataGridTextColumn Header="Giá" Binding="{Binding Price, StringFormat={}{0:N0} đ}" Width="100"/>
                    <DataGridTextColumn Header="Tồn kho" Binding="{Binding Stock}" Width="80"/>
                </DataGrid.Columns>
            </DataGrid>
        </Grid>

        <!-- Right: Cart & Checkout -->
        <Border Grid.Column="1" BorderBrush="#E0E0E0" BorderThickness="1,0,0,0">
            <Grid Margin="20">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <!-- Cart Header -->
                <TextBlock Grid.Row="0"
                           Text="GIỎ HÀNG"
                           FontSize="18"
                           FontWeight="Bold"
                           Margin="0,0,0,15"/>

                <!-- Cart Items -->
                <ListBox Grid.Row="1"
                         ItemsSource="{Binding CartItems}"
                         DisplayMemberPath="ProductName"/>

                <!-- Total -->
                <StackPanel Grid.Row="2" Margin="0,20,0,0">
                    <Border Background="#F5F5F5" Padding="15">
                        <StackPanel>
                            <TextBlock Text="TỔNG CỘNG" FontSize="14" FontWeight="SemiBold"/>
                            <TextBlock Text="{Binding TotalAmount, StringFormat={}{0:N0} đ}"
                                       FontSize="28"
                                       FontWeight="Bold"
                                       Foreground="#2196F3"
                                       Margin="0,5,0,0"/>
                        </StackPanel>
                    </Border>
                </StackPanel>

                <!-- Actions -->
                <StackPanel Grid.Row="3" Margin="0,15,0,0">
                    <Button Content="THANH TOÁN"
                            Command="{Binding CheckoutCommand}"
                            Height="50"
                            Background="#4CAF50"
                            Foreground="White"
                            FontSize="16"
                            FontWeight="Bold"/>

                    <Button Content="Xóa giỏ hàng"
                            Command="{Binding ClearCartCommand}"
                            Height="35"
                            Margin="0,10,0,0"
                            Background="#F44336"
                            Foreground="White"/>
                </StackPanel>
            </Grid>
        </Border>
    </Grid>
</UserControl>
```

**ViewModel outline: `ViewModels/POSViewModel.cs`**

```csharp
public partial class POSViewModel : ObservableObject
{
    private readonly ITcpNetworkService _networkService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string searchKeyword;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> cartItems = new();

    [ObservableProperty]
    private decimal totalAmount;

    public POSViewModel(ITcpNetworkService networkService, IAuthService authService)
    {
        _networkService = networkService;
        _authService = authService;

        _ = LoadProductsAsync();
    }

    partial void OnSearchKeywordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _ = SearchProductsAsync(value);
        }
    }

    private async Task LoadProductsAsync()
    {
        var response = await _networkService.SendMessageAsync(
            "GET_PRODUCTS",
            null,
            _authService.GetToken());

        if (response.Success)
        {
            var productsList = JsonSerializer.Deserialize<List<Product>>(
                response.Data.ToString());

            Products.Clear();
            foreach (var product in productsList)
            {
                Products.Add(product);
            }
        }
    }

    private async Task SearchProductsAsync(string keyword)
    {
        // Search by barcode first
        var response = await _networkService.SendMessageAsync(
            "GET_PRODUCT_BY_BARCODE",
            new { Barcode = keyword },
            _authService.GetToken());

        if (response.Success)
        {
            var product = JsonSerializer.Deserialize<Product>(response.Data.ToString());
            AddToCart(product);
            SearchKeyword = string.Empty;
        }
        else
        {
            // Search by name
            response = await _networkService.SendMessageAsync(
                "SEARCH_PRODUCTS",
                new { Keyword = keyword },
                _authService.GetToken());

            if (response.Success)
            {
                var productsList = JsonSerializer.Deserialize<List<Product>>(
                    response.Data.ToString());

                Products.Clear();
                foreach (var product in productsList)
                {
                    Products.Add(product);
                }
            }
        }
    }

    public void AddToCart(Product product, int quantity = 1)
    {
        var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            CartItems.Add(new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity
            });
        }

        CalculateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemModel item)
    {
        CartItems.Remove(item);
        CalculateTotal();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        CalculateTotal();
    }

    private void CalculateTotal()
    {
        TotalAmount = CartItems.Sum(x => x.Price * x.Quantity);
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            MessageBox.Show("Giỏ hàng trống", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var currentUser = _authService.GetCurrentUser();
        var employee = await GetEmployeeByUserIdAsync(currentUser.Id);

        var orderDto = new CreateOrderDto
        {
            EmployeeId = employee.Id,
            Items = CartItems.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToList()
        };

        var response = await _networkService.SendMessageAsync(
            "CREATE_ORDER",
            orderDto,
            _authService.GetToken());

        if (response.Success)
        {
            var orderData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                response.Data.ToString());

            var orderId = int.Parse(orderData["OrderId"].ToString());

            MessageBox.Show(
                $"Đơn hàng #{orderId} đã được tạo thành công!\nTổng tiền: {TotalAmount:N0} đ",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            ClearCart();
            await LoadProductsAsync();
        }
        else
        {
            MessageBox.Show(
                $"Lỗi tạo đơn hàng: {response.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

public class CartItemModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
}
```

---

### **Task B2.7: Implement Inventory View**

**Checklist:**

- [ ] Display all products với stock info
- [ ] Implement update stock functionality (Admin only)
- [ ] Implement update price functionality (Admin only)
- [ ] Show low stock warnings
- [ ] Add filtering và sorting

---

### **Task B2.8: Implement Reports View**

**Checklist:**

- [ ] Date range picker
- [ ] Fetch sales reports từ server
- [ ] Display total revenue, order count, average order value
- [ ] Display top selling products (optional)
- [ ] Export to Excel (optional)

---

### **✅ Checkpoint Phase B2:**

- ✅ WPF app running
- ✅ Login working với TCP Server
- ✅ POS functionality complete
- ✅ Inventory management working
- ✅ Reports displaying correctly
- ✅ UI responsive và user-friendly

---

## PHASE B3: TESTING & DEPLOYMENT FLOW B

### **Task B3.1: Integration Testing**

**Checklist:**

- [ ] Test TCP connection stability
- [ ] Test concurrent users
- [ ] Test POS workflow end-to-end
- [ ] Test inventory updates
- [ ] Test reports accuracy
- [ ] Test error handling và reconnection

---

### **Task B3.2: Performance Optimization**

**Checklist:**

- [ ] Optimize TCP message size
- [ ] Add caching cho product list
- [ ] Optimize database queries
- [ ] Add connection pooling

---

### **Task B3.3: Deployment**

**Checklist:**

- [ ] Setup TCP Server như Windows Service
- [ ] Create WPF app installer (ClickOnce / WiX / Inno Setup)
- [ ] Deploy tại cửa hàng
- [ ] Configure firewall rules
- [ ] Test trên production environment
- [ ] Train nhân viên

**Setup Windows Service:**

```bash
# Using NSSM (Non-Sucking Service Manager)
nssm install MS2TcpServer "C:\MS2\MS2.ServerApp.exe"
nssm set MS2TcpServer AppDirectory "C:\MS2"
nssm start MS2TcpServer
```

---

---

# TỔNG KẾT VÀ CHECKLIST TỔNG QUAN

## **Foundation (Phase 0)**

- [ ] Solution structure hoàn chỉnh
- [ ] Database schema created
- [ ] Repository pattern implemented
- [ ] Sample data seeded

## **Flow A: Web Application**

- [ ] Web API với JWT authentication
- [ ] CRUD endpoints cho Products, Orders
- [ ] Blazor app với login/register
- [ ] Product browsing và search
- [ ] Shopping cart functionality
- [ ] Checkout flow
- [ ] Order history
- [ ] Deployed lên production

## **Flow B: Desktop Application**

- [ ] TCP Server running như Windows Service
- [ ] Protocol defined và tested
- [ ] Multi-client support
- [ ] WPF app với login
- [ ] POS functionality complete
- [ ] Inventory management
- [ ] Reports
- [ ] Deployed tại cửa hàng

## **Final Integration**

- [ ] Cả 2 flows hoạt động với cùng database
- [ ] Data consistency maintained
- [ ] Error handling robust
- [ ] Logging và monitoring
- [ ] Documentation complete
- [ ] User training complete

---

# PHỤ LỤC

## **Các DTOs cần thiết**

Tạo thêm các DTOs trong `MS2.Models/DTOs/`:

```csharp
// Auth DTOs
public class LoginRequestDto { ... }
public class RegisterRequestDto { ... }
public class AuthResponseDto { ... }

// Product DTOs
public class ProductDto { ... }
public class CreateProductDto { ... }
public class UpdateProductDto { ... }
public class UpdateProductPriceDto { ... }
public class UpdateProductStockDto { ... }

// Order DTOs
public class OrderDto { ... }
public class CreateOrderDto { ... }
public class OrderItemDto { ... }
public class OrderDetailDto { ... }
```

## **Công cụ cần thiết**

- Visual Studio 2022
- SQL Server / SQL Server LocalDB
- .NET 8 SDK
- Postman (test API)
- SSMS (SQL Server Management Studio)
- NSSM (Windows Service Manager)

## **NuGet Packages Summary**

**MS2.DataAccess:**

- Microsoft.EntityFrameworkCore (8.0.x)
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.EntityFrameworkCore.Design

**MS2.WebAPI:**

- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore
- BCrypt.Net-Next

**MS2.BlazorApp:**

- Blazored.LocalStorage
- Microsoft.AspNetCore.Components.WebAssembly

**MS2.ServerApp:**

- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.Json

**MS2.DesktopApp:**

- CommunityToolkit.Mvvm
- System.Text.Json

---

**Chúc bạn triển khai dự án thành công! 🚀**
