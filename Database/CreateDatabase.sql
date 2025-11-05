-- =============================================
-- Script tạo Database và các bảng cho hệ thống Quản lý Quán Bida
-- Database: BilliardsDB
-- =============================================

-- Tạo Database (nếu chưa tồn tại)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BilliardsDB')
BEGIN
    CREATE DATABASE BilliardsDB;
END
GO

USE BilliardsDB;
GO

-- =============================================
-- XÓA CÁC BẢNG NẾU TỒN TẠI (Để tạo mới)
-- =============================================
IF OBJECT_ID('InvoiceDetails', 'U') IS NOT NULL DROP TABLE InvoiceDetails;
IF OBJECT_ID('HourlyPricingRules', 'U') IS NOT NULL DROP TABLE HourlyPricingRules;
IF OBJECT_ID('Invoices', 'U') IS NOT NULL DROP TABLE Invoices;
IF OBJECT_ID('Accounts', 'U') IS NOT NULL DROP TABLE Accounts;
IF OBJECT_ID('Employees', 'U') IS NOT NULL DROP TABLE Employees;
IF OBJECT_ID('Products', 'U') IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('ProductCategories', 'U') IS NOT NULL DROP TABLE ProductCategories;
IF OBJECT_ID('Customers', 'U') IS NOT NULL DROP TABLE Customers;
IF OBJECT_ID('Tables', 'U') IS NOT NULL DROP TABLE Tables;
IF OBJECT_ID('TableTypes', 'U') IS NOT NULL DROP TABLE TableTypes;
IF OBJECT_ID('Areas', 'U') IS NOT NULL DROP TABLE Areas;
GO

-- =============================================
-- TẠO CÁC BẢNG
-- =============================================

-- Bảng Areas (Khu vực)
CREATE TABLE Areas (
    ID INT PRIMARY KEY IDENTITY(1,1),
    AreaName NVARCHAR(100) NOT NULL
);
GO

-- Bảng TableTypes (Loại bàn)
CREATE TABLE TableTypes (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL
);
GO

-- Bảng Tables (Bàn)
CREATE TABLE Tables (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    AreaID INT FOREIGN KEY REFERENCES Areas(ID),
    TypeID INT FOREIGN KEY REFERENCES TableTypes(ID),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Free' -- (Free, InUse, Reserved, Maintenance)
);
GO

-- Bảng ProductCategories (Loại sản phẩm)
CREATE TABLE ProductCategories (
    ID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- Bảng Products (Sản phẩm/Dịch vụ)
CREATE TABLE Products (
    ID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(200) NOT NULL,
    CategoryID INT FOREIGN KEY REFERENCES ProductCategories(ID),
    SalePrice DECIMAL(18, 2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0
);
GO

-- Bảng Employees (Nhân viên)
CREATE TABLE Employees (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(20),
    Address NVARCHAR(500)
);
GO

-- Bảng Accounts (Tài khoản)
CREATE TABLE Accounts (
    Username VARCHAR(100) PRIMARY KEY,
    Password NVARCHAR(MAX) NOT NULL,
    EmployeeID INT FOREIGN KEY REFERENCES Employees(ID),
    Role NVARCHAR(50) NOT NULL -- (Admin, Cashier, Staff)
);
GO

-- Bảng Customers (Khách hàng)
CREATE TABLE Customers (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200),
    PhoneNumber VARCHAR(20) UNIQUE,
    LoyaltyPoints INT DEFAULT 0
);
GO

-- Bảng Invoices (Hóa đơn)
CREATE TABLE Invoices (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableID INT FOREIGN KEY REFERENCES Tables(ID),
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    CreatedByEmployeeID INT FOREIGN KEY REFERENCES Employees(ID),
    CustomerID INT FOREIGN KEY REFERENCES Customers(ID),
    TableFee DECIMAL(18, 2) DEFAULT 0,
    ProductFee DECIMAL(18, 2) DEFAULT 0,
    Discount DECIMAL(18, 2) DEFAULT 0,
    TotalAmount DECIMAL(18, 2) DEFAULT 0,
    Status NVARCHAR(50) NOT NULL -- (Active, Paid, Cancelled)
);
GO

-- Bảng InvoiceDetails (Chi tiết hóa đơn)
CREATE TABLE InvoiceDetails (
    InvoiceID INT FOREIGN KEY REFERENCES Invoices(ID),
    ProductID INT FOREIGN KEY REFERENCES Products(ID),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    PRIMARY KEY (InvoiceID, ProductID)
);
GO

-- Bảng HourlyPricingRules (Quy tắc giá theo giờ)
CREATE TABLE HourlyPricingRules (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableTypeID INT FOREIGN KEY REFERENCES TableTypes(ID),
    StartTimeSlot TIME NOT NULL,
    EndTimeSlot TIME NOT NULL,
    PricePerHour DECIMAL(18, 2) NOT NULL
);
GO

-- =============================================
-- CHÈN DỮ LIỆU MẪU ĐỂ TEST
-- =============================================

-- Chèn Areas (Khu vực)
INSERT INTO Areas (AreaName) VALUES
('Tầng 1'),
('Tầng 2'),
('Khu VIP');
GO

-- Chèn TableTypes (Loại bàn)
INSERT INTO TableTypes (TypeName) VALUES
('Bàn thường'),
('Bàn VIP'),
('Bàn Pro');
GO

-- Chèn Tables (Bàn)
INSERT INTO Tables (TableName, AreaID, TypeID, Status) VALUES
('Bàn 01', 1, 1, 'Free'),
('Bàn 02', 1, 1, 'Free'),
('Bàn 03', 1, 1, 'Free'),
('Bàn 04', 2, 1, 'Free'),
('Bàn 05', 2, 2, 'Free'),
('VIP 01', 3, 3, 'Free'),
('VIP 02', 3, 3, 'Free');
GO

-- Chèn ProductCategories (Loại sản phẩm)
INSERT INTO ProductCategories (CategoryName) VALUES
('Đồ uống'),
('Đồ ăn'),
('Thuốc lá'),
('Khác');
GO

-- Chèn Products (Sản phẩm)
INSERT INTO Products (ProductName, CategoryID, SalePrice, StockQuantity) VALUES
('Coca Cola', 1, 15000, 100),
('Pepsi', 1, 15000, 100),
('Nước suối', 1, 10000, 200),
('Bia Heineken', 1, 35000, 50),
('Bia Tiger', 1, 30000, 50),
('Mì tôm', 2, 25000, 50),
('Bánh mì', 2, 15000, 30),
('Thuốc lá Vina', 3, 20000, 50),
('Thuốc lá Jet', 3, 18000, 50),
('Khăn lạnh', 4, 5000, 200);
GO

-- Chèn Employees (Nhân viên)
INSERT INTO Employees (FullName, PhoneNumber, Address) VALUES
('Nguyễn Văn Admin', '0123456789', '123 Đường ABC'),
('Trần Thị Thu Ngân', '0987654321', '456 Đường XYZ'),
('Lê Văn Nhân Viên', '0912345678', '789 Đường DEF');
GO

-- Chèn Accounts (Tài khoản)
-- Mật khẩu: admin (sẽ được hash sau)
INSERT INTO Accounts (Username, Password, EmployeeID, Role) VALUES
('admin', 'admin', 1, 'Admin'),
('cashier', 'cashier', 2, 'Cashier'),
('staff', 'staff', 3, 'Staff');
GO

-- Chèn Customers (Khách hàng - có thể để trống, sẽ thêm khi cần)
INSERT INTO Customers (FullName, PhoneNumber, LoyaltyPoints) VALUES
('Khách vãng lai', NULL, 0);
GO

-- Chèn HourlyPricingRules (Quy tắc giá theo giờ)
-- Bàn thường: 8h-22h: 50000/h, 22h-24h: 70000/h
INSERT INTO HourlyPricingRules (TableTypeID, StartTimeSlot, EndTimeSlot, PricePerHour) VALUES
(1, '08:00:00', '22:00:00', 50000),
(1, '22:00:00', '24:00:00', 70000),
-- Bàn VIP: 8h-22h: 80000/h, 22h-24h: 100000/h
(2, '08:00:00', '22:00:00', 80000),
(2, '22:00:00', '24:00:00', 100000),
-- Bàn Pro: 8h-22h: 100000/h, 22h-24h: 120000/h
(3, '08:00:00', '22:00:00', 100000),
(3, '22:00:00', '24:00:00', 120000);
GO

-- =============================================
-- KIỂM TRA DỮ LIỆU
-- =============================================
SELECT 'Areas' AS TableName, COUNT(*) AS RecordCount FROM Areas
UNION ALL
SELECT 'TableTypes', COUNT(*) FROM TableTypes
UNION ALL
SELECT 'Tables', COUNT(*) FROM Tables
UNION ALL
SELECT 'ProductCategories', COUNT(*) FROM ProductCategories
UNION ALL
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Employees', COUNT(*) FROM Employees
UNION ALL
SELECT 'Accounts', COUNT(*) FROM Accounts
UNION ALL
SELECT 'Customers', COUNT(*) FROM Customers
UNION ALL
SELECT 'HourlyPricingRules', COUNT(*) FROM HourlyPricingRules;
GO

PRINT '=============================================';
PRINT 'Database BilliardsDB đã được tạo thành công!';
PRINT 'Tài khoản test:';
PRINT '  - admin / admin (Admin)';
PRINT '  - cashier / cashier (Cashier)';
PRINT '  - staff / staff (Staff)';
PRINT '=============================================';
GO


