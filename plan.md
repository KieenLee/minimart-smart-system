# KẾ HOẠCH TRIỂN KHAI DỰ ÁN MS2 - MINIMART SMART SYSTEM

**Phiên bản:** 2.5  
**Ngày cập nhật:** 13/02/2026  
**Kiến trúc:** Dual-Path Architecture (Web MVC + TCP Network)

---

## PHASE 0: FOUNDATION - PROGRESS TRACKER

### Tổng quan cập nhật quá trình

- Dự án đã hoàn thành phần scaffold database, entity, DbContext, setup project/solution và các package quan trọng.
- Database sử dụng: **MiniMart_Smart**
- **Repository Pattern đã hoàn thành** với Generic Repository và 5 specific repositories + UnitOfWork.
- **Đang tiến hành:** DTOs và TCP Models - bước cuối cùng của Phase 0.

---

### ❗ Tiến độ/Trạng thái từng Task

## ✅ Task 0.1: Khởi tạo Solution và Projects - HOÀN THÀNH

- ✅ Tạo solution `MS2.sln`
- ✅ Tạo project `MS2.Models` (.NET 8 Class Library)
- ✅ Tạo project `MS2.DataAccess` (.NET 8 Class Library)
- ✅ Add reference: `MS2.DataAccess` → `MS2.Models`
- ✅ Setup `.gitignore` cho .NET

## ✅ Task 0.2: Cài đặt NuGet Packages - HOÀN THÀNH

- ✅ Microsoft.EntityFrameworkCore 8.0.11
- ✅ Microsoft.EntityFrameworkCore.SqlServer
- ✅ Microsoft.EntityFrameworkCore.Tools
- ✅ Microsoft.EntityFrameworkCore.Design
- ✅ Microsoft.Extensions.Configuration
- ✅ Microsoft.Extensions.Configuration.Json

## ✅ Task 0.3: Thiết kế Entities - HOÀN THÀNH

**Database:** MiniMart_Smart (không phải MS2Database như plan ban đầu)

**Entities đã scaffold từ database:**

- ✅ User (Role: Admin/Employee/Customer)
- ✅ Category (ParentCategoryId, tự liên kết)
- ✅ Product (Barcode)
- ✅ CartItem
- ✅ Order (CustomerID, EmployeeID)
- ✅ OrderDetail

**Lưu ý:** Không sử dụng BaseEntity pattern, các entities có fields riêng từ database

## ✅ Task 0.4: Tạo DbContext - HOÀN THÀNH

- ✅ `MS2DbContext.cs` với DbSet, OnModelCreating
- ✅ `appsettings.json`: Server=WIN-R972FJEQE2C\SQLEXPRESS;Database=MiniMart_Smart
- ✅ `MS2DbContextFactory.cs` (IDesignTimeDbContextFactory)
- ✅ Relationships configured (category self-ref, multi-FK từ User)
- ✅ Đã kiểm tra kết nối và DbContext

## ⏭️ Task 0.5: EF Core Migrations - SKIPPED

**Lý do:** Database đã tồn tại → scaffold code, không cần migration

- ✅ Database MiniMart_Smart đã có sẵn
- ✅ Sample data đã seed (6 users, 5 categories, 19 products)

## ✅ Task 0.6: Implement Repository Pattern - HOÀN THÀNH

**Đã tạo structure hoàn chỉnh:**

- Tạo structure:
  ```
  MS2.DataAccess
  ├── Interfaces/
  │ ├── IRepository.cs ✅ Generic Repository Interface
  │ ├── IProductRepository.cs ✅ Product methods
  │ ├── IOrderRepository.cs ✅ Order methods
  │ ├── IUserRepository.cs ✅ User methods
  │ ├── ICategoryRepository.cs ✅ Category methods
  │ ├── ICartItemRepository.cs ✅ CartItem methods
  │ └── IUnitOfWork.cs ✅ UnitOfWork interface
  └── Repositories/
  ├── Repository.cs ✅ Generic implementation
  ├── ProductRepository.cs ✅ GetByBarcode, Search, LowStock
  ├── OrderRepository.cs ✅ GetWithDetails, DateRange, Sales
  ├── UserRepository.cs ✅ GetByUsername, GetByRole
  ├── CategoryRepository.cs ✅ GetRootCategories, SubCategories
  ├── CartItemRepository.cs ✅ GetByUserId, DeleteByUserId
  └── UnitOfWork.cs ✅ Transaction management
  ```

**Tiến độ:** Chưa bắt đầu code, đã xác định thiết kế – ƯU TIÊN TIẾP THEO.

**Features đã implement:**

- ✅ Generic Repository với CRUD cơ bản (GetAll, GetById, Add, Update, Delete)
- ✅ Expression-based queries (FindAsync, CountAsync, GetFirstOrDefaultAsync)
- ✅ Specific methods cho từng repository (GetByBarcode, SearchAsync, GetWithDetailsAsync...)
- ✅ Include navigation properties
- ✅ Transaction support (BeginTransaction/Commit/Rollback)
- ✅ Lazy initialization repositories trong UnitOfWork
- ✅ IDisposable pattern
- ✅ Async/await cho tất cả operations

**Đã kiểm tra và hoạt động ổn định!**

## ✅ Task 0.7: DTOs và TCP Models - HOÀN THÀNH

**Folders đã tạo:**

- MS2.Models/DTOs/Auth/
- MS2.Models/DTOs/Product/
- MS2.Models/DTOs/Order/
- MS2.Models/TCP/

- ✅ Tạo Auth DTOs
  - ✅ LoginRequestDto
  - ✅ LoginResponseDto (sử dụng SessionId thay vì Token)
  - ✅ UserDto
  - ✅ RegisterRequestDto
- ✅ Tạo Product DTOs
  - ✅ ProductDto
  - ✅ CreateProductDto
  - ✅ UpdateProductDto
  - ✅ UpdatePriceDto
  - ✅ UpdateStockDto
- ✅ Tạo Order DTOs
  - ✅ OrderDto
  - ✅ OrderDetailDto
  - ✅ CreateOrderDto
  - ✅ CreateOrderDetailDto
  - ✅ SalesReportDto
- ✅ Tạo TCP Protocol Models
  - ✅ TcpMessage (với SessionId thay vì Token, ToBytes/FromBytes methods)
  - ✅ TcpResponse (với CreateSuccess/CreateError factory methods)
  - ✅ TcpActions (constants cho tất cả actions)

**Đã kiểm tra và hoạt động ổn định!**

---

## 📊 Tổng kết tiến độ PHASE 0

**✅ Đã hoàn thành (100%):**

- Solution structure (MS2.sln, MS2.Models, MS2.DataAccess)
- Database schema (MiniMart_Smart với 6 tables)
- Entity models (6 entities scaffolded)
- DbContext với Factory pattern
- Repository Pattern (5 repositories + generic base)
- Unit of Work Pattern
- DTOs và TCP Models (Session-based authentication)
- Sample data seeded

**⏭️ Tiếp theo:** Phase B1 - Xây dựng TCP Server

---

# TOÀN BỘ KẾ HOẠCH TRIỂN KHAI (GIỮ LẠI ĐỂ THEO DÕI)

---

## TỔNG QUAN

Dự án chia thành 3 phase theo thứ tự triển khai:

1. **Phase 0 - FOUNDATION:** Database, Models, Repositories (Shared cho cả 2 flows)
2. **Phase B - DESKTOP APP (Flow B):** WPF Client + TCP Server (Internal POS System)
3. **Phase A - WEB APP (Flow A):** ASP.NET MVC + Web API (Public Customer Portal)

**Lý do thứ tự:**

- Desktop App (Flow B) là **ưu tiên cao nhất** phục vụ bán hàng tại cửa hàng
- Web App (Flow A) là **bổ sung** mở rộng bán online

---

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
├── Models/
│   ├── TcpSettings.cs            # TCP config (Host, Port)
│   └── UserSession.cs            # Session model
├── Network/                      # NETWORK LAYER
│   ├── TcpServer.cs              # TCP Listener + Client handling
│   └── TcpMessageRouter.cs       # Route messages to Business Services
└── Business/                     # BUSINESS LAYER
    ├── Interfaces/
    │   ├── ISessionManager.cs
    │   ├── IAuthService.cs
    │   ├── IProductService.cs
    │   ├── IOrderService.cs
    │   └── ICategoryService.cs
    └── Services/
        ├── SessionManager.cs     # Session storage & validation
        ├── AuthService.cs        # LOGIN, REGISTER, LOGOUT
        ├── ProductService.cs     # Product operations
        ├── OrderService.cs       # Order operations
        └── CategoryService.cs    # Category operations
```

**Lý do đơn giản hóa:**

- ❌ Bỏ Handlers/ folder - logic tập trung trong các Service files
- ❌ Bỏ Extensions/ folder - DI setup ngay trong Program.cs
- ✅ Tách Network layer (TCP) và Business layer (Logic)
- ✅ Mỗi Service file nhỏ (200-300 LOC), dễ đọc và maintain
- ✅ Dùng lại Repositories từ MS2.DataAccess (không duplicate)

**NuGet Packages:**

- [ ] `Microsoft.Extensions.DependencyInjection`
- [ ] `Microsoft.Extensions.Hosting`
- [ ] `Microsoft.Extensions.Configuration`
- [ ] `Microsoft.Extensions.Configuration.Json`
- [ ] `Microsoft.Extensions.Logging.Console`
- [ ] `BCrypt.Net-Next`

**Todo List:**

- [✅] Tạo project `MS2.ServerApp` (Console App .NET 8)
- [✅] Reference `MS2.Models` và `MS2.DataAccess`
- [✅] Cài đặt tất cả packages (6 packages, không có JWT)
- [✅] Setup `appsettings.json` với TcpSettings và ConnectionString
- [✅] Add project vào solution
- [✅] Tạo folder structure (Models/, Network/, Business/)
- [✅] Tạo Models/TcpSettings.cs
- [✅] Tạo Models/UserSession.cs

**✅ Task B1.1 HOÀN THÀNH!**

**Đã thực hiện:**

- Tạo MS2.ServerApp console project
- Cài đặt 6 packages (không có JWT, sử dụng SessionId)
- Setup appsettings.json với TcpSettings và ConnectionString
- Tạo folder structure: Models/, Network/, Business/Interfaces/, Business/Services/
- Tạo TcpSettings.cs và UserSession.cs

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

## ✅ Task B1.2: Design TCP Message Protocol - HOÀN THÀNH

**Đã thực hiện:**

- ✅ Tạo TcpMessage.cs với ToBytes()/FromBytes() methods
- ✅ Tạo TcpResponse.cs với CreateSuccess()/CreateError() factory methods
- ✅ Tạo TcpActions.cs với tất cả action constants
- ✅ Implement Length-Prefix protocol: [4 bytes length][JSON bytes]
- ✅ JSON serialization/deserialization working

---

## Task B1.2 (Original): Design TCP Message Protocol

**File:** `MS2.Models/TCP/TcpMessage.cs`

**TcpMessage Structure:**

- `string Action` - Action name (e.g., "LOGIN", "GET_PRODUCTS")
- `object Data` - JSON serialized data
- `string? SessionId` - Session identifier (null for LOGIN action)
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

## ✅ Task B1.3: Create Business Interfaces - HOÀN THÀNH

**Đã tạo 5 interfaces:**

- ✅ ISessionManager.cs (6 methods: CreateSession, GetSession, RemoveSession, IsValidSession, GetAllSessions, RemoveExpiredSessions)
- ✅ IAuthService.cs (3 methods: LoginAsync, RegisterAsync, LogoutAsync)
- ✅ IProductService.cs (6 methods: GetProducts, Search, GetByBarcode, UpdatePrice, UpdateStock, GetLowStock)
- ✅ IOrderService.cs (4 methods: CreateOrder, GetOrders, GetOrderDetails, GetSalesReport)
- ✅ ICategoryService.cs (3 methods: GetCategories, GetRootCategories, GetSubCategories)

**Kiến trúc:**

- Tất cả methods return Task<TcpResponse>
- Tất cả methods nhận TcpMessage parameter
- Session validation trong mỗi method (trừ Login)

---

## ✅ Task B1.4: Implement Business Services - HOÀN THÀNH

**Đã implement 5 services:**

**SessionManager.cs (~60 LOC):**

- ConcurrentDictionary<string, UserSession> for thread-safe storage
- GUID SessionId generation
- Session lifecycle management

**AuthService.cs (~150 LOC):**

- LoginAsync: BCrypt password verification, session creation, return SessionId + UserDto
- RegisterAsync: Hash password, create user, save to database
- LogoutAsync: Remove session

**ProductService.cs (~250 LOC):**

- GetProductsAsync: Return all products with Category navigation
- SearchProductsAsync: Keyword search
- GetProductByBarcodeAsync: For barcode scanner
- UpdateProductPriceAsync: Admin only
- UpdateProductStockAsync: Update inventory
- GetLowStockProductsAsync: Low stock alerts

**OrderService.cs (~280 LOC):**

- CreateOrderAsync: Transaction-based order creation with stock validation
- GetOrdersAsync: Return all orders with details
- GetOrderDetailsAsync: Single order with full details
- GetSalesReportAsync: Date range revenue report

**CategoryService.cs (~80 LOC):**

- GetCategoriesAsync: All categories
- GetRootCategoriesAsync: Top-level categories
- GetSubCategoriesAsync: Child categories by parent

**Các lỗi đã sửa (42 lỗi):**

- ✅ Xóa property Unit khỏi ProductDto (Entity không có)
- ✅ Thêm IsActive vào UserDto
- ✅ Sửa UpdateStockDto class name (bị trùng UpdatePriceDto)
- ✅ Thêm UnitPrice vào CreateOrderDetailDto
- ✅ Sửa SaveAsync → SaveChangesAsync
- ✅ Sửa Models.Entities.User → MS2.Models.Entities.User
- ✅ Thêm Context property vào IUnitOfWork và UnitOfWork
- ✅ Sửa OrderDetail creation logic (xóa Discount field)
- ✅ Sửa tất cả property mappings (OrderId → Id, ProductName từ Entity)
- ✅ Thêm GetAllWithDetailsAsync và GetSalesReportAsync vào IOrderRepository
- ✅ Implement 2 methods trong OrderRepository
- ✅ Cài Microsoft.EntityFrameworkCore cho MS2.Models
- ✅ Build thành công!

---

## ✅ Task B1.5: Implement Network Layer - HOÀN THÀNH

**Đã tạo 2 files:**

- ✅ **TcpMessageRouter.cs (~90 LOC):**
  - Switch-based routing theo message.Action
  - Scoped DI cho mỗi request (fresh DbContext)
  - 13 actions mapped: LOGIN, LOGOUT, GET_PRODUCTS, SEARCH_PRODUCTS, GET_PRODUCT_BY_BARCODE, UPDATE_PRODUCT_PRICE, UPDATE_PRODUCT_STOCK, GET_LOW_STOCK_PRODUCTS, CREATE_ORDER, GET_ORDERS, GET_ORDER_DETAILS, GET_SALES_REPORT, GET_CATEGORIES
  - Exception handling với error responses
  - Unknown action fallback

- ✅ **TcpServer.cs (~180 LOC):**
  - TcpListener trên configured host/port
  - AcceptClientsAsync loop cho concurrent clients
  - HandleClientAsync cho mỗi client
  - Length-prefix protocol (read 4 bytes length → read message)
  - Message routing qua TcpMessageRouter
  - Comprehensive logging (connect, disconnect, errors)
  - Graceful shutdown với 5s timeout

**Kiến trúc:**

- Thread-safe: Mỗi client được handle trong Task riêng
- Scoped DI: Mỗi message request có DbContext riêng
- Error handling: Try-catch ở mỗi layer
- Logging: Console logging cho debug và monitor

---

## ✅ Task B1.6: Setup Dependency Injection & Program.cs - HOÀN THÀNH

**Program.cs đã implement:**

- ✅ Host.CreateApplicationBuilder() setup
- ✅ Configuration loading từ appsettings.json
- ✅ Console logging với Information level
- ✅ TcpSettings singleton registration
- ✅ DbContext registration với SQL Server connection
- ✅ Repository registration: IUnitOfWork → UnitOfWork (Scoped)
- ✅ Business Services registration:
  - ISessionManager → SessionManager (Singleton)
  - IAuthService → AuthService (Scoped)
  - IProductService → ProductService (Scoped)
  - IOrderService → OrderService (Scoped)
  - ICategoryService → CategoryService (Scoped)
- ✅ Network Layer registration:
  - TcpMessageRouter (Singleton)
  - TcpServer (Singleton)
- ✅ Graceful shutdown handler (Console.CancelKeyPress)
- ✅ Server lifecycle management

**Build Status:**

- ✅ Build succeeded với 15 nullable warnings (non-critical)
- ✅ Tất cả dependencies resolved
- ✅ Ready to run

---

## ⏸️ Task B1.7: Test TCP Server - CHỜ TESTING

**Test Scenarios đã lên kế hoạch:**

1. **Server Startup Test:** Verify server binds to 127.0.0.1:5000
2. **Connection Test:** Client connect test
3. **LOGIN Action Test:** Valid credentials (admin/admin123)
4. **GET_PRODUCTS Test:** Retrieve products với valid SessionId
5. **Invalid Session Test:** Fake SessionId error handling
6. **Concurrent Client Test:** 2-3 clients simultaneously

**Test Tools:**

- PowerShell test client script (đã chuẩn bị)
- Test-NetConnection cmdlet
- Hoặc tạo Console test app riêng

---

## 📊 Tiến độ Phase B1 - TCP Server (95% hoàn thành)

**✅ Đã hoàn thành:**

- ✅ Task B1.1: Setup project, packages, folder structure
- ✅ Task B1.2: TCP Protocol Models (TcpMessage, TcpResponse, TcpActions)
- ✅ Task B1.3: Business Interfaces (5 interfaces, 22 methods)
- ✅ Task B1.4: Business Services (5 services, ~800 LOC, 42 bugs fixed)
- ✅ Task B1.5: Network Layer (TcpServer, TcpMessageRouter)
- ✅ Task B1.6: Program.cs với DI Container

**🔄 Chờ testing:**

- ⏸️ Task B1.7: Test TCP Server (95% complete, chỉ còn end-to-end testing)

---

## 📊 Tiến độ Phase B2 - WPF Desktop App (✅ 100% HOÀN THÀNH)

**✅ Đã hoàn thành tất cả 8 tasks:**

- ✅ Task B2.1: Setup WPF Project (cấu trúc đơn giản hóa, simplified UI)
- ✅ Task B2.2: TCP Client Service (đọc config từ appsettings.json)
- ✅ Task B2.3: Login Window với MVVM pattern (no decorative UI)
- ✅ Task B2.4: Main Window với Navigation Menu (4 views)
- ✅ Task B2.5: POS View - Point of Sale (search, cart, checkout)
- ✅ Task B2.6: Inventory View - Quản lý kho (update price/stock)
- ✅ Task B2.7: Reports View - Báo cáo (sales by date range)
- ✅ Task B2.8: Employees View - Quản lý nhân viên (list employees, Admin only)

**Code Statistics:**

- PosViewModel: ~300 LOC (search, cart management, checkout)
- InventoryViewModel: ~250 LOC (product editing, price/stock updates)
- ReportsViewModel: ~100 LOC (sales report by date range)
- EmployeesViewModel: ~70 LOC (employee list display)
- Total ViewModels: ~800+ LOC
- Total Views (XAML): ~520+ LOC
- CartItemModel: Local cart model for POS
- MainViewModel: Navigation updated for all 4 views

**Bug Fixes Applied:**

- Fixed TcpClientService config (now reads from appsettings.json)
- Added Microsoft.Extensions.Configuration.Binder package
- Fixed SalesReportDto structure mismatch (single object vs list)
- Fixed ReportsView DataGrid binding (Orders instead of SalesReports)
- All builds successful with zero errors

**⏭️ Next Step:**

- Testing: End-to-end testing với TCP Server
- Database: Ensure sample data exists for testing
- Performance: Test with large datasets
- User acceptance: Gather feedback for UI improvements

## ✅ Checkpoint Phase B1 (Khi hoàn thành 100%)

**Mục tiêu:**

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

## ✅ Task B2.1: Setup WPF Project - HOÀN THÀNH

**Folder Structure (đã đơn giản hóa):**

```
MS2.DesktopApp/                      # WPF .NET 8
├── App.xaml (đã setup Converters)
├── App.xaml.cs (đã setup DI Container)
├── appsettings.json
│
├── DTOs/                            # Data Transfer Objects (trống - dùng MS2.Models)
├── Export/                          # Chức năng export data của Admin
├── Models/                          # ViewModels
│   ├── TcpClientSettings.cs ✅
│   └── LoginViewModel.cs ✅
│
├── Network/                         # TCP Client Layer
│   └── TcpClientService.cs ✅
│
└── Presentation/                    # UI Layer
    ├── Converters/
    │   ├── BoolToVisibilityConverter.cs ✅
    │   └── StringToVisibilityConverter.cs ✅
    ├── LoginWindow.xaml ✅
    ├── LoginWindow.xaml.cs ✅
    ├── POS/                         # Màn hình bán hàng
    ├── Inventory/                   # Quản lý kho
    ├── Reports/                     # Báo cáo
    └── Employees/                   # Quản lý nhân viên
```

**NuGet Packages:**

- ✅ `CommunityToolkit.Mvvm 8.4.0` (MVVM pattern với source generators)
- ✅ `Microsoft.Extensions.DependencyInjection 10.0.3`
- ✅ `Microsoft.Extensions.Configuration 10.0.3`
- ✅ `Microsoft.Extensions.Configuration.Json 10.0.3`
- ✅ `Microsoft.Extensions.Configuration.Binder 10.0.3` (appsettings.json binding)
- ✅ `System.Text.Json 10.0.3`

**Đã thực hiện:**

- ✅ Tạo project `MS2.DesktopApp` (WPF .NET 8)
- ✅ Reference `MS2.Models`
- ✅ Cài đặt tất cả packages (6 packages)
- ✅ Tạo folders: DTOs, Export, Models, Network, Presentation (với subfolders POS, Inventory, Reports, Employees)
- ✅ Add project vào solution
- ✅ Đơn giản hóa cấu trúc: Xóa Converters (không cần decorative UI)
- ✅ Build thành công

---

## ✅ Task B2.2: Implement TCP Client Service - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **TcpClientSettings.cs** - Config model (Host, Port, ConnectTimeoutMs, ReadTimeoutMs)
2. ✅ **TcpClientService.cs** - TCP Client với các methods:
   - `ConnectAsync()` - Kết nối tới server
   - `SendMessageAsync(action, data, sessionId)` - Gửi message và nhận response
   - `Disconnect()` - Ngắt kết nối
   - `IsConnected` property
   - `CurrentSessionId` property - Lưu SessionId sau login

**Tính năng:**

- ✅ Length-prefix protocol (4 bytes + JSON)
- ✅ Thread-safe với SemaphoreSlim
- ✅ Async/await pattern
- ✅ Exception handling
- ✅ Console logging
- ✅ Dispose pattern
- ✅ **Đọc config từ appsettings.json** (Host: 127.0.0.1, Port: 5000)

**Bug Fix Applied:**

- ✅ Fixed: TcpClientService constructor now receives TcpClientSettings from DI
- ✅ Added: Microsoft.Extensions.Configuration.Binder package
- ✅ Updated: App.xaml.cs setup IConfiguration and bind TcpClient section
- ✅ Added: appsettings.json copy to output directory

---

## ✅ Task B2.3: Implement Login Window - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **LoginWindow.xaml** - UI đăng nhập (simplified, no decorative elements)
   - Username TextBox
   - Password PasswordBox
   - Login Button
   - Error Message display
   - Loading ProgressBar (simple)
2. ✅ **LoginWindow.xaml.cs** - Code-behind xử lý PasswordBox binding

3. ✅ **LoginViewModel.cs** - ViewModel với CommunityToolkit.Mvvm
   - Properties: Username, Password, ErrorMessage, IsLoading
   - LoginCommand (RelayCommand async)
   - Validation logic
   - TCP connection và authentication
   - SessionId management

**App.xaml.cs updates:**

- ✅ Setup DI Container (ServiceCollection)
- ✅ Setup IConfiguration với ConfigurationBuilder
- ✅ Bind TcpClient section to TcpClientSettings
- ✅ Register TcpClientSettings (Singleton)
- ✅ Register TcpClientService (Singleton)
- ✅ Register LoginViewModel (Transient)
- ✅ Register LoginWindow (Transient)
- ✅ Show LoginWindow với DataContext binding

**Chức năng:**

- ✅ MVVM pattern hoàn chỉnh với CommunityToolkit source generators
- ✅ Username/Password validation
- ✅ Connect tới TCP Server (đọc từ appsettings.json)
- ✅ Send LOGIN request với TcpActions.LOGIN
- ✅ Parse LoginResponseDto
- ✅ Lưu SessionId và CurrentUser vào TcpClientService
- ✅ Show MainWindow sau khi login thành công
- ✅ Error handling và display
- ✅ Loading indicator (simple, no decorative UI)

---

## ✅ Task B2.4: Implement Main Window - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **MainWindow.xaml** - UI với navigation menu
   - 2-column Grid: Navigation menu (left) + Content area (right)
   - 4 navigation buttons: POS, Quản lý kho, Báo cáo, Nhân viên
   - User info display (Username, Role)
   - Logout button
   - ContentControl để hiển thị các Views
   - Simple styling, no decorative elements

2. ✅ **MainWindow.xaml.cs** - Code-behind minimal

3. ✅ **MainViewModel.cs** - ViewModel với navigation logic
   - Properties: CurrentView, CurrentUser, Username, UserRole
   - Commands: NavigateToPos, NavigateToInventory, NavigateToReports, NavigateToEmployees, LogoutCommand
   - Visibility bindings cho Admin-only features
   - **Updated**: All navigation methods now create ViewModels with proper DataContext

**Chức năng:**

- ✅ Navigation menu với 4 main views
- ✅ ContentControl binding cho dynamic view loading
- ✅ User info display (Username và Role)
- ✅ Logout functionality (close MainWindow, show LoginWindow)
- ✅ Admin-only visibility cho Employees button
- ✅ Simple UI design per user request

---

## ✅ Task B2.5: Implement POS View - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **POS/PosView.xaml** (~200 lines XAML)
   - 3-row layout: Search bar, Main content (2:1 split), Status bar
   - Left: Products ListBox với Add to Cart buttons
   - Right: Cart ListBox với Remove buttons, Total display, Clear/Checkout buttons
   - Product details: Name, Category, Stock, Price
   - Cart items: Quantity x Price = Subtotal
   - Status bar: Gray background với status messages

2. ✅ **CartItemModel.cs** - Local cart model
   - Properties: ProductId, ProductName, UnitPrice, Quantity, Subtotal (calculated)
   - Used by PosViewModel for shopping cart

3. ✅ **Models/PosViewModel.cs** (~300 LOC)
   - Properties: SearchKeyword, Products, CartItems, TotalAmount, IsLoading, StatusMessage
   - Commands:
     - LoadProductsCommand: Load all active products (IsActive && Stock > 0)
     - SearchProductsCommand: Search by keyword
     - AddToCartCommand: Add to cart (increment if exists)
     - RemoveFromCartCommand: Remove item
     - ClearCartCommand: Clear all items
     - CheckoutCommand: Create order via TCP (CREATE_ORDER), clear cart, reload products
   - Features: Auto-calculate total, confirmation dialogs, status messages

**Chức năng:**

- ✅ Load products from TCP Server (GET_PRODUCTS)
- ✅ Search products by keyword
- ✅ Shopping cart management (add, remove, clear)
- ✅ Total calculation
- ✅ Checkout with order creation
- ✅ Stock validation
- ✅ Success/error messages

---

## ✅ Task B2.6: Implement Inventory View - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **Inventory/InventoryView.xaml** (~130 lines XAML)
   - 3-row layout: Search bar, Main content (2:1 split), Status bar
   - Left: DataGrid with 6 columns (ID, Name, Category, Price, Stock, Barcode)
   - Right: Edit panel với NewPrice/NewStock inputs and update buttons
   - Row selection triggers SelectedProduct update

2. ✅ **Models/InventoryViewModel.cs** (~250 LOC)
   - Properties: Products, SelectedProduct, SearchKeyword, NewPrice, NewStock, IsLoading, StatusMessage
   - Commands:
     - LoadProductsCommand: Load all products (including inactive)
     - SearchProductsCommand: Search by keyword
     - SelectProductCommand: Select product for editing (auto-fill NewPrice/NewStock)
     - UpdatePriceCommand: Update price via TCP (UPDATE_PRODUCT_PRICE)
     - UpdateStockCommand: Update stock via TCP (UPDATE_PRODUCT_STOCK)
   - Features: DataGrid selection binding, confirmation dialogs, auto-reload after updates

**Chức năng:**

- ✅ Load all products (including inactive)
- ✅ Search products
- ✅ Select product from DataGrid
- ✅ Update product price (Admin only)
- ✅ Update product stock
- ✅ Validation (price > 0, stock >= 0)
- ✅ Confirmation dialogs
- ✅ Auto-reload after updates

---

## ✅ Task B2.7: Implement Reports View - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **Reports/ReportsView.xaml** (~130 lines XAML)
   - 4-row layout: Title, Date filter, Report data, Status bar
   - Date filter: FromDate và ToDate DatePickers với "Xem báo cáo" button
   - Summary cards: TotalRevenue (Green), TotalOrders (Blue), AverageOrderValue (Orange)
   - DataGrid: 6 columns (Order ID, Date, Customer, Employee, Total, Status)

2. ✅ **Models/ReportsViewModel.cs** (~100 LOC)
   - Properties: FromDate, ToDate, Orders, TotalRevenue, TotalOrders, AverageOrderValue, IsLoading, StatusMessage
   - Commands:
     - LoadReportCommand: Load sales report by date range (GET_SALES_REPORT)
   - Features: DatePicker integration, summary statistics, order list display
   - **Bug Fixed**: Changed from List<SalesReportDto> to single SalesReportDto with Orders list

**Chức năng:**

- ✅ Date range selection (FromDate/ToDate)
- ✅ Load sales report from TCP Server
- ✅ Display summary statistics (Revenue, Orders, Average)
- ✅ Display order details in DataGrid
- ✅ Color-coded summary cards
- ✅ Date validation
- ✅ Status messages

---

## ✅ Task B2.8: Implement Employees View - HOÀN THÀNH

**Files đã tạo:**

1. ✅ **Employees/EmployeesView.xaml** (~60 lines XAML)
   - 3-row layout: Title with reload button, DataGrid, Status bar
   - DataGrid: 8 columns (ID, FullName, Username, Email, Phone, Role, IsActive, CreatedAt)
   - Simple read-only display
   - Admin-only visibility (MainViewModel binding)

2. ✅ **Models/EmployeesViewModel.cs** (~70 LOC)
   - Properties: Employees, IsLoading, StatusMessage
   - Commands:
     - LoadEmployeesCommand: Load all employees (GET_EMPLOYEES)
   - Features: Auto-load on initialization, simple display-only

**Chức năng:**

- ✅ Load all employees from TCP Server
- ✅ Display in DataGrid with 8 columns
- ✅ Auto-load on view open
- ✅ Reload button
- ✅ Admin-only access (controlled by MainViewModel)
- ✅ Simple display-only (no editing yet)

---

## ✅ Phase B2 Summary - WPF Desktop App HOÀN THÀNH

**Total Implementation:**

- ✅ 8 ViewModels created (~800+ LOC total)
  - LoginViewModel, MainViewModel
  - PosViewModel (~300 LOC)
  - InventoryViewModel (~250 LOC)
  - ReportsViewModel (~100 LOC)
  - EmployeesViewModel (~70 LOC)
- ✅ 8 Views created (~520+ LOC XAML total)
  - LoginWindow, MainWindow
  - PosView (~200 lines)
  - InventoryView (~130 lines)
  - ReportsView (~130 lines)
  - EmployeesView (~60 lines)
- ✅ 1 Local Model: CartItemModel
- ✅ TcpClientService with appsettings.json config
- ✅ DI Container setup với Microsoft.Extensions.DependencyInjection
- ✅ All builds successful with zero errors

**Bug Fixes Applied:**

1. TcpClientService config issue - Now reads from appsettings.json
2. SalesReportDto structure mismatch - Fixed binding to single report object
3. ReportsView DataGrid binding - Changed from SalesReports to Orders

**Testing Checklist:**

- [ ] Start TCP Server (MS2.ServerApp)
- [ ] Start Desktop App (MS2.DesktopApp)
- [ ] Test Login (admin/admin123)
- [ ] Test POS: search, cart, checkout
- [ ] Test Inventory: update price/stock
- [ ] Test Reports: date range, summary, orders
- [ ] Test Employees: list display, reload

**⏭️ Next Phase:** Testing và Phase A (Web App)

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
- [ ] Cache SessionId trong AuthService (in-memory)
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
3. Call \_authService.LoginAsync(username, password)
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
│ [Logo] MS2 SYSTEM [User: Admin] [X] │
├────────┬─────────────────────────────────┤
│ │ │
│ Menu │ │
│ │ Content Area │
│ [POS] │ (ContentControl) │
│ [Kho] │ │
│ [BC] │ │
│ [NV] │ │
│ │ │
│ [Exit] │ │
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
│ [Barcode: ________________] [Tìm: __________] │
├────────────────────────┬─────────────────────────┤
│ │ GIỎ HÀNG │
│ DANH SÁCH SẢN PHẨM │ ┌─────────────────┐ │
│ ┌──────────────────┐ │ │ Coca 330ml │ │
│ │ Coca Cola │ │ │ SL: 2 50,000đ │ │
│ │ 25,000đ │ │ └─────────────────┘ │
│ │ [Thêm] │ │ ┌─────────────────┐ │
│ └──────────────────┘ │ │ Snack Oishi │ │
│ │ │ SL: 1 15,000đ │ │
│ │ │ │ │
│ │ └─────────────────┘ │
│ │ │
│ │ TỔNG: 115,000đ │
│ │ [Xóa] [Thanh toán] │
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
4. Call \_orderService.CreateOrderAsync()
5. If success:
   - Show success message
   - Print receipt (optional)
   - Clear cart
   - Reload products list (cập nhật stock)
6. If fail:
   - Show error message

````

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
````

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
