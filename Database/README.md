# Database Scripts

## Cách sử dụng:

1. **Mở SQL Server Management Studio (SSMS)** hoặc dùng công cụ SQL Server khác

2. **Chạy file `CreateDatabase.sql`** để:
   - Tạo database `BilliardsDB`
   - Tạo tất cả 11 bảng
   - Chèn dữ liệu mẫu để test

3. **Kết nối trong app:**
   - Connection string mặc định: `Server=.\\SQLEXPRESS;Database=BilliardsDB;Trusted_Connection=True;TrustServerCertificate=True;`
   - Nếu bạn dùng SQL Server khác, sửa connection string trong `Billiards.UI/appsettings.json`

## Tài khoản test:

- **Username:** `admin` | **Password:** `admin` | **Role:** Admin
- **Username:** `cashier` | **Password:** `cashier` | **Role:** Cashier  
- **Username:** `staff` | **Password:** `staff` | **Role:** Staff

## Dữ liệu mẫu:

- 3 Khu vực (Tầng 1, Tầng 2, Khu VIP)
- 3 Loại bàn (Bàn thường, Bàn VIP, Bàn Pro)
- 7 Bàn (Bàn 01-05, VIP 01-02)
- 4 Loại sản phẩm
- 10 Sản phẩm mẫu
- 3 Nhân viên với tài khoản
- Quy tắc giá theo giờ cho các loại bàn


