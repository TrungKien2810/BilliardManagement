using Billiards.DAL;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class LoyaltyService
{
    private readonly LoyaltyRuleRepository _loyaltyRuleRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly AppDbContext _context;

    public LoyaltyService()
    {
        _context = new AppDbContext();
        _loyaltyRuleRepository = new LoyaltyRuleRepository(_context);
        _customerRepository = new CustomerRepository(_context);
    }

    /// <summary>
    /// Lấy quy tắc tích điểm đang kích hoạt
    /// </summary>
    public LoyaltyRule? GetActiveRule()
    {
        return _loyaltyRuleRepository.GetActiveRule();
    }

    /// <summary>
    /// Lấy tất cả quy tắc
    /// </summary>
    public List<LoyaltyRule> GetAllRules()
    {
        return _loyaltyRuleRepository.GetAll();
    }

    /// <summary>
    /// Lấy quy tắc theo ID
    /// </summary>
    public LoyaltyRule? GetRuleById(int id)
    {
        return _loyaltyRuleRepository.GetById(id);
    }

    /// <summary>
    /// Thêm quy tắc mới
    /// </summary>
    public void AddRule(LoyaltyRule rule)
    {
        _loyaltyRuleRepository.Add(rule);
    }

    /// <summary>
    /// Cập nhật quy tắc
    /// </summary>
    public void UpdateRule(LoyaltyRule rule)
    {
        _loyaltyRuleRepository.Update(rule);
    }

    /// <summary>
    /// Xóa quy tắc
    /// </summary>
    public void DeleteRule(int id)
    {
        _loyaltyRuleRepository.Delete(id);
    }

    /// <summary>
    /// Đặt quy tắc làm quy tắc đang kích hoạt
    /// </summary>
    public void SetActiveRule(int ruleId)
    {
        _loyaltyRuleRepository.SetActiveRule(ruleId);
    }

    /// <summary>
    /// Tính số điểm sẽ được tích dựa trên tổng tiền thanh toán
    /// </summary>
    public int CalculatePointsEarned(decimal totalAmount)
    {
        var rule = GetActiveRule();
        if (rule == null || !rule.IsActive)
        {
            return 0;
        }

        // Tính điểm: totalAmount / PointsPerAmount
        // Ví dụ: 500.000đ / 10.000đ = 50 điểm
        return (int)Math.Floor(totalAmount / rule.PointsPerAmount);
    }

    /// <summary>
    /// Tính số tiền giảm giá khi đổi điểm
    /// </summary>
    public decimal CalculateDiscountFromPoints(int points)
    {
        var rule = GetActiveRule();
        if (rule == null || !rule.IsActive)
        {
            return 0;
        }

        // Tính giảm giá: points * AmountPerPoint
        // Ví dụ: 1500 điểm * 100đ = 150.000đ
        return points * rule.AmountPerPoint;
    }

    /// <summary>
    /// Kiểm tra xem khách hàng có đủ điểm để đổi không
    /// </summary>
    public bool CanRedeemPoints(int customerPoints)
    {
        var rule = GetActiveRule();
        if (rule == null || !rule.IsActive)
        {
            return false;
        }

        return customerPoints >= rule.MinimumPointsToRedeem;
    }

    /// <summary>
    /// Lấy số điểm tối thiểu để được phép đổi
    /// </summary>
    public int GetMinimumPointsToRedeem()
    {
        var rule = GetActiveRule();
        if (rule == null || !rule.IsActive)
        {
            return 0;
        }

        return rule.MinimumPointsToRedeem;
    }

    /// <summary>
    /// Tích điểm cho khách hàng sau khi thanh toán
    /// </summary>
    public void EarnPoints(int customerId, int points)
    {
        if (points <= 0) return;

        var customer = _customerRepository.GetById(customerId);
        if (customer == null)
        {
            throw new Exception("Không tìm thấy khách hàng!");
        }

        customer.LoyaltyPoints += points;
        _customerRepository.Update(customer);
    }

    /// <summary>
    /// Trừ điểm khi khách hàng đổi điểm
    /// </summary>
    public void RedeemPoints(int customerId, int points)
    {
        if (points <= 0) return;

        var customer = _customerRepository.GetById(customerId);
        if (customer == null)
        {
            throw new Exception("Không tìm thấy khách hàng!");
        }

        if (customer.LoyaltyPoints < points)
        {
            throw new Exception("Khách hàng không đủ điểm để đổi!");
        }

        customer.LoyaltyPoints -= points;
        _customerRepository.Update(customer);
    }
}

