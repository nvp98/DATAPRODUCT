# HỆ THỐNG QUẢN LÝ DỮ LIỆU SẢN XUẤT (DATA PRODUCT)

## Tổng quan dự án

Hệ thống quản lý dữ liệu sản xuất là một ứng dụng web được phát triển để quản lý toàn bộ quy trình sản xuất tại nhà máy, bao gồm quản lý biên bản giao nhận, nhật ký sản xuất, quản lý gang thép, và các báo cáo thống kê.

## Thông tin dự án

- **Tên dự án**: Data Product System
- **Nền tảng**: ASP.NET Core 6.0 MVC
- **Database**: SQL Server + MySQL (BKMIS)
- **Pattern**: Repository Pattern, Service Layer
- **Authentication**: Cookie Authentication
- **UI Framework**: Bootstrap 5

## Tính năng chính

### 1. Quản lý biên bản giao nhận (BM_11)

- Tạo, sửa, xóa biên bản giao nhận
- Quy trình phê duyệt đa cấp
- Xuất PDF, tạo excel
- Giao nhận giữa các BP/NM

### 2. Quản lý sản xuất gang thép

- **BM_16 - Gang Long**: Quản lý lô sản xuất, chia gang, phân bổ gang CR
- **BM_16 - Gang Thối**: Quản lý gang thối, mẻ thối, lò cao
- **BM_18 - Xi Hạt**: Quản lý phiếu xi hạt lò cao
- Tự động tính toán tỷ lệ chia gang
- Quản lý trạng thái thùng gang/thép

### 3. Nhật ký sản xuất

- Ghi nhận sản xuất theo ca/kíp
- Quản lý thiết bị, cụm thiết bị
- Theo dõi vật tư, nguyên liệu
- Báo cáo thống kê sản xuất

### 4. Quản lý danh mục

- Tài khoản & phân quyền
- Phòng ban, xưởng, vị trí
- Vật tư, mã lô
- Kíp làm việc, xe goong

### 5. Dashboard & Báo cáo

- Dashboard tổng quan
- Thống kê theo xưởng
- Báo cáo định kỳ
- Xuất Excel, PDF

## Công nghệ sử dụng

### Backend

- **Framework**: ASP.NET Core 6.0
- **ORM**: Entity Framework Core 6.0
- **Database**: SQL Server, MySQL
- **API**: RESTful API, YARP Reverse Proxy
- **PDF Generation**: iText7, iTextSharp
- **Excel**: ClosedXML, ExcelDataReader
- **Scheduling**: Quartz.NET

### Frontend

- **Template**: Bootstrap 5
- **JavaScript**: jQuery
- **UI Components**: Chosen.js, DataTables
- **Icons**: Bootstrap Icons, Boxicons, Remixicon

### Security

- Cookie Authentication
- Basic Authentication for API
- Role-based Authorization
- Session Management

## Cấu trúc dự án

```
Data_Product/
├── API/                    # Web API Controllers
├── Controllers/            # MVC Controllers
├── Models/                 # Entity Models
├── DTO/                    # Data Transfer Objects
├── Repositorys/            # DataContext, Repository Pattern
├── Services/               # Business Logic Services
├── Views/                  # Razor Views
├── wwwroot/               # Static Files (css, js, images)
├── Common/                # Utilities (Encryptor, TaoMa)
└── Program.cs             # Application Entry Point
```

## Yêu cầu hệ thống

- .NET 6.0 SDK
- SQL Server 2016 trở lên
- MySQL 5.7+ (cho BKMIS)
- IIS hoặc Kestrel Server
- Modern Web Browser (Chrome, Edge, Firefox)

## Hướng dẫn cài đặt

### 1. Clone source code

```bash
git clone <repository-url>
cd DATAPRODUCT_DEV
```

### 2. Cấu hình database

Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Data Source=<YOUR_SQL_SERVER>;Initial Catalog=PRODUCTDATA;User Id=<USERNAME>;Password=<PASSWORD>;TrustServerCertificate=True",
    "BKMIS": "Server=<YOUR_MYSQL_SERVER>;Database=bkmis_kcshpsdq;User Id=<USERNAME>;Password=<PASSWORD>;Port=<PORT>;"
  }
}
```

### 3. Restore packages

```bash
dotnet restore
```

### 4. Build project

```bash
dotnet build
```

### 5. Run application

```bash
dotnet run
```

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

## Cấu trúc database chính

### Bảng quản lý hệ thống

- `Tbl_TaiKhoan` - Tài khoản người dùng
- `Tbl_Quyen` - Quyền hệ thống
- `Tbl_PhongBan` - Phòng ban
- `Tbl_Xuong` - Xưởng sản xuất
- `Tbl_Kip` - Ca kíp

### Bảng biên bản giao nhận

- `Tbl_BienBanGiaoNhan` - Biên bản giao nhận
- `Tbl_ChiTiet_BienBanGiaoNhan` - Chi tiết biên bản

### Bảng sản xuất gang thép

- `Tbl_BM_16_GangLong` - Gang long
- `Tbl_BM_16_ChiaGang` - Chia gang
- `Tbl_BM_16_LoSanXuat` - Lô sản xuất
- `Tbl_MeThoi` - Mẻ thối
- `Tbl_LoCao` - Lò cao
- `Tbl_BM_18_PhieuXiHat` - Phiếu xi hạt

### Bảng nhật ký sản xuất

- `Tbl_NhatKy_SanXuat` - Nhật ký sản xuất
- `Tbl_NhatKy_SanXuat_ChiTiet` - Chi tiết nhật ký
- `Tbl_VatTu` - Vật tư
- `Tbl_MaLo` - Mã lô

## API Endpoints

### Authentication

- `POST /DangNhap/Index` - Đăng nhập

### Biên bản giao nhận

- `GET /api/ProductApi/GetBBGN` - Lấy danh sách biên bản
- Basic Auth: Cấu hình trong appsettings.json

### Reverse Proxy

- `/sanxuat/*` → Proxy tới external reporting service

## Quy trình phát triển

1. Tạo branch mới từ `main`/`master`
2. Phát triển tính năng trên branch
3. Test kỹ trên môi trường dev
4. Tạo Pull Request
5. Review code
6. Merge vào branch chính
7. Deploy lên môi trường production

## Tài liệu kỹ thuật

Chi tiết tài liệu kỹ thuật nằm trong thư mục `docs/`:

- [01-business-flow.md](docs/01-business-flow.md) - Quy trình nghiệp vụ
- [02-architecture.md](docs/02-architecture.md) - Kiến trúc hệ thống
- [03-backend.md](docs/03-backend.md) - Backend Implementation
- [04-frontend.md](docs/04-frontend.md) - Frontend Implementation
- [05-database.md](docs/05-database.md) - Database Design
- [06-deployment.md](docs/06-deployment.md) - Deployment Guide
- [07-operation.md](docs/07-operation.md) - Operation & Maintenance

## Liên hệ & Hỗ trợ

- **Team**: Phòng CNTT - Hòa Phát Dung Quất
- **Email**: pcntt@hoaphat.com.vn

## License

Copyright © 2024 Hòa Phát Dung Quất. All rights reserved.
