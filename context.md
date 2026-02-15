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
ASP.NET Core MVC với Razor Views (.cshtml)
   <-> Services Layer (Business Logic)
   <-> Database (EF Core + Repositories)
```

**Mục đích:**

- Phục vụ khách hàng truy cập từ Internet
- Backend và Frontend tích hợp trong cùng một project
- Code C# nhúng trực tiếp vào HTML qua Razor syntax (@Model, @foreach, @if...)
- Controllers xử lý logic nghiệp vụ và trả về Views
- KHÔNG sử dụng Web API riêng biệt

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

| Thành phần      | Công nghệ                                                  |
| --------------- | ---------------------------------------------------------- |
| Web UI          | ASP.NET Core MVC với Razor Views (.cshtml) - Tích hợp B&F  |
| Web Backend     | Controllers + Services (Business Logic) trong cùng project |
| Desktop Client  | WPF (.NET 8)                                               |
| Internal Server | Console App (TCP Listener)                                 |
| Communication   | System.Net.Sockets (TCP/IP)                                |
| Data Access     | Entity Framework Core                                      |
| Database        | SQL Server                                                 |
| Security (Web)  | Cookie Authentication                                      |
| Security (TCP)  | Session-based (SessionId)                                  |
| Logging (TCP)   | Console.WriteLine (simplified)                             |

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

## 7. Tiến độ dự án (Cập nhật: 15/02/2026)

### Phase 0: FOUNDATION - ✅ 100% Complete

- ✅ Database: MiniMart_Smart (6 tables, seeded data)
- ✅ MS2.Models: 6 Entities, 15+ DTOs, 3 TCP Models
- ✅ MS2.DataAccess: Repository Pattern, UnitOfWork, 6 Repositories
- ✅ All builds successful

### Phase B1: TCP SERVER - ✅ 100% Complete

- ✅ Task B1.1: Project setup (MS2.ServerApp)
- ✅ Task B1.2: TCP Protocol (TcpMessage, TcpResponse, TcpActions)
- ✅ Task B1.3: Business Interfaces (6 interfaces)
- ✅ Task B1.4: Business Services (6 services, ~900 LOC, BCrypt hashing)
- ✅ Task B1.5: Network Layer (TcpServer, TcpMessageRouter)
- ✅ Task B1.6: Program.cs + DI Container
- ✅ Task B1.7: Testing - Server stable on port 5000

### Phase B2: WPF DESKTOP APP - ✅ 100% Complete

- ✅ Task B2.1: WPF Project setup (simplified structure)
- ✅ Task B2.2: TcpClientService (appsettings.json config)
- ✅ Task B2.3: Login Window (MVVM pattern)
- ✅ Task B2.4: Main Window (role-based navigation)
- ✅ Task B2.5: POS View (quantity input, cart management)
- ✅ Task B2.6: Inventory View (price/stock updates)
- ✅ Task B2.7: Reports View (date range, statistics)
- ✅ Task B2.8: Employees View (search, create employee)
- ✅ Task B2.9: Profile View (user profile management)

**Phase B Features:**

- ✅ 13+ TCP Actions (LOGIN, GET_PRODUCTS, CREATE_ORDER, UPDATE_PRICE, UPDATE_STOCK, GET_SALES_REPORT, GET_EMPLOYEES, SEARCH_USERS, CREATE_USER, UPDATE_USER_PROFILE...)
- ✅ Session-based authentication (SessionId with Address field mapping)
- ✅ Role-based access control (Employee: POS only, Admin: all features)
- ✅ Employee management (search, create with BCrypt hashing)
- ✅ Profile management (edit fullname, email, phone, address, change password)
- ✅ Minimalist UI (no decorative colors, default Windows theme)
- ✅ Real-time cart updates (ObservableObject pattern)
- ✅ Silent error handling
- ✅ Simplified logging (Console.WriteLine with online user count)
- ✅ Single-line log format: IP:Port | Event | Action | Status | Records | Online
- ✅ End-to-end testing successful

### Phase A: WEB APP - ⏸️ Not Started (NEXT PHASE)

**Next Steps:**

1. Setup ASP.NET Core MVC project (MS2.WebApp) - tích hợp Backend & Frontend
2. Implement Cookie authentication (không dùng JWT)
3. Implement Services layer (gọi Repositories trực tiếp)
4. Implement Controllers (xử lý logic và trả về Views)
5. Implement Razor Views (.cshtml) với C# nhúng vào HTML
6. Implement customer-facing features (Product browsing, Cart, Checkout)
7. Session-based shopping cart (không lưu database)

---

## 8. Project Structure

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
│ ├── Entities/ # Domain Entities (scaffolded from DB)
│ │ ├── User.cs # Role: Admin/Employee/Customer
│ │ ├── Category.cs # Self-referencing (ParentCategoryId)
│ │ ├── Product.cs # Barcode, Stock, Price
│ │ ├── CartItem.cs
│ │ ├── Order.cs # CustomerID, EmployeeID
│ │ └── OrderDetail.cs
│ │
│ ├── DTOs/ # Data Transfer Objects
│ │ ├── Auth/
│ │ │ ├── LoginRequestDto.cs
│ │ │ ├── LoginResponseDto.cs # SessionId + UserDto
│ │ │ ├── UserDto.cs # With Address field
│ │ │ ├── CreateUserDto.cs # For employee creation
│ │ │ └── UpdateUserDto.cs # For profile updates
│ │ │
│ │ ├── Product/
│ │ │ ├── ProductDto.cs
│ │ │ ├── CreateProductDto.cs
│ │ │ ├── UpdatePriceDto.cs
│ │ │ └── UpdateStockDto.cs
│ │ │
│ │ └── Order/
│ │ ├── OrderDto.cs
│ │ ├── CreateOrderDto.cs
│ │ ├── CreateOrderDetailDto.cs
│ │ └── SalesReportDto.cs # Revenue, Orders list
│ │
│ └── TCP/ # TCP Protocol Models
│ ├── TcpMessage.cs # Action, Data, SessionId, RequestId
│ ├── TcpResponse.cs # Success, Data, Message
│ └── TcpActions.cs # 13+ action constants
│
│
├── MS2.DataAccess/ # Data Access Layer
│ ├── MS2.DataAccess.csproj
│ ├── appsettings.json # ConnectionString
│ │
│ ├── Data/
│ │ ├── MS2DbContext.cs # EF Core DbContext
│ │ └── MS2DbContextFactory.cs # Design-time factory
│ │
│ ├── Interfaces/ # Repository Interfaces
│ │ ├── IRepository.cs # Generic CRUD
│ │ ├── IProductRepository.cs
│ │ ├── IOrderRepository.cs
│ │ ├── IUserRepository.cs
│ │ ├── ICategoryRepository.cs
│ │ ├── ICartItemRepository.cs
│ │ └── IUnitOfWork.cs
│ │
│ └── Repositories/ # Repository Implementations
│ ├── Repository.cs # Generic base
│ ├── ProductRepository.cs # GetByBarcode, Search, LowStock
│ ├── OrderRepository.cs # GetWithDetails, SalesReport
│ ├── UserRepository.cs # GetByUsername, GetByRole
│ ├── CategoryRepository.cs # GetRootCategories, SubCategories
│ ├── CartItemRepository.cs
│ └── UnitOfWork.cs # Transaction management
│
│
├── MS2.ServerApp/ # Flow B: TCP Server (Console App)
│ ├── MS2.ServerApp.csproj
│ ├── Program.cs # HostBuilder + TcpServer (no logging providers)
│ ├── appsettings.json # TcpSettings (Host: 127.0.0.1, Port: 5000)
│ │
│ ├── Models/
│ │ ├── TcpSettings.cs # TCP config
│ │ └── UserSession.cs # SessionId, User, LoginTime
│ │
│ ├── Network/ # TCP Communication Layer
│ │ ├── TcpServer.cs # TcpListener + Console.WriteLine logging + online count
│ │ └── TcpMessageRouter.cs # Route messages to Business Services
│ │
│ └── Business/ # Business Logic Layer
│ ├── Interfaces/
│ │ ├── ISessionManager.cs
│ │ ├── IAuthService.cs
│ │ ├── IProductService.cs
│ │ ├── IOrderService.cs
│ │ ├── ICategoryService.cs
│ │ └── IUserService.cs # Search, create, update profile
│ └── Services/
│ ├── SessionManager.cs # ConcurrentDictionary sessions
│ ├── AuthService.cs # BCrypt + Address field mapping
│ ├── ProductService.cs
│ ├── OrderService.cs
│ ├── CategoryService.cs
│ └── UserService.cs # Search, create, update profile
│
│
└── MS2.DesktopApp/ # Flow B: WPF Desktop App
├── MS2.DesktopApp.csproj
├── App.xaml # DI Container setup
├── App.xaml.cs
├── appsettings.json # TcpClient config (Host: 127.0.0.1, Port: 5000)
├── AssemblyInfo.cs
│
├── DTOs/ # (Empty - uses MS2.Models)
├── Export/ # Export functionality
│
├── Models/ # ViewModels + Local Models
│ ├── TcpClientSettings.cs # TCP config model
│ ├── CartItemModel.cs # Local cart (ObservableObject)
│ ├── LoginViewModel.cs # Login logic
│ ├── MainViewModel.cs # Navigation + role-based menu
│ ├── PosViewModel.cs # POS logic (~300 LOC)
│ ├── InventoryViewModel.cs # Inventory management (~250 LOC)
│ ├── ReportsViewModel.cs # Sales reports (~100 LOC)
│ ├── EmployeesViewModel.cs # Employee management (~150 LOC)
│ └── ProfileViewModel.cs # Profile editing (~200 LOC)
│
├── Network/ # TCP Client Layer
│ └── TcpClientService.cs # Connect, SendMessage, Disconnect
│
└── Presentation/ # UI Layer (XAML Views)
├── LoginWindow.xaml # Login UI (simplified)
├── LoginWindow.xaml.cs
├── MainWindow.xaml # Main UI with navigation
├── MainWindow.xaml.cs
│
├── POS/
│ ├── PosView.xaml # POS UI with quantity input
│ └── PosView.xaml.cs # Quantity validation
│
├── Inventory/
│ ├── InventoryView.xaml # Product management
│ └── InventoryView.xaml.cs
│
├── Reports/
│ ├── ReportsView.xaml # Sales report
│ └── ReportsView.xaml.cs
│
├── Employees/
│ ├── EmployeesView.xaml # Employee list + search + create
│ └── EmployeesView.xaml.cs # Create employee dialog
│
└── Profile/
├── ProfileView.xaml # User profile editing (all roles)
└── ProfileView.xaml.cs
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
├── MS2.WebApp/ # Flow A: ASP.NET Core MVC (Backend & Frontend tích hợp)
│ ├── MS2.WebApp.csproj
│ ├── appsettings.json
│ ├── Program.cs
│ │
│ ├── Controllers/ # Controllers xử lý logic và trả về Views
│ │ ├── HomeController.cs # Homepage, About
│ │ ├── AccountController.cs # Login, Register, Logout
│ │ ├── ProductsController.cs # Product listing, Search, Details
│ │ ├── CartController.cs # Cart management, Checkout
│ │ ├── OrdersController.cs # Order history, Order details
│ │ └── ProfileController.cs # Customer profile
│ │
│ ├── Services/ # Business Logic Layer
│ │ ├── IAuthService.cs
│ │ ├── AuthService.cs # Login, Register, BCrypt hashing
│ │ ├── IProductService.cs
│ │ ├── ProductService.cs
│ │ ├── IOrderService.cs
│ │ ├── OrderService.cs
│ │ ├── ICartService.cs
│ │ └── CartService.cs # Session-based cart
│ │
│ ├── ViewModels/ # ViewModels cho Views
│ │ ├── LoginViewModel.cs
│ │ ├── RegisterViewModel.cs
│ │ ├── ProductListViewModel.cs
│ │ ├── ProductDetailViewModel.cs
│ │ ├── CartViewModel.cs
│ │ ├── CheckoutViewModel.cs
│ │ └── OrderHistoryViewModel.cs
│ │
│ ├── Views/ # Razor Views (.cshtml)
│ │ ├── Shared/
│ │ │ ├── \_Layout.cshtml
│ │ │ ├── \_LoginPartial.cshtml
│ │ │ └── Error.cshtml
│ │ ├── Home/
│ │ │ ├── Index.cshtml
│ │ │ └── About.cshtml
│ │ ├── Account/
│ │ │ ├── Login.cshtml
│ │ │ └── Register.cshtml
│ │ ├── Products/
│ │ │ ├── Index.cshtml
│ │ │ └── Details.cshtml
│ │ ├── Cart/
│ │ │ ├── Index.cshtml
│ │ │ └── Checkout.cshtml
│ │ ├── Orders/
│ │ │ ├── Index.cshtml
│ │ │ └── Details.cshtml
│ │ └── Profile/
│ │ └── Index.cshtml
│ │
│ ├── Models/
│ │ └── CartItemModel.cs # Session cart model
│ │
│ └── wwwroot/ # Static files
│ ├── css/
│ │ ├── site.css
│ │ └── bootstrap/
│ ├── js/
│ │ ├── site.js
│ │ └── cart.js
│ └── images/
│ └── products/
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
