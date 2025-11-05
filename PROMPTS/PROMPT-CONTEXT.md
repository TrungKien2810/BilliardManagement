### PROJECT CONTEXT (Bối cảnh dự án)

Chúng ta đang xây dựng ứng dụng Quản lý Quán Bida bằng C#, WPF, và SQL Server.

**1. Công nghệ (Tech Stack):**

* **Ngôn ngữ:** C#
* **Giao diện (UI):** WPF (.NET 8)
* **Cơ sở dữ liệu (DB):** SQL Server Express Edition
* **Truy cập dữ liệu (ORM):** Entity Framework Core 8 (EF Core)

**2. Kiến trúc (Architecture):**

BẮT BUỘC tuân theo kiến trúc 3-lớp (3-Tier) trong cùng một Solution:

* `Billiards.UI` (Project WPF chính - Chứa Windows, UserControls, XAML)
* `Billiards.BLL` (Project Class Library - Chứa Services, Business Logic)
* `Billiards.DAL` (Project Class Library - Chứa Models, DbContext)

**Nguyên tắc:**

* `UI` chỉ được gọi `BLL`.
* `BLL` chỉ được gọi `DAL`.
* KHÔNG BAO GIỜ gọi `DAL` từ `UI`.

**3. Lược đồ Database (Database Schema):**

Đây là cấu trúc CSDL đầy đủ. Hãy sử dụng đúng tên bảng và tên cột này khi tạo Model và DbContext.

```sql
CREATE TABLE Areas (
    ID INT PRIMARY KEY IDENTITY(1,1),
    AreaName NVARCHAR(100) NOT NULL
);

CREATE TABLE TableTypes (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TypeName NVARCHAR(100) NOT NULL
);

CREATE TABLE Tables (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    AreaID INT FOREIGN KEY REFERENCES Areas(ID),
    TypeID INT FOREIGN KEY REFERENCES TableTypes(ID),
    Status NVARCHAR(50) NOT NULL -- (e.g., Free, InUse, Reserved, Maintenance)
);

CREATE TABLE ProductCategories (
    ID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);

CREATE TABLE Products (
    ID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(200) NOT NULL,
    CategoryID INT FOREIGN KEY REFERENCES ProductCategories(ID),
    SalePrice DECIMAL(18, 2) NOT NULL,
    StockQuantity INT NOT NULL
);

CREATE TABLE Employees (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber VARCHAR(20),
    Address NVARCHAR(500)
);

CREATE TABLE Accounts (
    Username VARCHAR(100) PRIMARY KEY,
    Password NVARCHAR(MAX) NOT NULL, -- Sẽ được hash
    EmployeeID INT FOREIGN KEY REFERENCES Employees(ID),
    Role NVARCHAR(50) NOT NULL -- (e.g., Admin, Cashier, Staff)
);

CREATE TABLE Customers (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200),
    PhoneNumber VARCHAR(20) UNIQUE,
    LoyaltyPoints INT DEFAULT 0
);

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
    Status NVARCHAR(50) NOT NULL -- (e.g., Active, Paid, Cancelled)
);

CREATE TABLE InvoiceDetails (
    InvoiceID INT FOREIGN KEY REFERENCES Invoices(ID),
    ProductID INT FOREIGN KEY REFERENCES Products(ID),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    PRIMARY KEY (InvoiceID, ProductID)
);

CREATE TABLE HourlyPricingRules (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableTypeID INT FOREIGN KEY REFERENCES TableTypes(ID),
    StartTimeSlot TIME NOT NULL,
    EndTimeSlot TIME NOT NULL,
    PricePerHour DECIMAL(18, 2) NOT NULL
);
```

