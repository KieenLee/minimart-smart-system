# KẾ HOẠCH TRIỂN KHAI DỰ ÁN MS2 - MINIMART SMART SYSTEM

**Phiên bản:** 2.0  
**Ngày cập nhật:** 09/02/2026  
**Kiến trúc:** Dual-Path Architecture (Web MVC + TCP Network)

---

## TỔNG QUAN

Dự án được chia thành **3 phases theo thứ tự triển khai:**

1. **Phase 0 - FOUNDATION:** Database, Models, Repositories (Shared cho cả 2 flows)
2. **Phase B - DESKTOP APP (Flow B):** WPF Client + TCP Server (Internal POS System)
3. **Phase A - WEB APP (Flow A):** ASP.NET MVC + Web API (Public Customer Portal)

**Lý do thứ tự này:**

- Desktop App (Flow B) là **ưu tiên cao nhất** - phục vụ nghiệp vụ bán hàng hàng ngày tại cửa hàng
- Web App (Flow A) là **bổ sung** - mở rộng kênh bán hàng online cho khách hàng

---

# PHASE 0: FOUNDATION - CƠ SỞ HẠ TẦNG CHUNG

> **Mục tiêu:** Tạo database schema, entities, repositories dùng chung cho cả 2 flows

---

## Task 0.1: Khởi tạo Solution và Projects

**Folder Structure:**

```
MS2.sln
├── MS2.Models/              # Shared Models (Class Library .NET 8)
└── MS2.DataAccess/          # Shared Data Access (Class Library .NET 8)
```

**Todo List:**

- [ ] Tạo solution `MS2.sln`
- [ ] Tạo project `MS2.Models` (Class Library .NET 8)
- [ ] Tạo project `MS2.DataAccess` (Class Library .NET 8)
- [ ] Add reference: `MS2.DataAccess` → `MS2.Models`
- [ ] Setup `.gitignore` cho .NET
- [ ] Commit initial structure

**CLI Commands:**

```bash
dotnet new sln -n MS2
dotnet new classlib -n MS2.Models -f net8.0
dotnet new classlib -n MS2.DataAccess -f net8.0
dotnet sln add MS2.Models/MS2.Models.csproj
dotnet sln add MS2.DataAccess/MS2.DataAccess.csproj
dotnet add MS2.DataAccess reference MS2.Models
```

---

## Task 0.2: Cài đặt NuGet Packages

**MS2.Models:**

- [ ] `System.ComponentModel.Annotations`

**MS2.DataAccess:**

- [ ] `Microsoft.EntityFrameworkCore` (8.0.x)
- [ ] `Microsoft.EntityFrameworkCore.SqlServer`
- [ ] `Microsoft.EntityFrameworkCore.Tools`
- [ ] `Microsoft.EntityFrameworkCore.Design`
- [ ] `System.Text.Json`

**CLI Commands:**

```bash
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.SqlServer
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.Tools
dotnet add MS2.DataAccess package Microsoft.EntityFrameworkCore.Design
dotnet add MS2.DataAccess package System.Text.Json
```

---

## Task 0.3: Thiết kế Entities

**Folder Structure:**

```
MS2.Models/
├── Entities/
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Customer.cs
│   ├── Employee.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── OrderDetail.cs
├── DTOs/
│   ├── Auth/
│   ├── Product/
│   ├── Order/
│   ├── Customer/
│   └── Employee/
└── TCP/
    ├── TcpMessage.cs
    ├── TcpResponse.cs
    └── TcpActions.cs
```

**Entities cần tạo:**

**BaseEntity.cs:**

- `int Id`
- `DateTime CreatedAt`
- `DateTime? UpdatedAt`
- `bool IsDeleted`

**User.cs:**

- `int Id`
- `string Username` (Required, MaxLength 50)
- `string PasswordHash` (Required)
- `string Email` (Required, MaxLength 100)
- `string Role` (Required: "Admin", "Employee", "Customer")
- `DateTime CreatedAt`

**Customer.cs:**

- `int Id`
- `int UserId` (FK → User)
- `string FullName` (Required, MaxLength 100)
- `string Phone` (MaxLength 20)
- `string Address` (MaxLength 500)
- `int Points` (Default: 0)
- Navigation: `User`, `List<Order>`

**Employee.cs:**

- `int Id`
- `int UserId` (FK → User)
- `string FullName` (Required, MaxLength 100)
- `string Position` (MaxLength 50)
- `DateTime HireDate`
- `decimal Salary`
- Navigation: `User`, `List<Order>`

**Category.cs:**

- `int Id`
- `string Name` (Required, MaxLength 100)
- `string Description` (MaxLength 500)
- `int? ParentCategoryId` (FK → Category)
- Navigation: `ParentCategory`, `List<SubCategories>`, `List<Product>`

**Product.cs:**

- `int Id`
- `int CategoryId` (FK → Category)
- `string Name` (Required, MaxLength 200)
- `string Description` (MaxLength 1000)
- `decimal Price` (Required)
- `int Stock` (Required)
- `string Barcode` (MaxLength 50, Unique)
- `string ImageUrl` (MaxLength 500)
- Navigation: `Category`, `List<OrderDetail>`

**Order.cs:**

- `int Id`
- `int? CustomerId` (FK → Customer, nullable for guest orders)
- `int? EmployeeId` (FK → Employee, null for online orders)
- `DateTime OrderDate`
- `decimal TotalAmount`
- `string Status` ("Pending", "Processing", "Completed", "Cancelled")
- `string OrderType` ("Online", "POS")
- Navigation: `Customer`, `Employee`, `List<OrderDetail>`

**OrderDetail.cs:**

- `int Id`
- `int OrderId` (FK → Order)
- `int ProductId` (FK → Product)
- `int Quantity`
- `decimal UnitPrice`
- `decimal Subtotal`
- Navigation: `Order`, `Product`

**Todo List:**

- [ ] Tạo tất cả entity classes với Data Annotations
- [ ] Implement Navigation Properties
- [ ] Tạo DTOs cho Auth, Product, Order, Customer, Employee
- [ ] Tạo TCP protocol models (TcpMessage, TcpResponse)

---

## Task 0.4: Tạo DbContext

**File:** `MS2.DataAccess/Data/MS2DbContext.cs`

**Todo List:**

- [ ] Tạo `MS2DbContext` class kế thừa `DbContext`
- [ ] Khai báo DbSet cho tất cả entities
- [ ] Configure relationships trong `OnModelCreating()`
- [ ] Setup cascade delete rules (Restrict cho User relationships)
- [ ] Implement soft delete với query filters: `IsDeleted == false`
- [ ] Tạo `appsettings.json` với connection string

**Connection String:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MS2Database;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

## Task 0.5: Chạy EF Core Migrations

**Todo List:**

- [ ] Tạo migration: `Add-Migration InitialCreate`
- [ ] Review migration code
- [ ] Apply migration: `Update-Database`
- [ ] Verify database trong SSMS/Azure Data Studio
- [ ] Kiểm tra tất cả tables, indexes, foreign keys

**CLI Commands:**

```bash
dotnet ef migrations add InitialCreate --project MS2.DataAccess
dotnet ef database update --project MS2.DataAccess
```

---

## Task 0.6: Implement Repository Pattern

**Folder Structure:**

```
MS2.DataAccess/
├── Interfaces/
│   ├── IRepository.cs
│   ├── IProductRepository.cs
│   ├── IOrderRepository.cs
│   ├── IUserRepository.cs
│   ├── IEmployeeRepository.cs
│   ├── ICustomerRepository.cs
│   ├── ICategoryRepository.cs
│   └── IUnitOfWork.cs
└── Repositories/
    ├── Repository.cs
    ├── ProductRepository.cs
    ├── OrderRepository.cs
    ├── UserRepository.cs
    ├── EmployeeRepository.cs
    ├── CustomerRepository.cs
    ├── CategoryRepository.cs
    └── UnitOfWork.cs
```

**IRepository<T> Methods:**

- `Task<IEnumerable<T>> GetAllAsync()`
- `Task<T> GetByIdAsync(int id)`
- `Task<T> AddAsync(T entity)`
- `Task UpdateAsync(T entity)`
- `Task DeleteAsync(int id)`
- `Task<bool> ExistsAsync(int id)`

**IProductRepository (extends IRepository<Product>):**

- `Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)`
- `Task<Product> GetByBarcodeAsync(string barcode)`
- `Task<IEnumerable<Product>> SearchAsync(string keyword)`
- `Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)`

**IOrderRepository (extends IRepository<Order>):**

- `Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)`
- `Task<IEnumerable<Order>> GetByEmployeeIdAsync(int employeeId)`
- `Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime from, DateTime to)`
- `Task<Order> GetWithDetailsAsync(int orderId)`

**IUserRepository (extends IRepository<User>):**

- `Task<User> GetByUsernameAsync(string username)`
- `Task<User> GetByEmailAsync(string email)`
- `Task<bool> UsernameExistsAsync(string username)`

**Todo List:**

- [ ] Tạo tất cả interfaces trong `Interfaces/`
- [ ] Implement generic `Repository<T>` class
- [ ] Implement specific repositories với methods bổ sung
- [ ] Sử dụng async/await cho tất cả methods

---

## Task 0.7: Implement Unit of Work Pattern

**IUnitOfWork Interface:**

```
Properties:
- IProductRepository Products
- IOrderRepository Orders
- IUserRepository Users
- IEmployeeRepository Employees
- ICustomerRepository Customers
- ICategoryRepository Categories

Methods:
- Task<int> SaveChangesAsync()
- Task BeginTransactionAsync()
- Task CommitTransactionAsync()
- Task RollbackTransactionAsync()
- void Dispose()
```

**Todo List:**

- [ ] Tạo `IUnitOfWork` interface
- [ ] Implement `UnitOfWork` class
- [ ] Inject `MS2DbContext` vào constructor
- [ ] Khởi tạo tất cả repositories trong constructor
- [ ] Implement transaction management
- [ ] Implement IDisposable pattern

---

## Task 0.8: Seed Sample Data

**File:** `MS2.DataAccess/Seeders/DataSeeder.cs`

**Data cần seed:**

**Users:**

- [ ] 1 Admin user (username: `admin`, password: `admin123`)
- [ ] 3-5 Employee users
- [ ] 5-10 Customer users

**Categories:**

- [ ] Đồ uống
- [ ] Snack
- [ ] Bánh kẹo
- [ ] Đồ gia dụng
- [ ] Thực phẩm khô

**Products:**

- [ ] 30-50 products với barcode hợp lệ
- [ ] Phân bố đều qua các categories
- [ ] Stock từ 50-200 items
- [ ] Giá từ 5,000 - 100,000 VNĐ

**Todo List:**

- [ ] Tạo `DataSeeder` static class
- [ ] Method `SeedAsync(MS2DbContext context)`
- [ ] Check if data exists trước khi seed
- [ ] Hash passwords với BCrypt
- [ ] Gọi seeder trong startup

---

## ✅ Checkpoint Foundation

**Sau khi hoàn thành Phase 0:**

- ✅ Solution structure hoàn chỉnh
- ✅ Database với tất cả tables
- ✅ Repository pattern implemented
- ✅ Unit of Work implemented
- ✅ Sample data seeded
- ✅ Có thể test CRUD operations qua Unit Tests (optional)

**→ Bắt đầu Phase B: Desktop App (Flow B)**

---

---

# PHASE B: DESKTOP APP (FLOW B) - INTERNAL PATH

> **Target Users:** Nhân viên POS, Admin, Quản lý kho  
> **Tech Stack:** WPF .NET 8 + TCP Server + Socket Communication  
> **Kiến trúc:** 3-Layer Architecture (Presentation → Business → DataAccess) + DI Container

---

## PHASE B1: XÂY DỰNG TCP SERVER

---

## Task B1.1: Setup TCP Server Project

**Folder Structure:**

```
MS2.ServerApp/                    # Console App .NET 8
├── Program.cs
├── appsettings.json
├── Services/
│   ├── TcpServer.cs
│   ├── ITcpMessageHandler.cs
│   ├── TcpMessageHandler.cs
│   ├── IJwtTokenService.cs
│   └── JwtTokenService.cs
├── Handlers/
│   ├── LoginHandler.cs
│   ├── ProductHandler.cs
│   ├── OrderHandler.cs
│   ├── InventoryHandler.cs
│   └── ReportHandler.cs
├── Models/
│   ├── TcpSettings.cs
│   └── JwtSettings.cs
└── Extensions/
    └── ServiceExtensions.cs
```

**NuGet Packages:**

- [ ] `Microsoft.Extensions.DependencyInjection`
- [ ] `Microsoft.Extensions.Hosting`
- [ ] `Microsoft.Extensions.Configuration`
- [ ] `Microsoft.Extensions.Configuration.Json`
- [ ] `Microsoft.Extensions.Logging.Console`
- [ ] `System.IdentityModel.Tokens.Jwt`
- [ ] `BCrypt.Net-Next`

**Todo List:**

- [ ] Tạo project `MS2.ServerApp` (Console App .NET 8)
- [ ] Reference `MS2.Models` và `MS2.DataAccess`
- [ ] Cài đặt tất cả packages
- [ ] Setup `appsettings.json` với TCP settings và JWT settings
- [ ] Add project vào solution

**CLI Commands:**

```bash
dotnet new console -n MS2.ServerApp -f net8.0
dotnet sln add MS2.ServerApp/MS2.ServerApp.csproj
dotnet add MS2.ServerApp reference MS2.Models
dotnet add MS2.ServerApp reference MS2.DataAccess
dotnet add MS2.ServerApp package Microsoft.Extensions.DependencyInjection
dotnet add MS2.ServerApp package Microsoft.Extensions.Hosting
dotnet add MS2.ServerApp package Microsoft.Extensions.Configuration
dotnet add MS2.ServerApp package Microsoft.Extensions.Configuration.Json
dotnet add MS2.ServerApp package System.IdentityModel.Tokens.Jwt
dotnet add MS2.ServerApp package BCrypt.Net-Next
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
    "Issuer": "MS2TcpServer",
    "Audience": "MS2DesktopApp",
    "ExpirationMinutes": 480
  }
}
```

---

## Task B1.2: Design TCP Message Protocol

**File:** `MS2.Models/TCP/TcpMessage.cs`

**TcpMessage Structure:**

- `string Action` - Action name (e.g., "LOGIN", "GET_PRODUCTS")
- `object Data` - JSON serialized data
- `string Token` - JWT token (null for LOGIN action)
- `string RequestId` - Unique request identifier (GUID)

**TcpResponse Structure:**

- `bool Success`
- `object Data` - Response data
- `string Message` - Error message hoặc success message
- `string RequestId` - Match với request

**Defined Actions:**

- `LOGIN` - Đăng nhập (Data: { Username, Password })
- `GET_PRODUCTS` - Lấy tất cả sản phẩm
- `SEARCH_PRODUCTS` - Tìm kiếm sản phẩm (Data: { Keyword })
- `GET_PRODUCT_BY_BARCODE` - Lấy sản phẩm theo barcode (Data: { Barcode })
- `CREATE_ORDER` - Tạo đơn hàng (Data: CreateOrderDto)
- `UPDATE_PRODUCT_PRICE` - Cập nhật giá (Data: { ProductId, NewPrice })
- `UPDATE_PRODUCT_STOCK` - Cập nhật tồn kho (Data: { ProductId, NewStock })
- `GET_SALES_REPORT` - Báo cáo doanh thu (Data: { FromDate, ToDate })
- `GET_EMPLOYEES` - Danh sách nhân viên
- `GET_INVENTORY` - Thông tin tồn kho

**Todo List:**

- [ ] Tạo `TcpMessage` class với methods `ToBytes()` và `FromBytes()`
- [ ] Tạo `TcpResponse` class với methods `ToBytes()`
- [ ] Tạo `TcpActions` static class với constants
- [ ] Implement JSON serialization/deserialization
- [ ] Implement Length-Prefix protocol: `[4 bytes length][message bytes]`

---

## Task B1.3: Implement TCP Server Core

**File:** `MS2.ServerApp/Services/TcpServer.cs`

**TcpServer Class:**

**Properties:**

- `TcpListener _listener`
- `List<Task> _clientTasks`
- `CancellationTokenSource _cancellationTokenSource`

**Methods:**

- `Task StartAsync()` - Start listening
- `Task AcceptClientsAsync(CancellationToken)` - Accept client loop
- `Task HandleClientAsync(TcpClient, CancellationToken)` - Handle individual client
- `Task<TcpResponse> ProcessMessageAsync(TcpMessage)` - Process message via handler
- `Task StopAsync()` - Graceful shutdown

**Todo List:**

- [ ] Tạo `TcpServer` class
- [ ] Initialize `TcpListener` với config từ appsettings
- [ ] Implement async client acceptance loop
- [ ] Implement multi-client handling (mỗi client 1 Task riêng)
- [ ] Implement graceful shutdown khi Ctrl+C
- [ ] Add logging cho connections, errors

---

## Task B1.4: Implement Message Handlers

**Interface:** `ITcpMessageHandler`

- `Task<TcpResponse> HandleAsync(TcpMessage message)`

**Class:** `TcpMessageHandler`

**Routing Logic:**

```
Action → Method
- LOGIN → HandleLoginAsync()
- GET_PRODUCTS → HandleGetProductsAsync()
- SEARCH_PRODUCTS → HandleSearchProductsAsync()
- GET_PRODUCT_BY_BARCODE → HandleGetProductByBarcodeAsync()
- CREATE_ORDER → HandleCreateOrderAsync()
- UPDATE_PRODUCT_PRICE → HandleUpdateProductPriceAsync()
- UPDATE_PRODUCT_STOCK → HandleUpdateProductStockAsync()
- GET_SALES_REPORT → HandleGetSalesReportAsync()
- GET_EMPLOYEES → HandleGetEmployeesAsync()
- GET_INVENTORY → HandleGetInventoryAsync()
```

**Todo List:**

- [ ] Tạo `ITcpMessageHandler` interface
- [ ] Implement `TcpMessageHandler` class
- [ ] Inject `IUnitOfWork` và `IJwtTokenService`
- [ ] Implement routing switch/case theo Action
- [ ] Validate JWT token cho tất cả actions trừ LOGIN
- [ ] Implement từng handler method
- [ ] Return `TcpResponse.CreateSuccess()` hoặc `TcpResponse.CreateError()`

**Handler Examples:**

**HandleLoginAsync:**

- Deserialize `LoginRequestDto` từ `message.Data`
- Tìm user theo username
- Verify password với BCrypt
- Generate JWT token
- Return token + user info

**HandleGetProductsAsync:**

- Validate JWT token
- Call `_unitOfWork.Products.GetAllAsync()`
- Return products list

**HandleCreateOrderAsync:**

- Validate JWT token
- Deserialize `CreateOrderDto`
- Validate products stock
- Create Order + OrderDetails
- Update product stock
- Use transaction (BeginTransaction/Commit/Rollback)
- Return created order

---

## Task B1.5: Setup Dependency Injection & Program.cs

**Program.cs Structure:**

```csharp
- Configure Host Builder
- Add Configuration (appsettings.json)
- Add Logging (Console, Debug)
- Register DbContext
- Register Repositories (IUnitOfWork, UnitOfWork)
- Register Services (IJwtTokenService, ITcpMessageHandler, TcpServer)
- Configure JwtSettings, TcpSettings
- Start TcpServer
- Handle Ctrl+C for graceful shutdown
```

**Todo List:**

- [ ] Setup `Host.CreateApplicationBuilder()`
- [ ] Configure `appsettings.json` loading
- [ ] Configure Console logging
- [ ] Register `MS2DbContext` với DI
- [ ] Register `IUnitOfWork` → `UnitOfWork` (Scoped)
- [ ] Register `IJwtTokenService` → `JwtTokenService` (Scoped)
- [ ] Register `ITcpMessageHandler` → `TcpMessageHandler` (Scoped)
- [ ] Register `TcpServer` (Singleton)
- [ ] Start server và wait for cancellation
- [ ] Implement graceful shutdown

---

## Task B1.6: Test TCP Server

**Test Console Client:**

**Todo List:**

- [ ] Tạo simple Console test client
- [ ] Test connection tới `127.0.0.1:5000`
- [ ] Test LOGIN action
- [ ] Test GET_PRODUCTS action với token
- [ ] Test CREATE_ORDER action
- [ ] Test error handling (invalid token, wrong action)
- [ ] Test concurrent connections (3-5 clients)
- [ ] Verify database changes

**Test Scenarios:**

1. **Login Test:** Send LOGIN → Nhận token
2. **Products Test:** Send GET_PRODUCTS với token → Nhận danh sách
3. **Barcode Scan Test:** Send GET_PRODUCT_BY_BARCODE → Nhận product
4. **Order Test:** Send CREATE_ORDER → Verify trong database
5. **Invalid Token Test:** Send request với fake token → Nhận error
6. **Concurrent Test:** 5 clients login đồng thời

---

## ✅ Checkpoint Phase B1

**Sau khi hoàn thành:**

- ✅ TCP Server chạy ổn định trên port 5000
- ✅ Multi-client support working
- ✅ Tất cả TCP actions implemented
- ✅ JWT authentication working
- ✅ Database operations successful
- ✅ Error handling robust
- ✅ Logging đầy đủ

**→ Tiếp tục Phase B2: WPF Desktop App**

---

---

## PHASE B2: XÂY DỰNG WPF DESKTOP APPLICATION

---

## Task B2.1: Setup WPF Project với 3-Layer Architecture

**Folder Structure (theo ảnh kiến trúc):**

```
MS2.DesktopApp/                      # WPF .NET 8
├── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
│
├── Business/                        # BUSINESS LAYER
│   ├── DTOs/
│   │   ├── LoginDto.cs
│   │   ├── ProductDto.cs
│   │   └── OrderDto.cs
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IProductService.cs
│   │   ├── IOrderService.cs
│   │   └── INetworkService.cs
│   ├── Repositories/
│   │   ├── AuthRepository.cs
│   │   ├── ProductRepository.cs
│   │   └── OrderRepository.cs
│   └── Services/
│       ├── AuthService.cs
│       ├── ProductService.cs
│       ├── OrderService.cs
│       └── DialogService.cs
│
├── DataAccess/                      # DATA ACCESS LAYER (TCP)
│   ├── Network/
│   │   ├── TcpNetworkService.cs
│   │   └── NetworkConfig.cs
│   └── Repositories/
│       ├── TcpAuthRepository.cs
│       ├── TcpProductRepository.cs
│       └── TcpOrderRepository.cs
│
├── Presentation/                    # PRESENTATION LAYER
│   ├── Views/
│   │   ├── LoginWindow.xaml
│   │   ├── LoginWindow.xaml.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── POS/
│   │   │   ├── POSView.xaml
│   │   │   └── POSView.xaml.cs
│   │   ├── Inventory/
│   │   │   ├── InventoryView.xaml
│   │   │   └── InventoryView.xaml.cs
│   │   ├── Reports/
│   │   │   ├── ReportsView.xaml
│   │   │   └── ReportsView.xaml.cs
│   │   └── Employees/
│   │       ├── EmployeeManagementView.xaml
│   │       └── EmployeeManagementView.xaml.cs
│   │
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── MainViewModel.cs
│   │   ├── POSViewModel.cs
│   │   ├── InventoryViewModel.cs
│   │   ├── ReportsViewModel.cs
│   │   └── EmployeeManagementViewModel.cs
│   │
│   ├── Converters/
│   │   ├── BoolToVisibilityConverter.cs
│   │   └── DecimalToCurrencyConverter.cs
│   │
│   └── Resources/
│       ├── Styles/
│       │   ├── ButtonStyles.xaml
│       │   └── TextBoxStyles.xaml
│       └── Images/
│
├── Models/                          # VIEW MODELS
│   ├── CartItemModel.cs
│   ├── AppSettings.cs
│   └── ViewModelBase.cs
│
└── DependencyInjection/             # DI CONTAINER
    └── ServiceConfiguration.cs
```

**NuGet Packages:**

- [ ] `CommunityToolkit.Mvvm` (cho MVVM pattern)
- [ ] `Microsoft.Extensions.DependencyInjection`
- [ ] `Microsoft.Extensions.Configuration`
- [ ] `Microsoft.Extensions.Configuration.Json`
- [ ] `System.Text.Json`

**Todo List:**

- [ ] Tạo project `MS2.DesktopApp` (WPF .NET 8)
- [ ] Reference `MS2.Models`
- [ ] Cài đặt tất cả packages
- [ ] Tạo tất cả folders theo cấu trúc 3-layer
- [ ] Add project vào solution

**CLI Commands:**

```bash
dotnet new wpf -n MS2.DesktopApp -f net8.0
dotnet sln add MS2.DesktopApp/MS2.DesktopApp.csproj
dotnet add MS2.DesktopApp reference MS2.Models
dotnet add MS2.DesktopApp package CommunityToolkit.Mvvm
dotnet add MS2.DesktopApp package Microsoft.Extensions.DependencyInjection
dotnet add MS2.DesktopApp package Microsoft.Extensions.Configuration
dotnet add MS2.DesktopApp package Microsoft.Extensions.Configuration.Json
dotnet add MS2.DesktopApp package System.Text.Json
```

---

## Task B2.2: Setup Dependency Injection Container

**File:** `DependencyInjection/ServiceConfiguration.cs`

**Services cần register:**

**DataAccess Layer:**

- [ ] `INetworkService` → `TcpNetworkService` (Singleton)
- [ ] `TcpAuthRepository` (Transient)
- [ ] `TcpProductRepository` (Transient)
- [ ] `TcpOrderRepository` (Transient)

**Business Layer:**

- [ ] `IAuthService` → `AuthService` (Singleton)
- [ ] `IProductService` → `ProductService` (Singleton)
- [ ] `IOrderService` → `OrderService` (Singleton)
- [ ] `IDialogService` → `DialogService` (Singleton)

**Presentation Layer:**

- [ ] `LoginViewModel` (Transient)
- [ ] `MainViewModel` (Singleton)
- [ ] `POSViewModel` (Transient)
- [ ] `InventoryViewModel` (Transient)
- [ ] `ReportsViewModel` (Transient)
- [ ] `EmployeeManagementViewModel` (Transient)

**Views:**

- [ ] `LoginWindow` (Transient)
- [ ] `MainWindow` (Singleton)

**Todo List:**

- [ ] Tạo `ServiceConfiguration` static class
- [ ] Method `ConfigureServices(IServiceCollection services)`
- [ ] Register tất cả services theo lifetime phù hợp
- [ ] Configure trong `App.xaml.cs` startup

**App.xaml.cs:**

```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        ServiceConfiguration.ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }
}
```

---

## Task B2.3: Implement DataAccess Layer - TCP Network Service

**File:** `DataAccess/Network/TcpNetworkService.cs`

**Interface:** `INetworkService`

- `Task<bool> ConnectAsync()`
- `Task DisconnectAsync()`
- `Task<TcpResponse> SendMessageAsync(string action, object data, string token)`
- `bool IsConnected { get; }`

**Implementation:**

**Properties:**

- `TcpClient _client`
- `NetworkStream _stream`
- `string _host = "127.0.0.1"`
- `int _port = 5000`
- `SemaphoreSlim _sendLock` (thread-safe sending)

**Methods:**

- `ConnectAsync()` - Connect tới TCP Server
- `DisconnectAsync()` - Close connection
- `SendMessageAsync()` - Send message và receive response
- `ReadLengthPrefixedMessage()` - Đọc message theo protocol
- `WriteLengthPrefixedMessage()` - Ghi message theo protocol

**Todo List:**

- [ ] Implement `INetworkService` interface
- [ ] Implement `TcpNetworkService` class
- [ ] Handle connection errors (auto-reconnect nếu cần)
- [ ] Implement thread-safe message sending
- [ ] Implement length-prefix protocol (4 bytes length + message)
- [ ] Add error handling và logging

---

## Task B2.4: Implement Business Layer - Services

**IAuthService:**

```
Methods:
- Task<bool> LoginAsync(string username, string password)
- void Logout()
- string GetToken()
- User GetCurrentUser()
- bool IsAuthenticated { get; }
```

**IProductService:**

```
Methods:
- Task<List<Product>> GetAllProductsAsync()
- Task<List<Product>> SearchProductsAsync(string keyword)
- Task<Product> GetByBarcodeAsync(string barcode)
- Task<bool> UpdatePriceAsync(int productId, decimal newPrice)
- Task<bool> UpdateStockAsync(int productId, int newStock)
```

**IOrderService:**

```
Methods:
- Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
- Task<List<Order>> GetSalesReportAsync(DateTime from, DateTime to)
```

**Todo List:**

- [ ] Tạo tất cả interfaces trong `Business/Interfaces/`
- [ ] Implement services trong `Business/Services/`
- [ ] Inject repositories vào services
- [ ] Services gọi repositories → repositories gọi NetworkService
- [ ] Cache token trong AuthService (in-memory)
- [ ] Cache products list trong ProductService (optional)

---

## Task B2.5: Implement Presentation Layer - Login Window

**File:** `Presentation/Views/LoginWindow.xaml`

**UI Elements:**

- [ ] TextBox: Username
- [ ] PasswordBox: Password
- [ ] Button: Đăng nhập
- [ ] TextBlock: Error message (Red, Collapsed by default)
- [ ] ProgressRing: Loading indicator
- [ ] Logo/Title: "MS2 - MINIMART SMART SYSTEM"

**File:** `Presentation/ViewModels/LoginViewModel.cs`

**Properties:**

- `string Username` (ObservableProperty)
- `string Password` (ObservableProperty)
- `string ErrorMessage` (ObservableProperty)
- `bool IsLoading` (ObservableProperty)

**Commands:**

- `LoginCommand` (RelayCommand) - Async

**LoginCommand Logic:**

```
1. Validate username/password not empty
2. Set IsLoading = true
3. Call _authService.LoginAsync(username, password)
4. If success:
   - Close LoginWindow
   - Open MainWindow
5. If fail:
   - Show ErrorMessage
6. Set IsLoading = false
```

**Todo List:**

- [ ] Design LoginWindow.xaml UI
- [ ] Create LoginViewModel với CommunityToolkit.Mvvm
- [ ] Implement LoginCommand
- [ ] Bind ViewModel to View (DataContext)
- [ ] Test login flow end-to-end

---

## Task B2.6: Implement Presentation Layer - Main Window

**File:** `Presentation/Views/MainWindow.xaml`

**Layout:**

```
┌──────────────────────────────────────────┐
│  [Logo] MS2 SYSTEM    [User: Admin] [X]  │
├────────┬─────────────────────────────────┤
│        │                                  │
│ Menu   │                                  │
│        │        Content Area              │
│ [POS]  │      (ContentControl)            │
│ [Kho]  │                                  │
│ [BC]   │                                  │
│ [NV]   │                                  │
│        │                                  │
│ [Exit] │                                  │
└────────┴─────────────────────────────────┘
```

**Navigation Menu:**

- [ ] POS (Bán hàng)
- [ ] Inventory (Quản lý kho)
- [ ] Reports (Báo cáo)
- [ ] Employees (Nhân viên) - Admin only
- [ ] Logout

**File:** `Presentation/ViewModels/MainViewModel.cs`

**Properties:**

- `object CurrentView` (ObservableProperty) - Hiển thị view hiện tại
- `User CurrentUser` - Lấy từ AuthService
- `string WelcomeMessage` - "Xin chào, [Username]"

**Commands:**

- `NavigateToPOSCommand` - Load POSView
- `NavigateToInventoryCommand` - Load InventoryView
- `NavigateToReportsCommand` - Load ReportsView
- `NavigateToEmployeesCommand` - Load EmployeeManagementView (Admin only)
- `LogoutCommand` - Logout và về LoginWindow

**Todo List:**

- [ ] Design MainWindow.xaml với Grid layout
- [ ] Create navigation menu với Buttons/ListBox
- [ ] Create MainViewModel
- [ ] Implement navigation commands (thay đổi CurrentView)
- [ ] Bind CurrentView tới ContentControl
- [ ] Test navigation giữa các views

---

## Task B2.7: Implement POS View (Point of Sale)

**File:** `Presentation/Views/POS/POSView.xaml`

**UI Layout:**

```
┌──────────────────────────────────────────────────┐
│  [Barcode: ________________] [Tìm: __________]   │
├────────────────────────┬─────────────────────────┤
│                        │  GIỎ HÀNG               │
│  DANH SÁCH SẢN PHẨM    │  ┌─────────────────┐   │
│  ┌──────────────────┐  │  │ Coca 330ml      │   │
│  │ Coca Cola        │  │  │ SL: 2  50,000đ  │   │
│  │ 25,000đ          │  │  └─────────────────┘   │
│  │ [Thêm]           │  │  ┌─────────────────┐   │
│  └──────────────────┘  │  │ Snack Oishi     │   │
│                        │  │ SL: 1  15,000đ  │   │
│                        │  │                 │   │
│                        │  └─────────────────┘   │
│                        │                         │
│                        │  TỔNG: 115,000đ         │
│                        │  [Xóa] [Thanh toán]    │
└────────────────────────┴─────────────────────────┘
```

**ViewModel:** `POSViewModel.cs`

**Properties:**

- `string SearchKeyword` - Tìm kiếm/Barcode
- `ObservableCollection<Product> Products` - Danh sách sản phẩm
- `ObservableCollection<CartItemModel> CartItems` - Giỏ hàng
- `decimal TotalAmount` - Tổng tiền

**Commands:**

- `SearchProductsCommand` - Tìm theo keyword/barcode
- `AddToCartCommand(Product)` - Thêm vào giỏ
- `RemoveFromCartCommand(CartItemModel)` - Xóa khỏi giỏ
- `ClearCartCommand` - Xóa toàn bộ giỏ
- `CheckoutCommand` - Thanh toán

**CheckoutCommand Logic:**

```
1. Validate cart not empty
2. Show confirmation dialog
3. Create CreateOrderDto với cart items
4. Call _orderService.CreateOrderAsync()
5. If success:
   - Show success message
   - Print receipt (optional)
   - Clear cart
   - Reload products list (cập nhật stock)
6. If fail:
   - Show error message
```

**Todo List:**

- [ ] Design POSView.xaml
- [ ] Create POSViewModel
- [ ] Implement product search (keyword + barcode)
- [ ] Implement cart management
- [ ] Implement checkout flow
- [ ] Add keyboard shortcuts (Enter để scan barcode)
- [ ] Test POS workflow end-to-end

---

## Task B2.8: Implement Inventory View

**File:** `Presentation/Views/Inventory/InventoryView.xaml`

**UI Elements:**

- [ ] DataGrid: Hiển thị tất cả products (Name, Category, Price, Stock, Barcode)
- [ ] TextBox: Search filter
- [ ] Button: Cập nhật giá (Admin only)
- [ ] Button: Cập nhật tồn kho (Admin only)
- [ ] Label: Cảnh báo sản phẩm sắp hết (Stock < 10)

**ViewModel:** `InventoryViewModel.cs`

**Properties:**

- `ObservableCollection<Product> Products`
- `Product SelectedProduct`
- `string SearchKeyword`

**Commands:**

- `LoadProductsCommand` - Load all products
- `SearchCommand` - Filter products
- `UpdatePriceCommand` - Show dialog để update price (Admin only)
- `UpdateStockCommand` - Show dialog để update stock
- `RefreshCommand` - Reload data

**Todo List:**

- [ ] Design InventoryView.xaml với DataGrid
- [ ] Create InventoryViewModel
- [ ] Implement LoadProducts
- [ ] Implement search/filter
- [ ] Create UpdatePriceDialog.xaml (input new price)
- [ ] Create UpdateStockDialog.xaml (input new stock)
- [ ] Show low stock warnings (red color cho Stock < 10)
- [ ] Test inventory management

---

## Task B2.9: Implement Reports View

**File:** `Presentation/Views/Reports/ReportsView.xaml`

**UI Elements:**

- [ ] DatePicker: From Date
- [ ] DatePicker: To Date
- [ ] Button: Xem báo cáo
- [ ] TextBlock: Tổng doanh thu
- [ ] TextBlock: Số đơn hàng
- [ ] TextBlock: Giá trị trung bình/đơn
- [ ] DataGrid: Chi tiết đơn hàng theo ngày

**ViewModel:** `ReportsViewModel.cs`

**Properties:**

- `DateTime FromDate`
- `DateTime ToDate`
- `decimal TotalRevenue`
- `int TotalOrders`
- `decimal AverageOrderValue`
- `ObservableCollection<Order> Orders`

**Commands:**

- `LoadReportCommand` - Load sales report

**Todo List:**

- [ ] Design ReportsView.xaml
- [ ] Create ReportsViewModel
- [ ] Implement LoadReportCommand
- [ ] Call `_orderService.GetSalesReportAsync(from, to)`
- [ ] Calculate statistics
- [ ] Display data trong DataGrid
- [ ] Add export to Excel (optional)

---

## Task B2.10: Implement Employee Management View (Admin only)

**File:** `Presentation/Views/Employees/EmployeeManagementView.xaml`

**UI Elements:**

- [ ] DataGrid: Danh sách nhân viên (FullName, Position, Salary, HireDate)
- [ ] Button: Thêm nhân viên
- [ ] Button: Sửa nhân viên
- [ ] Button: Xóa nhân viên

**ViewModel:** `EmployeeManagementViewModel.cs`

**Properties:**

- `ObservableCollection<Employee> Employees`
- `Employee SelectedEmployee`

**Commands:**

- `LoadEmployeesCommand`
- `AddEmployeeCommand` - Show AddEmployeeDialog
- `EditEmployeeCommand` - Show EditEmployeeDialog
- `DeleteEmployeeCommand` - Confirm và delete

**Todo List:**

- [ ] Design EmployeeManagementView.xaml
- [ ] Create EmployeeManagementViewModel
- [ ] Create AddEmployeeDialog.xaml
- [ ] Create EditEmployeeDialog.xaml
- [ ] Implement CRUD operations
- [ ] Validate user role = "Admin" trước khi show view

---

## ✅ Checkpoint Phase B2

**Sau khi hoàn thành:**

- ✅ WPF app chạy ổn định
- ✅ 3-Layer architecture rõ ràng với DI Container
- ✅ Login working với TCP Server
- ✅ POS functionality hoàn chỉnh (scan barcode, checkout)
- ✅ Inventory management working
- ✅ Reports hiển thị chính xác
- ✅ Employee management (Admin only)
- ✅ UI/UX thân thiện, responsive

**→ Tiếp tục Phase B3: Testing & Deployment**

---

---

## PHASE B3: TESTING & DEPLOYMENT FLOW B

---

## Task B3.1: Integration Testing

**Test Scenarios:**

**TCP Connection:**

- [ ] Desktop App connect tới TCP Server successfully
- [ ] Handle network errors gracefully (server offline)
- [ ] Auto-reconnect khi mất kết nối

**Authentication:**

- [ ] Login với valid credentials → Success
- [ ] Login với invalid credentials → Show error
- [ ] Token được lưu và sử dụng cho các requests sau
- [ ] Logout → Clear token

**POS Workflow:**

- [ ] Scan barcode → Product hiển thị
- [ ] Add products to cart → Cart cập nhật
- [ ] Checkout → Order được tạo trong database
- [ ] Product stock giảm sau khi checkout
- [ ] Print receipt (optional)

**Inventory Management:**

- [ ] Load all products → Display correctly
- [ ] Search products → Filter correctly
- [ ] Update price → Database cập nhật
- [ ] Update stock → Database cập nhật
- [ ] Low stock warning hiển thị

**Reports:**

- [ ] Select date range → Load orders correctly
- [ ] Calculate revenue accurately
- [ ] Export to Excel (optional)

**Concurrent Users:**

- [ ] 3-5 Desktop Apps cùng kết nối tới TCP Server
- [ ] Không bị conflict khi cùng tạo orders

**Todo List:**

- [ ] Tạo test checklist đầy đủ
- [ ] Manual testing tất cả workflows
- [ ] Test error scenarios
- [ ] Test concurrent users
- [ ] Fix bugs nếu có

---

## Task B3.2: Performance Optimization

**Todo List:**

- [ ] Optimize TCP message size (compression nếu cần)
- [ ] Cache products list ở client-side (reduce network calls)
- [ ] Optimize database queries (indexes)
- [ ] Add connection pooling cho DbContext
- [ ] Lazy load images (nếu có ảnh sản phẩm)
- [ ] Measure response time (< 500ms cho mỗi action)

---

## Task B3.3: Deployment

**TCP Server Deployment:**

**Option 1: Windows Service (Khuyến nghị)**

- [ ] Install NSSM (Non-Sucking Service Manager)
- [ ] Convert Console App thành Windows Service
- [ ] Auto-start khi Windows khởi động
- [ ] Configure error recovery (auto-restart on crash)

**CLI Commands:**

```bash
# Install NSSM
choco install nssm

# Create service
nssm install MS2TcpServer "C:\MS2\MS2.ServerApp.exe"
nssm set MS2TcpServer AppDirectory "C:\MS2"
nssm set MS2TcpServer Start SERVICE_AUTO_START
nssm start MS2TcpServer
```

**Option 2: Console App với Task Scheduler**

- [ ] Tạo Task trong Task Scheduler
- [ ] Run at startup với highest privileges
- [ ] Configure restart on failure

**Desktop App Deployment:**

**Option 1: ClickOnce**

- [ ] Configure ClickOnce deployment trong Visual Studio
- [ ] Publish to network share hoặc web server
- [ ] Auto-update support

**Option 2: MSI Installer (WiX Toolset)**

- [ ] Create installer project
- [ ] Include .NET 8 Desktop Runtime
- [ ] Create desktop shortcut
- [ ] Add to Start Menu

**Option 3: Simple ZIP Deploy**

- [ ] Build Release configuration
- [ ] Publish self-contained (.NET runtime included)
- [ ] Zip và distribute
- [ ] Manual installation

**CLI Commands:**

```bash
# Publish self-contained
dotnet publish MS2.DesktopApp/MS2.DesktopApp.csproj -c Release -r win-x64 --self-contained true -o ./publish/DesktopApp

# Publish TCP Server
dotnet publish MS2.ServerApp/MS2.ServerApp.csproj -c Release -r win-x64 --self-contained true -o ./publish/ServerApp
```

**Todo List:**

- [ ] Choose deployment method
- [ ] Configure production database connection string
- [ ] Apply EF migrations lên production database
- [ ] Deploy TCP Server tại server/máy chủ cửa hàng
- [ ] Deploy Desktop App tại các máy POS
- [ ] Configure firewall cho port 5000
- [ ] Test deployment trên production environment
- [ ] Train nhân viên sử dụng hệ thống

---

## Task B3.4: Documentation & Training

**Documentation:**

- [ ] Hướng dẫn cài đặt TCP Server
- [ ] Hướng dẫn cài đặt Desktop App
- [ ] User manual cho nhân viên POS
- [ ] Admin manual (quản lý kho, nhân viên, báo cáo)
- [ ] Troubleshooting guide

**Training:**

- [ ] Train nhân viên sử dụng POS
- [ ] Train Admin quản lý kho
- [ ] Train Admin xem báo cáo
- [ ] Train IT staff troubleshoot issues

---

## ✅ Checkpoint Phase B: DESKTOP APP HOÀN THÀNH

**Hoàn thành:**

- ✅ TCP Server deployed như Windows Service
- ✅ Desktop App deployed tại các máy POS
- ✅ Tất cả tính năng hoạt động ổn định
- ✅ Nhân viên đã được training
- ✅ Documentation hoàn chỉnh

**→ Bắt đầu Phase A: Web App (Flow A) - nếu cần**

---

---

# PHASE A: WEB APP (FLOW A) - PUBLIC PATH

> **Target Users:** Khách hàng trực tuyến  
> **Tech Stack:** ASP.NET Core MVC (.cshtml) + Web API + JWT Authentication  
> **Ưu tiên:** Thấp hơn Desktop App (triển khai sau)

---

## PHASE A1: XÂY DỰNG WEB API BACKEND

---

## Task A1.1: Setup Web API Project

**Folder Structure:**

```
MS2.WebAPI/                    # ASP.NET Core Web API .NET 8
├── Program.cs
├── appsettings.json
├── Controllers/
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   ├── CustomersController.cs
│   └── CategoriesController.cs
├── Services/
│   ├── IJwtTokenService.cs
│   ├── JwtTokenService.cs
│   ├── IAuthService.cs
│   └── AuthService.cs
├── Models/
│   ├── JwtSettings.cs
│   └── ApiResponse.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── LoggingMiddleware.cs
└── Extensions/
    ├── ServiceExtensions.cs
    └── SwaggerExtensions.cs
```

**NuGet Packages:**

- [ ] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] `Swashbuckle.AspNetCore` (Swagger)
- [ ] `BCrypt.Net-Next`
- [ ] `System.IdentityModel.Tokens.Jwt`

**Todo List:**

- [ ] Tạo project `MS2.WebAPI` (ASP.NET Core Web API .NET 8)
- [ ] Reference `MS2.Models` và `MS2.DataAccess`
- [ ] Cài đặt packages
- [ ] Setup `appsettings.json` với JWT settings
- [ ] Add project vào solution

**CLI Commands:**

```bash
dotnet new webapi -n MS2.WebAPI -f net8.0
dotnet sln add MS2.WebAPI/MS2.WebAPI.csproj
dotnet add MS2.WebAPI reference MS2.Models
dotnet add MS2.WebAPI reference MS2.DataAccess
dotnet add MS2.WebAPI package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add MS2.WebAPI package Swashbuckle.AspNetCore
dotnet add MS2.WebAPI package BCrypt.Net-Next
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MS2Database;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForWebAPIAtLeast32Chars!",
    "Issuer": "MS2WebAPI",
    "Audience": "MS2WebApp",
    "ExpirationMinutes": 60
  }
}
```

---

## Task A1.2: Configure JWT Authentication & Swagger

**Program.cs Structure:**

```
- Configure JWT Authentication
- Configure Swagger với JWT support
- Register DbContext
- Register Repositories (IUnitOfWork)
- Register Services (IJwtTokenService, IAuthService)
- Configure CORS
- Add Controllers
- Add Authentication/Authorization middleware
```

**Todo List:**

- [ ] Configure JWT Bearer Authentication
- [ ] Configure Swagger UI với JWT authorization
- [ ] Register `MS2DbContext` với DI
- [ ] Register `IUnitOfWork` → `UnitOfWork`
- [ ] Register `IJwtTokenService` → `JwtTokenService`
- [ ] Configure CORS cho Web App (https://localhost:7001)
- [ ] Add exception handling middleware

---

## Task A1.3: Implement API Controllers

**AuthController:**

**Endpoints:**

- `POST /api/auth/register` - Đăng ký tài khoản khách hàng
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/logout` - Đăng xuất (optional)

**ProductsController:**

**Endpoints:**

- `GET /api/products` - Get all products (public)
- `GET /api/products/{id}` - Get product by id (public)
- `GET /api/products/category/{categoryId}` - Get by category (public)
- `GET /api/products/search?keyword=` - Search (public)
- `POST /api/products` - Create (Admin only)
- `PUT /api/products/{id}` - Update (Admin only)
- `DELETE /api/products/{id}` - Delete (Admin only)

**OrdersController:**

**Endpoints:**

- `GET /api/orders` - Get customer's orders (Authenticated)
- `GET /api/orders/{id}` - Get order details (Authenticated)
- `POST /api/orders` - Create order (Authenticated)
- `PUT /api/orders/{id}/status` - Update status (Admin/Employee only)

**CustomersController:**

**Endpoints:**

- `GET /api/customers/me` - Get current customer info (Authenticated)
- `PUT /api/customers/me` - Update profile (Authenticated)

**CategoriesController:**

**Endpoints:**

- `GET /api/categories` - Get all categories (public)

**Todo List:**

- [ ] Implement tất cả controllers
- [ ] Use `[Authorize]` attribute cho protected endpoints
- [ ] Use `[Authorize(Roles = "Admin")]` cho admin endpoints
- [ ] Return consistent ApiResponse format
- [ ] Add validation (ModelState)
- [ ] Handle exceptions gracefully

---

## Task A1.4: Test Web API với Swagger

**Todo List:**

- [ ] Run Web API project
- [ ] Open Swagger UI (https://localhost:7000/swagger)
- [ ] Test POST /api/auth/register
- [ ] Test POST /api/auth/login → Lấy token
- [ ] Click "Authorize" trong Swagger → Nhập token
- [ ] Test GET /api/products (không cần token)
- [ ] Test GET /api/orders (cần token)
- [ ] Test POST /api/orders (create order)
- [ ] Verify data trong database

---

## ✅ Checkpoint Phase A1

**Sau khi hoàn thành:**

- ✅ Web API running trên https://localhost:7000
- ✅ Swagger UI accessible
- ✅ JWT authentication working
- ✅ Tất cả CRUD endpoints tested
- ✅ Authorization working (Admin, Customer roles)

**→ Tiếp tục Phase A2: ASP.NET Core MVC Web App**

---

---

## PHASE A2: XÂY DỰNG ASP.NET CORE MVC WEB APPLICATION

---

## Task A2.1: Setup MVC Project

**Folder Structure:**

```
MS2.WebApp/                         # ASP.NET Core MVC .NET 8
├── Program.cs
├── appsettings.json
│
├── Controllers/
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── ProductsController.cs
│   ├── CartController.cs
│   ├── OrdersController.cs
│   └── ProfileController.cs
│
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _LoginPartial.cshtml
│   │   └── Error.cshtml
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── About.cshtml
│   ├── Account/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Products/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   ├── Cart/
│   │   ├── Index.cshtml
│   │   └── Checkout.cshtml
│   ├── Orders/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   └── Profile/
│       └── Index.cshtml
│
├── ViewModels/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ProductListViewModel.cs
│   ├── ProductDetailViewModel.cs
│   ├── CartViewModel.cs
│   ├── CheckoutViewModel.cs
│   └── OrderHistoryViewModel.cs
│
├── Services/
│   ├── IApiClient.cs
│   ├── ApiClient.cs
│   ├── IAuthService.cs
│   ├── AuthService.cs
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IOrderService.cs
│   ├── OrderService.cs
│   ├── ICartService.cs
│   └── CartService.cs
│
├── Models/
│   └── CartItemModel.cs
│
└── wwwroot/
    ├── css/
    │   ├── site.css
    │   └── bootstrap/
    ├── js/
    │   ├── site.js
    │   └── cart.js
    └── images/
        └── products/
```

**NuGet Packages:**

- [ ] `Microsoft.AspNetCore.Authentication.Cookies`
- [ ] `System.IdentityModel.Tokens.Jwt`
- [ ] `Newtonsoft.Json` hoặc `System.Text.Json`

**Todo List:**

- [ ] Tạo project `MS2.WebApp` (ASP.NET Core MVC .NET 8)
- [ ] Reference `MS2.Models`
- [ ] Cài đặt packages
- [ ] Setup `appsettings.json` với API base URL
- [ ] Add project vào solution

**CLI Commands:**

```bash
dotnet new mvc -n MS2.WebApp -f net8.0
dotnet sln add MS2.WebApp/MS2.WebApp.csproj
dotnet add MS2.WebApp reference MS2.Models
dotnet add MS2.WebApp package Microsoft.AspNetCore.Authentication.Cookies
```

**appsettings.json:**

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

---

## Task A2.2: Implement API Client Service

**Interface:** `IApiClient`

- `Task<T> GetAsync<T>(string endpoint, string token = null)`
- `Task<T> PostAsync<T>(string endpoint, object data, string token = null)`
- `Task<T> PutAsync<T>(string endpoint, object data, string token = null)`
- `Task<bool> DeleteAsync(string endpoint, string token = null)`

**Implementation:** `ApiClient.cs`

**Todo List:**

- [ ] Inject `HttpClient` với base address từ config
- [ ] Implement generic HTTP methods
- [ ] Add JWT token to Authorization header
- [ ] Handle HTTP errors (404, 401, 500)
- [ ] Deserialize JSON responses

---

## Task A2.3: Implement Authentication (Cookie-based)

**Program.cs:**

```
- Configure Cookie Authentication
- Set login path = /Account/Login
- Set cookie expiration = 60 minutes
- Register services (IApiClient, IAuthService, etc.)
```

**AuthService:**

- `Task<bool> LoginAsync(string username, string password)` - Call API, lưu token vào session/cookie
- `Task<bool> RegisterAsync(RegisterViewModel model)` - Call API register
- `Task LogoutAsync()` - Clear session/cookie
- `string GetToken()` - Lấy token từ session
- `bool IsAuthenticated()` - Check token exists

**Todo List:**

- [ ] Configure Cookie Authentication
- [ ] Implement `IAuthService` → `AuthService`
- [ ] Store JWT token trong Session hoặc encrypted Cookie
- [ ] Implement login/logout logic
- [ ] Test authentication flow

---

## Task A2.4: Implement Home & Products Pages

**HomeController:**

- `Index()` - Hiển thị homepage (featured products)
- `About()` - About page

**ProductsController:**

- `Index(string search, int? categoryId)` - Danh sách sản phẩm (có search + filter)
- `Details(int id)` - Chi tiết sản phẩm

**Views:**

**Home/Index.cshtml:**

- Banner/Hero section
- Featured products (lấy 6-8 sản phẩm)
- Categories list
- Footer

**Products/Index.cshtml:**

- Search bar
- Category filter (sidebar/dropdown)
- Product grid (cards)
- Pagination (optional)

**Products/Details.cshtml:**

- Product image
- Product name, price, description
- Stock availability
- "Thêm vào giỏ" button
- Back to products button

**Todo List:**

- [ ] Create ViewModels (ProductListViewModel, ProductDetailViewModel)
- [ ] Implement controllers
- [ ] Call `_productService.GetAllProductsAsync()`
- [ ] Design Views với Bootstrap
- [ ] Test product browsing

---

## Task A2.5: Implement Shopping Cart

**CartService:**

- `List<CartItemModel> GetCartItems()` - Lấy từ session
- `void AddToCart(Product product, int quantity)` - Thêm vào giỏ
- `void UpdateQuantity(int productId, int quantity)` - Update
- `void RemoveFromCart(int productId)` - Xóa
- `void ClearCart()` - Xóa hết
- `decimal GetTotal()` - Tính tổng

**CartController:**

- `Index()` - Hiển thị giỏ hàng
- `AddToCart(int productId, int quantity)` - POST action
- `UpdateQuantity(int productId, int quantity)` - POST action
- `RemoveFromCart(int productId)` - POST action
- `ClearCart()` - POST action

**Views:**

**Cart/Index.cshtml:**

- Table: Product name, quantity, price, subtotal
- Update quantity buttons (+/-)
- Remove button
- Total amount
- "Tiếp tục mua hàng" button
- "Thanh toán" button → Redirect to Checkout

**Todo List:**

- [ ] Implement `ICartService` → `CartService`
- [ ] Store cart trong Session (serialize to JSON)
- [ ] Create CartController
- [ ] Design Cart/Index.cshtml
- [ ] Add AJAX for update quantity (optional)
- [ ] Test cart functionality

---

## Task A2.6: Implement Checkout & Orders

**CartController:**

- `Checkout()` - GET: Show checkout form
- `Checkout(CheckoutViewModel model)` - POST: Create order

**CheckoutViewModel:**

- `string FullName`
- `string Phone`
- `string Address`
- `string Notes`
- `List<CartItemModel> CartItems` (readonly)
- `decimal TotalAmount` (readonly)

**Checkout Logic:**

```
1. Validate cart not empty
2. Validate delivery info (FullName, Phone, Address)
3. Create CreateOrderDto từ cart
4. Call _orderService.CreateOrderAsync(orderDto, token)
5. If success:
   - Clear cart
   - Redirect to Orders/Success
6. If fail:
   - Show error message
```

**OrdersController:**

- `Index()` - Lịch sử đơn hàng (GET /api/orders)
- `Details(int id)` - Chi tiết đơn hàng (GET /api/orders/{id})
- `Success()` - Order success page

**Views:**

**Cart/Checkout.cshtml:**

- Cart summary (readonly)
- Delivery info form (FullName, Phone, Address, Notes)
- Total amount
- "Đặt hàng" button

**Orders/Index.cshtml:**

- Table: Order ID, Order Date, Total Amount, Status
- "Xem chi tiết" button

**Orders/Details.cshtml:**

- Order info
- Order items table
- Total amount
- Status

**Orders/Success.cshtml:**

- "Đặt hàng thành công!"
- Order ID
- "Xem đơn hàng" button

**Todo List:**

- [ ] Create CheckoutViewModel
- [ ] Implement Checkout actions
- [ ] Implement OrdersController
- [ ] Design all views
- [ ] Test checkout flow end-to-end
- [ ] Require [Authorize] for checkout

---

## Task A2.7: Implement User Profile

**ProfileController:**

- `Index()` - GET: Show profile info (call GET /api/customers/me)
- `Edit()` - GET: Show edit form
- `Edit(CustomerEditViewModel model)` - POST: Update (call PUT /api/customers/me)

**Views:**

**Profile/Index.cshtml:**

- Display: FullName, Email, Phone, Address, Points
- "Chỉnh sửa" button

**Profile/Edit.cshtml:**

- Form: FullName, Phone, Address
- "Lưu" button

**Todo List:**

- [ ] Create ProfileController
- [ ] Create ViewModels
- [ ] Design views
- [ ] Test profile management

---

## Task A2.8: UI/UX Improvements

**Todo List:**

- [ ] Responsive design cho mobile (Bootstrap grid)
- [ ] Add loading spinners khi gọi API
- [ ] Add toastr notifications (success/error)
- [ ] Add product images
- [ ] Add pagination cho products list
- [ ] Add category filter
- [ ] Add sorting (price, name)
- [ ] Optimize images (lazy loading)

---

## ✅ Checkpoint Phase A2

**Sau khi hoàn thành:**

- ✅ MVC Web App chạy ổn định trên https://localhost:7001
- ✅ Authentication working (cookie-based)
- ✅ Product browsing working
- ✅ Shopping cart working
- ✅ Checkout flow complete
- ✅ Order history working
- ✅ User profile working
- ✅ UI/UX responsive và đẹp

**→ Tiếp tục Phase A3: Testing & Deployment**

---

---

## PHASE A3: TESTING & DEPLOYMENT FLOW A

---

## Task A3.1: Integration Testing

**Test Scenarios:**

**Authentication:**

- [ ] Register new customer → Success
- [ ] Login với valid credentials → Success
- [ ] Login với invalid credentials → Error
- [ ] Access protected pages without login → Redirect to login

**Product Browsing:**

- [ ] View all products → Display correctly
- [ ] Search products → Filter correctly
- [ ] View product details → Show correct info
- [ ] Category filter → Works correctly

**Shopping Cart:**

- [ ] Add product to cart → Cart updated
- [ ] Update quantity → Cart updated
- [ ] Remove from cart → Cart updated
- [ ] Cart persists across page reloads (session)

**Checkout:**

- [ ] Checkout with valid info → Order created
- [ ] Verify order trong database
- [ ] Cart cleared after checkout
- [ ] Redirect to success page

**Order History:**

- [ ] View orders → Display customer's orders only
- [ ] View order details → Show correct info

**Profile:**

- [ ] View profile → Display correct info
- [ ] Update profile → Database updated

**Todo List:**

- [ ] Create test checklist
- [ ] Manual testing tất cả workflows
- [ ] Test trên multiple browsers (Chrome, Firefox, Edge)
- [ ] Test trên mobile devices
- [ ] Fix bugs

---

## Task A3.2: Performance & Security

**Todo List:**

- [ ] Enable HTTPS
- [ ] Add CSRF protection (AntiForgeryToken)
- [ ] Sanitize user inputs
- [ ] Add rate limiting (optional)
- [ ] Optimize images (compression, CDN)
- [ ] Enable caching (ResponseCache attribute)
- [ ] Minify CSS/JS
- [ ] Add Content Security Policy headers

---

## Task A3.3: Deployment

**Option 1: IIS (Windows Server)**

**Todo List:**

- [ ] Publish Web API: `dotnet publish -c Release`
- [ ] Publish Web App: `dotnet publish -c Release`
- [ ] Create IIS sites (separate sites hoặc sub-apps)
- [ ] Configure application pools (.NET CLR version: No Managed Code)
- [ ] Setup HTTPS certificates
- [ ] Configure connection strings cho production database
- [ ] Apply EF migrations: `dotnet ef database update`
- [ ] Test deployed apps

**Option 2: Azure App Service**

**Todo List:**

- [ ] Create Azure App Service cho Web API
- [ ] Create Azure App Service cho Web App
- [ ] Create Azure SQL Database
- [ ] Configure connection strings trong App Settings
- [ ] Publish Web API từ Visual Studio hoặc CLI
- [ ] Publish Web App từ Visual Studio hoặc CLI
- [ ] Apply EF migrations
- [ ] Configure custom domain (optional)
- [ ] Enable Application Insights (monitoring)

**CLI Commands:**

```bash
# Publish Web API
dotnet publish MS2.WebAPI/MS2.WebAPI.csproj -c Release -o ./publish/WebAPI

# Publish Web App
dotnet publish MS2.WebApp/MS2.WebApp.csproj -c Release -o ./publish/WebApp

# Deploy to Azure (nếu dùng Azure CLI)
az webapp deploy --resource-group MS2ResourceGroup --name ms2-webapi --src-path ./publish/WebAPI
az webapp deploy --resource-group MS2ResourceGroup --name ms2-webapp --src-path ./publish/WebApp
```

---

## Task A3.4: Monitoring & Maintenance

**Todo List:**

- [ ] Setup logging (Serilog, NLog)
- [ ] Setup application monitoring (Application Insights, ELK)
- [ ] Setup database backups (automatic)
- [ ] Create admin dashboard (optional)
- [ ] Document deployment process
- [ ] Create runbook cho common issues

---

## ✅ Checkpoint Phase A: WEB APP HOÀN THÀNH

**Hoàn thành:**

- ✅ Web API deployed (IIS/Azure)
- ✅ Web MVC App deployed (IIS/Azure)
- ✅ Database deployed (SQL Server/Azure SQL)
- ✅ HTTPS configured
- ✅ Monitoring setup
- ✅ Tất cả tính năng working
- ✅ Documentation complete

---

---

# TỔNG KẾT TOÀN BỘ DỰ ÁN

---

## Final Project Structure

```
MS2.sln
│
├── MS2.Models/                    # Shared Models Layer
├── MS2.DataAccess/                # Shared Data Access Layer
│
├── MS2.ServerApp/                 # Flow B: TCP Server (Console)
├── MS2.DesktopApp/                # Flow B: WPF Desktop App
│
├── MS2.WebAPI/                    # Flow A: Web API Backend
└── MS2.WebApp/                    # Flow A: ASP.NET MVC Web App
```

---

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    INTERNET                             │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────▼──────────┐
         │   MS2.WebApp (MVC)   │  (Public Web - Port 443)
         │  https://ms2.com     │
         └───────────┬──────────┘
                     │
         ┌───────────▼──────────┐
         │   MS2.WebAPI         │  (REST API - Port 7000)
         └───────────┬──────────┘
                     │
         ┌───────────▼──────────┐
         │   SQL Server         │  (Database)
         │   MS2Database        │
         └───────────▲──────────┘
                     │
         ┌───────────┴──────────┐
         │   MS2.ServerApp      │  (TCP Server - Port 5000)
         │   (Windows Service)  │
         └───────────▲──────────┘
                     │
     ┌───────────────┼───────────────┐
     │               │               │
┌────▼────┐     ┌────▼────┐     ┌────▼────┐
│ Desktop │     │ Desktop │     │ Desktop │  (WPF Clients)
│  App 1  │     │  App 2  │     │  App 3  │
│  (POS)  │     │  (POS)  │     │ (Admin) │
└─────────┘     └─────────┘     └─────────┘
```

---

## Checklist Tổng Quan

### **Phase 0: Foundation**

- [ ] ✅ Solution structure
- [ ] ✅ Database với EF Core
- [ ] ✅ Entities & DTOs
- [ ] ✅ Repository Pattern
- [ ] ✅ Unit of Work
- [ ] ✅ Sample data

### **Phase B: Desktop App (Flow B) - Ưu tiên cao**

- [ ] ✅ TCP Server deployed như Windows Service
- [ ] ✅ WPF Desktop App deployed tại các máy POS
- [ ] ✅ 3-Layer Architecture với DI Container
- [ ] ✅ POS functionality (scan barcode, checkout)
- [ ] ✅ Inventory management
- [ ] ✅ Reports
- [ ] ✅ Employee management (Admin)
- [ ] ✅ Nhân viên trained

### **Phase A: Web App (Flow A) - Ưu tiên thấp**

- [ ] ✅ Web API deployed
- [ ] ✅ ASP.NET MVC Web App deployed
- [ ] ✅ Authentication (Cookie + JWT)
- [ ] ✅ Product browsing
- [ ] ✅ Shopping cart
- [ ] ✅ Checkout & Orders
- [ ] ✅ User profile
- [ ] ✅ Responsive design

### **Final Integration**

- [ ] ✅ Cả 2 flows hoạt động với cùng database
- [ ] ✅ Data consistency maintained
- [ ] ✅ No conflicts giữa POS và Web orders
- [ ] ✅ Performance optimized
- [ ] ✅ Security hardened
- [ ] ✅ Monitoring & logging
- [ ] ✅ Documentation complete

---

## Công cụ & Technologies Summary

| Layer               | Technology                                       |
| ------------------- | ------------------------------------------------ |
| **Shared**          | .NET 8, EF Core, SQL Server                      |
| **Desktop Client**  | WPF, CommunityToolkit.Mvvm, 3-Layer Architecture |
| **Internal Server** | Console App, TCP/IP Sockets, DI Container        |
| **Web Backend**     | ASP.NET Core Web API, JWT, Swagger               |
| **Web Frontend**    | ASP.NET Core MVC, Razor Views, Bootstrap         |
| **Database**        | SQL Server, EF Core Migrations                   |
| **Security**        | JWT Bearer, BCrypt, Cookie Authentication        |
| **Deployment**      | IIS, Azure App Service, Windows Service          |

---

## Thời gian ước tính

| Phase                                 | Estimated Time |
| ------------------------------------- | -------------- |
| Phase 0: Foundation                   | 3-5 days       |
| Phase B1: TCP Server                  | 3-4 days       |
| Phase B2: WPF Desktop App             | 5-7 days       |
| Phase B3: Testing & Deployment Flow B | 2-3 days       |
| **Total Flow B**                      | **13-19 days** |
| Phase A1: Web API                     | 2-3 days       |
| Phase A2: ASP.NET MVC Web App         | 4-6 days       |
| Phase A3: Testing & Deployment Flow A | 2-3 days       |
| **Total Flow A**                      | **8-12 days**  |
| **GRAND TOTAL**                       | **24-36 days** |

---

## Notes

**Ưu tiên triển khai:**

1. **Phase 0 (Foundation)** - Bắt buộc đầu tiên
2. **Phase B (Desktop App)** - Ưu tiên cao, phục vụ bán hàng hàng ngày
3. **Phase A (Web App)** - Ưu tiên thấp hơn, mở rộng kênh online

**Có thể triển khai từng phần:**

- Sau khi hoàn thành Phase 0 + Phase B → Hệ thống POS đã có thể vận hành
- Phase A có thể triển khai sau, độc lập với Flow B

**Lưu ý bảo mật:**

- JWT tokens khác nhau cho TCP Server và Web API (different secrets)
- TCP Server chỉ listen trên internal network (127.0.0.1 hoặc private IP)
- Web API expose ra internet cần hardening (rate limiting, HTTPS, CORS)

---

**CHÚC BẠN TRIỂN KHAI DỰ ÁN THÀNH CÔNG! 🚀**

---
