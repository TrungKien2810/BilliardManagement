-- Migration Script: Remove Staff Role
-- Chuyển tất cả account có role Staff sang Cashier
-- Hoặc xóa account Staff nếu không cần thiết

-- Cách 1: Chuyển tất cả Staff account sang Cashier
UPDATE Accounts
SET Role = 'Cashier'
WHERE Role = 'Staff';
GO

-- Cách 2: Xóa tất cả Staff account (uncomment nếu muốn xóa thay vì chuyển)
-- DELETE FROM Accounts WHERE Role = 'Staff';
-- GO

PRINT 'Đã chuyển tất cả Staff account sang Cashier';
GO

