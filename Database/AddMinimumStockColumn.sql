-- =============================================
-- Script thêm cột MinimumStock vào bảng Products
-- Chạy script này nếu database đã tồn tại
-- =============================================

USE BilliardsDB;
GO

-- Kiểm tra và thêm cột MinimumStock nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Products') AND name = 'MinimumStock')
BEGIN
    ALTER TABLE Products
    ADD MinimumStock INT NOT NULL DEFAULT 10;
    
    PRINT 'Đã thêm cột MinimumStock vào bảng Products với giá trị mặc định là 10';
END
ELSE
BEGIN
    PRINT 'Cột MinimumStock đã tồn tại trong bảng Products';
END
GO

-- Cập nhật giá trị MinimumStock cho các sản phẩm hiện có (nếu chưa có giá trị)
UPDATE Products
SET MinimumStock = 10
WHERE MinimumStock IS NULL OR MinimumStock = 0;
GO

PRINT 'Đã cập nhật giá trị MinimumStock cho tất cả sản phẩm';
GO

