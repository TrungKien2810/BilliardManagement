namespace Billiards.DAL.Models;

/// <summary>
/// Quy tắc tích điểm và đổi điểm cho hệ thống khách hàng thân thiết
/// </summary>
public class LoyaltyRule
{
    public int ID { get; set; }
    
    /// <summary>
    /// Quy tắc tích điểm: Số VND cần để có 1 điểm
    /// Ví dụ: 10000 = 10.000đ = 1 điểm
    /// </summary>
    public decimal PointsPerAmount { get; set; } = 10000; // Mặc định: 10.000đ = 1 điểm
    
    /// <summary>
    /// Quy tắc đổi điểm: 1 điểm = bao nhiêu VND khi giảm giá
    /// Ví dụ: 100 = 1 điểm = 100đ giảm giá
    /// </summary>
    public decimal AmountPerPoint { get; set; } = 100; // Mặc định: 1 điểm = 100đ
    
    /// <summary>
    /// Số điểm tối thiểu để được phép đổi điểm
    /// Ví dụ: 1000 = cần ít nhất 1000 điểm mới được đổi
    /// </summary>
    public int MinimumPointsToRedeem { get; set; } = 1000; // Mặc định: 1000 điểm
    
    /// <summary>
    /// Có kích hoạt hệ thống tích điểm không
    /// </summary>
    public bool IsActive { get; set; } = true;
}

