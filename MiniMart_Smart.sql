USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MiniMart_Smart')
BEGIN
    ALTER DATABASE MiniMart_Smart SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MiniMart_Smart;
END
GO

CREATE DATABASE MiniMart_Smart;
GO

USE MiniMart_Smart;
GO


-- Users (Customer + Employee + Admin)
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Address NVARCHAR(500) NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'Customer' CHECK (Role IN ('Admin', 'Employee', 'Customer')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- Categories
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ParentCategoryId INT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
);
GO

-- Products
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    Barcode NVARCHAR(50) NULL UNIQUE,
    ImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
GO

-- CartItems
CREATE TABLE CartItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    UNIQUE(UserId, ProductId)
);
GO

-- Orders
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId INT NOT NULL,
    EmployeeId INT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Completed', 'Cancelled')),
    OrderType NVARCHAR(20) NOT NULL DEFAULT 'Online' CHECK (OrderType IN ('Online', 'POS')),
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Users(Id)
);
GO

-- OrderDetails
CREATE TABLE OrderDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

-- SEED DATA
INSERT INTO Users (Username, PasswordHash, Email, FullName, Phone, Address, Role) VALUES
-- Admin (password: admin123)
('admin', '$2a$11$N9qo8uLOickgx2ZMRZoMye7FQXbXvK7hP4aJJQq6W6hPJGxqS8Cte', 'admin@minimart.com', N'Admin', '0900000001', N'HCM', 'Admin'),
-- Employees (password: emp123)
('emp001', '$2a$11$KFzV6aU0RvtX5VKxC0OEQewLPRwQJQzR5bqB5Y5VyH5vP6pX0HKVS', 'emp1@minimart.com', N'Nguyễn Văn A', '0901111111', N'HCM', 'Employee'),
('emp002', '$2a$11$KFzV6aU0RvtX5VKxC0OEQewLPRwQJQzR5bqB5Y5VyH5vP6pX0HKVS', 'emp2@minimart.com', N'Trần Thị B', '0902222222', N'HCM', 'Employee'),
-- Customers (password: cust123)
('customer01', '$2a$11$8QG5nY7VQjR5X5KxQ0OEQR8wLPRwQJQzR5bqB5Y5VyH5vP6pX0HKVS', 'cust1@gmail.com', N'Lê Văn C', '0911111111', N'Q1, HCM', 'Customer'),
('customer02', '$2a$11$8QG5nY7VQjR5X5KxQ0OEQR8wLPRwQJQzR5bqB5Y5VyH5vP6pX0HKVS', 'cust2@gmail.com', N'Phạm Thị D', '0922222222', N'Q3, HCM', 'Customer'),
('customer03', '$2a$11$8QG5nY7VQjR5X5KxQ0OEQR8wLPRwQJQzR5bqB5Y5VyH5vP6pX0HKVS', 'cust3@gmail.com', N'Hoàng Văn E', '0933333333', N'Q5, HCM', 'Customer');
GO

INSERT INTO Categories (Name, Description, ParentCategoryId) VALUES
(N'Đồ uống', N'Nước giải khát', NULL),
(N'Snack', N'Đồ ăn vặt', NULL),
(N'Bánh kẹo', N'Bánh và kẹo', NULL),
(N'Đồ gia dụng', N'Dụng cụ gia đình', NULL),
(N'Thực phẩm', N'Thực phẩm khô', NULL);
GO

INSERT INTO Products (CategoryId, Name, Description, Price, Stock, Barcode) VALUES
-- Đồ uống
(1, N'Coca Cola 330ml', N'Nước ngọt có ga', 10000, 200, 'COCA330'),
(1, N'Pepsi 330ml', N'Nước ngọt có ga', 10000, 180, 'PEPSI330'),
(1, N'Lavie 500ml', N'Nước khoáng', 5000, 300, 'LAVIE500'),
(1, N'Sting 330ml', N'Nước tăng lực', 12000, 100, 'STING330'),
(1, N'C2 455ml', N'Trà xanh', 8000, 120, 'C2-455'),
-- Snack
(2, N'Ostar 42g', N'Snack khoai tây', 7000, 150, 'OSTAR42'),
(2, N'Poca 52g', N'Snack khoai tây', 8000, 130, 'POCA52'),
(2, N'Lays 56g', N'Snack cao cấp', 15000, 100, 'LAYS56'),
-- Bánh kẹo
(3, N'Oreo 133g', N'Bánh quy kem', 18000, 80, 'OREO133'),
(3, N'Mentos 37g', N'Kẹo nhai', 10000, 150, 'MENTOS37'),
(3, N'Kitkat 17g', N'Socola', 8000, 120, 'KITKAT17'),
-- Đồ gia dụng
(4, N'Giấy vệ sinh', N'10 cuộn', 35000, 60, 'TISSUE10'),
(4, N'Nước rửa chén 750ml', N'Sunlight', 42000, 50, 'SUNLIGHT'),
(4, N'Khăn giấy 150 tờ', N'Kleenex', 28000, 80, 'KLEENEX'),
-- Thực phẩm
(5, N'Hảo Hảo', N'Mỳ gói tôm chua cay', 4500, 200, 'HAOHAO'),
(5, N'Omachi', N'Mỳ xào gói', 6000, 180, 'OMACHI'),
(5, N'Dầu ăn 1L', N'Simply', 48000, 60, 'OIL1L'),
(5, N'Nước mắm 500ml', N'Nam Ngư', 35000, 70, 'NUOCMAM'),
(5, N'Mì chính 400g', N'Ajinomoto', 25000, 80, 'AJI400');
GO

INSERT INTO CartItems (UserId, ProductId, Quantity) VALUES
(4, 1, 2),  -- customer01: 2 Coca
(4, 6, 3),  -- customer01: 3 Ostar
(5, 3, 5);  -- customer02: 5 Lavie
GO

-- POS Orders
INSERT INTO Orders (CustomerId, EmployeeId, OrderDate, TotalAmount, Status, OrderType) VALUES
(4, 2, DATEADD(DAY, -2, GETDATE()), 78000, 'Completed', 'POS'),
(5, 3, DATEADD(DAY, -1, GETDATE()), 125000, 'Completed', 'POS');
-- Online Orders
INSERT INTO Orders (CustomerId, EmployeeId, OrderDate, TotalAmount, Status, OrderType) VALUES
(6, NULL, GETDATE(), 89000, 'Pending', 'Online');
GO

-- Order 1 Details
INSERT INTO OrderDetails (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal) VALUES
(1, 1, N'Coca Cola 330ml', 5, 10000, 50000),
(1, 10, N'Mentos 37g', 2, 10000, 20000),
(1, 15, N'Hảo Hảo', 2, 4500, 9000);
-- Order 2 Details
INSERT INTO OrderDetails (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal) VALUES
(2, 3, N'Lavie 500ml', 10, 5000, 50000),
(2, 8, N'Lays 56g', 3, 15000, 45000),
(2, 9, N'Oreo 133g', 2, 18000, 36000);
-- Order 3 Details
INSERT INTO OrderDetails (OrderId, ProductId, ProductName, Quantity, UnitPrice, Subtotal) VALUES
(3, 4, N'Sting 330ml', 6, 12000, 72000),
(3, 14, N'Khăn giấy 150 tờ', 1, 28000, 28000);
GO