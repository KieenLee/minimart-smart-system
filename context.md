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
Blazor App <-> ASP.NET Web API <-> Database (EF Core)
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
| Khách hàng | Web (Blazor)  | Xem sản phẩm, đặt hàng trực tuyến, xem lịch sử đơn hàng |
| Nhân viên  | Desktop (TCP) | Đăng nhập, bán hàng POS, in hóa đơn, kiểm tra tồn kho   |
| Admin      | Desktop (TCP) | Quản lý nhân viên, chỉnh sửa giá, xem báo cáo doanh thu |

---

## 5. Danh mục công nghệ (Tech Stack)

| Thành phần      | Công nghệ                      |
| --------------- | ------------------------------ |
| Web UI          | Blazor Page (ASP.NET Core)     |
| Web Backend     | ASP.NET Core Web API (RESTful) |
| Desktop Client  | WPF (.NET 8)                   |
| Internal Server | Console App (TCP Listener)     |
| Communication   | System.Net.Sockets (TCP/IP)    |
| Data Access     | Entity Framework Core          |
| Database        | SQL Server                     |
| Security        | JWT Bearer Authentication      |

---

## 6. Kỹ thuật & Kiến thức ôn tập

- Entity Framework Core
- LINQ
- Async / Await
- Dependency Injection (Console App)
- Dependency Container
- Interface
- Kiến trúc 3 Layers
