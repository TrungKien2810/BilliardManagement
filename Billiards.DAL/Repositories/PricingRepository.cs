using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class PricingRepository
{
    private readonly AppDbContext _context;

    public PricingRepository()
    {
        _context = new AppDbContext();
    }

    public PricingRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<HourlyPricingRule> GetAll()
    {
        return _context.HourlyPricingRules
            .Include(p => p.TableType)
            .OrderBy(p => p.TableTypeID)
            .ThenBy(p => p.StartTimeSlot)
            .ToList();
    }

    public List<HourlyPricingRule> GetRules(int tableTypeId)
    {
        return _context.HourlyPricingRules
            .Include(p => p.TableType)
            .Where(p => p.TableTypeID == tableTypeId)
            .OrderBy(p => p.StartTimeSlot)
            .ToList();
    }

    public HourlyPricingRule? GetById(int pricingRuleId)
    {
        return _context.HourlyPricingRules
            .Include(p => p.TableType)
            .FirstOrDefault(p => p.ID == pricingRuleId);
    }

    public void Add(HourlyPricingRule rule)
    {
        _context.HourlyPricingRules.Add(rule);
        _context.SaveChanges();
    }

    public void Update(HourlyPricingRule rule)
    {
        _context.HourlyPricingRules.Update(rule);
        _context.SaveChanges();
    }

    public void Delete(int pricingRuleId)
    {
        var rule = _context.HourlyPricingRules.FirstOrDefault(p => p.ID == pricingRuleId);
        if (rule != null)
        {
            _context.HourlyPricingRules.Remove(rule);
            _context.SaveChanges();
        }
    }
}

