-- Script để thêm bảng LoyaltyRules vào database nếu chưa tồn tại
USE BilliardsDB;
GO

-- Kiểm tra và tạo bảng LoyaltyRules
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoyaltyRules')
BEGIN
    CREATE TABLE LoyaltyRules (
        ID INT PRIMARY KEY IDENTITY(1,1),
        PointsPerAmount DECIMAL(18, 2) NOT NULL DEFAULT 10000, -- Số VND cần để có 1 điểm (mặc định: 10.000đ = 1 điểm)
        AmountPerPoint DECIMAL(18, 2) NOT NULL DEFAULT 100, -- 1 điểm = bao nhiêu VND khi đổi (mặc định: 1 điểm = 100đ)
        MinimumPointsToRedeem INT NOT NULL DEFAULT 1000, -- Số điểm tối thiểu để được đổi (mặc định: 1000 điểm)
        IsActive BIT NOT NULL DEFAULT 1 -- Có kích hoạt hệ thống tích điểm không
    );
    PRINT 'Bảng LoyaltyRules đã được tạo.';
END
ELSE
BEGIN
    PRINT 'Bảng LoyaltyRules đã tồn tại.';
END
GO

-- Kiểm tra và chèn dữ liệu mẫu nếu chưa có
IF NOT EXISTS (SELECT * FROM LoyaltyRules)
BEGIN
    INSERT INTO LoyaltyRules (PointsPerAmount, AmountPerPoint, MinimumPointsToRedeem, IsActive) VALUES
    (10000, 100, 1000, 1);
    PRINT 'Đã chèn dữ liệu mẫu vào bảng LoyaltyRules.';
END
ELSE
BEGIN
    PRINT 'Bảng LoyaltyRules đã có dữ liệu.';
END
GO

