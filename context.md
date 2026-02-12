# MINI MART SMART SYSTEM (MS2)

## WEB API & TCP NETWORK

---

## 1. Giới thiệu dự án

**MiniMart Smart System (MS2)** là một hệ thống quản lý bán lẻ hiện đại, được thiết kế để giải quyết nhu cầu bán hàng tại quầy và bán hàng trực tuyến.

Dự án tập trung áp dụng **hệ sinh thái .NET** để xây dựng **kiến trúc phân tán** bao gồm:

- Web Application
- Desktop Application
- Backend Server

---

## 2. Kiến trúc luồng dữ liệu (Dual-Path Architecture)

Hệ thống được chia thành **hai kênh độc lập**, cùng kết nối chung một **Database**.

### 2.1. Luồng Public (Web App – Khách hàng)

**Phương thức:**

```
ASP.NET Core MVC (.cshtml) <-> ASP.NET Web API <-> Database (EF Core)
```

**Mục đích:**

- Phục vụ khách hàng truy cập từ Internet
- Thực hiện các thao tác **không yêu cầu kết nối liên tục**

---

### 2.2. Luồng Internal (Desktop App – Nội bộ)

**Phương thức:**

```
Desktop App (WPF)
   <-> TCP Network (Socket)
   <-> ServerApp (Console)
   <-> Database
```

**Mục đích:**

- Tối ưu cho nhân viên bán hàng tại quầy (POS)
- Yêu cầu:
  - Tốc độ phản hồi cực nhanh
  - Khả năng tương tác với thiết bị ngoại vi

---

## 3. Logic kết nối TCP Network

Đây là **thành phần cốt lõi** của luồng nội bộ.

### Mô hình hoạt động

- `ServerApp` khởi tạo một `TcpListener`
- Lắng nghe kết nối từ `Desktop App`

### Quy trình truyền tin

1. Client đóng gói dữ liệu (Object) thành **JSON**
2. Chuỗi JSON được chuyển thành **mảng Byte**
3. Gửi qua `NetworkStream`
4. Server nhận Byte
5. Deserialize dữ liệu
6. Thực thi nghiệp vụ

---

## 4. Tác nhân và Chức năng

| Tác nhân   | Nền tảng      | Chức năng chính                                         |
| ---------- | ------------- | ------------------------------------------------------- |
| Khách hàng | Web (MVC)     | Xem sản phẩm, đặt hàng trực tuyến, xem lịch sử đơn hàng |
| Nhân viên  | Desktop (TCP) | Đăng nhập, bán hàng POS, in hóa đơn, kiểm tra tồn kho   |
| Admin      | Desktop (TCP) | Quản lý nhân viên, chỉnh sửa giá, xem báo cáo doanh thu |

---

## 5. Danh mục công nghệ (Tech Stack)

| Thành phần      | Công nghệ                      |
| --------------- | ------------------------------ |
| Web UI          | ASP.NET Core MVC (.cshtml)     |
| Web Backend     | ASP.NET Core Web API (RESTful) |
| Desktop Client  | WPF (.NET 8)                   |
| Internal Server | Console App (TCP Listener)     |
| Communication   | System.Net.Sockets (TCP/IP)    |
| Data Access     | Entity Framework Core          |
| Database        | SQL Server                     |
| Security (Web)  | JWT Bearer Authentication      |
| Security (TCP)  | Session-based (SessionId)      |

---

## 6. Kỹ thuật & Kiến thức ôn tập

- Entity Framework Core
- LINQ
- Async / Await
- Dependency Injection (Console App)
- Dependency Container
- Interface
- Kiến trúc 3 Layers

---

## 7. Tiến độ dự án (Cập nhật: 13/02/2026)

### Phase 0: FOUNDATION - ✅ 100% Complete

- ✅ Database: MiniMart_Smart (6 tables, seeded data)
- ✅ MS2.Models: 6 Entities, 15 DTOs, 3 TCP Models
- ✅ MS2.DataAccess: Repository Pattern, UnitOfWork, 5 Repositories
- ✅ All builds successful

### Phase B1: TCP SERVER - 🔄 60% Complete

- ✅ Task B1.1: Project setup (MS2.ServerApp)
- ✅ Task B1.2: TCP Protocol (TcpMessage, TcpResponse, TcpActions)
- ✅ Task B1.3: Business Interfaces (5 interfaces)
- ✅ Task B1.4: Business Services (5 services, 800 LOC, 42 bugs fixed)
- ⏸️ Task B1.5: Network Layer (TcpServer, TcpMessageRouter) - IN PROGRESS
- ⏸️ Task B1.6: Program.cs + DI Container
- ⏸️ Task B1.7: Testing

### Phase B2: WPF DESKTOP APP - ⏸️ Not Started

### Phase A: WEB APP - ⏸️ Not Started

**Next Steps:**

1. Complete Network Layer (TcpServer, TcpMessageRouter)
2. Setup DI Container in Program.cs
3. End-to-end testing
4. Start WPF Desktop App development

---

## 8. Project Structure

---

## 7. Tiến độ dự án (Cập nhật: 13/02/2026)

### Phase 0: FOUNDATION - ✅ 100% Complete

- ✅ Database: MiniMart_Smart (6 tables, seeded data)
- ✅ MS2.Models: 6 Entities, 15 DTOs, 3 TCP Models
- ✅ MS2.DataAccess: Repository Pattern, UnitOfWork, 5 Repositories
- ✅ All builds successful

### Phase B1: TCP SERVER - 🔄 60% Complete

- ✅ Task B1.1: Project setup (MS2.ServerApp)
- ✅ Task B1.2: TCP Protocol (TcpMessage, TcpResponse, TcpActions)
- ✅ Task B1.3: Business Interfaces (5 interfaces)
- ✅ Task B1.4: Business Services (5 services, 800 LOC, 42 bugs fixed)
- ⏸️ Task B1.5: Network Layer (TcpServer, TcpMessageRouter) - IN PROGRESS
- ⏸️ Task B1.6: Program.cs + DI Container
- ⏸️ Task B1.7: Testing

### Phase B2: WPF DESKTOP APP - ⏸️ Not Started

### Phase A: WEB APP - ⏸️ Not Started

**Next Steps:**

1. Complete Network Layer (TcpServer, TcpMessageRouter)
2. Setup DI Container in Program.cs
3. End-to-end testing
4. Start WPF Desktop App development

minimart-smart-system/
│
├── .git/
├── .gitignore
├── context.md
├── plan.md
├── README.md
└── MS2.sln
│
├── MS2.Models/ # Shared Models Layer
│ ├── MS2.Models.csproj
│ │
│ ├── Entities/ # Domain Entities
│ │ ├── BaseEntity.cs
│ │ ├── User.cs
│ │ ├── Customer.cs
│ │ ├── Employee.cs
│ │ ├── Category.cs
│ │ ├── Product.cs
│ │ ├── Order.cs
│ │ └── OrderDetail.cs
│ │
│ ├── DTOs/ # Data Transfer Objects
│ │ ├── Auth/
│ │ │ ├── LoginRequestDto.cs
│ │ │ ├── RegisterRequestDto.cs
│ │ │ └── AuthResponseDto.cs
│ │ │
│ │ ├── Product/
│ │ │ ├── ProductDto.cs
│ │ │ ├── CreateProductDto.cs
│ │ │ ├── UpdateProductDto.cs
│ │ │ ├── UpdateProductPriceDto.cs
│ │ │ └── UpdateProductStockDto.cs
│ │ │
│ │ ├── Order/
│ │ │ ├── OrderDto.cs
│ │ │ ├── CreateOrderDto.cs
│ │ │ ├── OrderItemDto.cs
│ │ │ └── OrderDetailDto.cs
│ │ │
│ │ ├── Customer/
│ │ │ ├── CustomerDto.cs
│ │ │ └── CreateCustomerDto.cs
│ │ │
│ │ └── Employee/
│ │ ├── EmployeeDto.cs
│ │ └── CreateEmployeeDto.cs
│ │
│ └── TCP/ # TCP Protocol Models
│ ├── TcpMessage.cs
│ ├── TcpResponse.cs
│ └── TcpActions.cs
│
│
├── MS2.DataAccess/ # Data Access Layer
│ ├── MS2.DataAccess.csproj
│ ├── appsettings.json
│ │
│ ├── Data/
│ │ └── MS2DbContext.cs # EF Core DbContext
│ │
│ ├── Interfaces/ # Repository Interfaces
│ │ ├── IRepository.cs
│ │ ├── IProductRepository.cs
│ │ ├── IOrderRepository.cs
│ │ ├── IUserRepository.cs
│ │ ├── IEmployeeRepository.cs
│ │ ├── ICustomerRepository.cs
│ │ ├── ICategoryRepository.cs
│ │ └── IUnitOfWork.cs
│ │
│ ├── Repositories/ # Repository Implementations
│ │ ├── Repository.cs
│ │ ├── ProductRepository.cs
│ │ ├── OrderRepository.cs
│ │ ├── UserRepository.cs
│ │ ├── EmployeeRepository.cs
│ │ ├── CustomerRepository.cs
│ │ ├── CategoryRepository.cs
│ │ └── UnitOfWork.cs
│ │
│ ├── Migrations/ # EF Core Migrations
│ │ └── (Auto-generated migration files)
│ │
│ └── Seeders/ # Data Seeders
│ └── DataSeeder.cs
│
│
├── MS2.WebAPI/ # Flow A: Web API Backend
│ ├── MS2.WebAPI.csproj
│ ├── appsettings.json
│ ├── appsettings.Development.json
│ ├── Program.cs
│ │
│ ├── Controllers/
│ │ ├── AuthController.cs
│ │ ├── ProductsController.cs
│ │ ├── OrdersController.cs
│ │ ├── CustomersController.cs
│ │ ├── CategoriesController.cs
│ │ └── EmployeesController.cs
│ │
│ ├── Services/ # Business Logic Services
│ │ ├── IJwtTokenService.cs
│ │ ├── JwtTokenService.cs
│ │ ├── IAuthService.cs
│ │ └── AuthService.cs
│ │
│ ├── Models/
│ │ ├── JwtSettings.cs
│ │ └── ApiResponse.cs
│ │
│ ├── Middleware/
│ │ ├── ExceptionHandlingMiddleware.cs
│ │ └── LoggingMiddleware.cs
│ │
│ └── Extensions/
│ ├── ServiceExtensions.cs
│ └── SwaggerExtensions.cs
│
│
├── MS2.ServerApp/ # Flow B: TCP Server (Console App)
│ ├── MS2.ServerApp.csproj
│ ├── Program.cs
│ ├── appsettings.json
│ │
│ ├── Models/
│ │ ├── TcpSettings.cs
│ │ └── UserSession.cs
│ │
│ ├── Network/ # TCP Communication Layer
│ │ ├── TcpServer.cs
│ │ └── TcpMessageRouter.cs
│ │
│ └── Business/ # Business Logic Layer
│ ├── Interfaces/
│ │ ├── ISessionManager.cs
│ │ ├── IAuthService.cs
│ │ ├── IProductService.cs
│ │ ├── IOrderService.cs
│ │ └── ICategoryService.cs
│ └── Services/
│ ├── SessionManager.cs
│ ├── AuthService.cs
│ ├── ProductService.cs
│ ├── OrderService.cs
│ └── CategoryService.cs
│
│
├── MS2.BlazorApp/ # Flow A: Blazor Web App (FUTURE)
│ ├── MS2.BlazorApp.csproj
│ ├── Program.cs
│ ├── App.razor
│ ├── \_Imports.razor
│ ├── Routes.razor
│ │
│ ├── wwwroot/
│ │ ├── appsettings.json
│ │ ├── css/
│ │ │ ├── app.css
│ │ │ └── bootstrap/
│ │ ├── js/
│ │ │ └── site.js
│ │ └── images/
│ │
│ ├── Pages/ # Blazor Pages
│ │ ├── Index.razor
│ │ ├── Login.razor
│ │ ├── Register.razor
│ │ ├── Products.razor
│ │ ├── ProductDetail.razor
│ │ ├── Cart.razor
│ │ ├── Checkout.razor
│ │ ├── Orders.razor
│ │ └── OrderDetail.razor
│ │
│ ├── Components/ # Reusable Components
│ │ ├── Layout/
│ │ │ ├── MainLayout.razor
│ │ │ ├── NavMenu.razor
│ │ │ └── Footer.razor
│ │ │
│ │ ├── Product/
│ │ │ ├── ProductCard.razor
│ │ │ ├── ProductList.razor
│ │ │ └── ProductFilter.razor
│ │ │
│ │ ├── Cart/
│ │ │ ├── CartItem.razor
│ │ │ └── CartSummary.razor
│ │ │
│ │ └── Order/
│ │ ├── OrderItem.razor
│ │ └── OrderSummary.razor
│ │
│ ├── Services/ # HTTP Services
│ │ ├── IAuthService.cs
│ │ ├── AuthService.cs
│ │ ├── IProductService.cs
│ │ ├── ProductService.cs
│ │ ├── IOrderService.cs
│ │ ├── OrderService.cs
│ │ ├── ICartService.cs
│ │ └── CartService.cs
│ │
│ ├── Auth/
│ │ ├── CustomAuthStateProvider.cs
│ │ └── AuthenticationHeaderHandler.cs
│ │
│ └── Models/
│ ├── CartItem.cs
│ └── CheckoutModel.cs
│
│
├── MS2.ServerApp/ # Flow B: TCP Server
│ ├── MS2.ServerApp.csproj
│ ├── Program.cs
│ ├── appsettings.json
│ ├── appsettings.Development.json
│ │
│ ├── Services/
│ │ ├── TcpServer.cs
│ │ ├── ITcpMessageHandler.cs
│ │ ├── TcpMessageHandler.cs
│ │ ├── IJwtTokenService.cs
│ │ └── JwtTokenService.cs
│ │
│ ├── Handlers/ # TCP Action Handlers
│ │ ├── LoginHandler.cs
│ │ ├── ProductHandler.cs
│ │ ├── OrderHandler.cs
│ │ ├── InventoryHandler.cs
│ │ └── ReportHandler.cs
│ │
│ ├── Models/
│ │ ├── TcpSettings.cs
│ │ └── JwtSettings.cs
│ │
│ └── Extensions/
│ └── ServiceExtensions.cs
│
│
└── MS2.DesktopApp/ # Flow B: WPF Desktop App
├── MS2.DesktopApp.csproj
├── App.xaml
├── App.xaml.cs
├── AssemblyInfo.cs
│
├── Views/ # XAML Views
│ ├── LoginWindow.xaml
│ ├── LoginWindow.xaml.cs
│ ├── MainWindow.xaml
│ ├── MainWindow.xaml.cs
│ │
│ ├── POS/
│ │ ├── POSView.xaml
│ │ └── POSView.xaml.cs
│ │
│ ├── Inventory/
│ │ ├── InventoryView.xaml
│ │ ├── InventoryView.xaml.cs
│ │ ├── UpdateStockDialog.xaml
│ │ └── UpdatePriceDialog.xaml
│ │
│ ├── Reports/
│ │ ├── ReportsView.xaml
│ │ ├── ReportsView.xaml.cs
│ │ ├── SalesReportView.xaml
│ │ └── InventoryReportView.xaml
│ │
│ └── Employees/
│ ├── EmployeeManagementView.xaml
│ ├── EmployeeManagementView.xaml.cs
│ ├── AddEmployeeDialog.xaml
│ └── EditEmployeeDialog.xaml
│
├── ViewModels/ # MVVM ViewModels
│ ├── LoginViewModel.cs
│ ├── MainViewModel.cs
│ ├── POSViewModel.cs
│ ├── InventoryViewModel.cs
│ ├── ReportsViewModel.cs
│ └── EmployeeManagementViewModel.cs
│
├── Services/ # WPF Services
│ ├── ITcpNetworkService.cs
│ ├── TcpNetworkService.cs
│ ├── IAuthService.cs
│ ├── AuthService.cs
│ ├── INavigationService.cs
│ ├── NavigationService.cs
│ ├── IDialogService.cs
│ └── DialogService.cs
│
├── Models/
│ ├── AppSettings.cs
│ ├── CartItemModel.cs
│ └── ViewModelBase.cs
│
├── Converters/ # Value Converters
│ ├── BoolToVisibilityConverter.cs
│ ├── DecimalToCurrencyConverter.cs
│ └── NullToVisibilityConverter.cs
│
├── Resources/ # WPF Resources
│ ├── Styles/
│ │ ├── ButtonStyles.xaml
│ │ ├── TextBoxStyles.xaml
│ │ └── DataGridStyles.xaml
│ │
│ ├── Images/
│ │ ├── logo.png
│ │ └── icons/
│ │
│ └── ResourceDictionaries.xaml
│
└── Helpers/
├── RelayCommand.cs
├── AsyncRelayCommand.cs
└── ObservableObject.cs
