using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class LoyaltyRuleRepository
{
    private readonly AppDbContext _context;

    public LoyaltyRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public LoyaltyRule? GetActiveRule()
    {
        return _context.LoyaltyRules
            .FirstOrDefault(r => r.IsActive);
    }

    public List<LoyaltyRule> GetAll()
    {
        return _context.LoyaltyRules
            .OrderBy(r => r.ID)
            .ToList();
    }

    public LoyaltyRule? GetById(int id)
    {
        return _context.LoyaltyRules
            .FirstOrDefault(r => r.ID == id);
    }

    public void Add(LoyaltyRule rule)
    {
        _context.LoyaltyRules.Add(rule);
        _context.SaveChanges();
    }

    public void Update(LoyaltyRule rule)
    {
        _context.LoyaltyRules.Update(rule);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var rule = _context.LoyaltyRules.FirstOrDefault(r => r.ID == id);
        if (rule != null)
        {
            _context.LoyaltyRules.Remove(rule);
            _context.SaveChanges();
        }
    }

    /// <summary>
    /// Vô hiệu hóa tất cả các rule, chỉ kích hoạt rule có ID được chỉ định
    /// </summary>
    public void SetActiveRule(int ruleId)
    {
        // Vô hiệu hóa tất cả
        var allRules = _context.LoyaltyRules.ToList();
        foreach (var rule in allRules)
        {
            rule.IsActive = (rule.ID == ruleId);
        }
        _context.SaveChanges();
    }
}

