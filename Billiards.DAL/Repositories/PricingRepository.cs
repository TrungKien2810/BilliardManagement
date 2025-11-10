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

    public bool HasOverlappingTimeSlot(int tableTypeId, TimeSpan startTime, TimeSpan endTime, int? excludeRuleId = null)
    {
        // Check for overlapping time slots for the same table type
        // Two time slots overlap if: start1 < end2 && start2 < end1
        var query = _context.HourlyPricingRules
            .Where(r => r.TableTypeID == tableTypeId &&
                       ((r.StartTimeSlot < endTime && startTime < r.EndTimeSlot)));
        
        if (excludeRuleId.HasValue)
        {
            query = query.Where(r => r.ID != excludeRuleId.Value);
        }
        
        return query.Any();
    }

    public void Add(HourlyPricingRule rule)
    {
        if (!rule.TableTypeID.HasValue)
        {
            throw new InvalidOperationException("Loại bàn không được để trống!");
        }
        if (HasOverlappingTimeSlot(rule.TableTypeID.Value, rule.StartTimeSlot, rule.EndTimeSlot))
        {
            throw new InvalidOperationException($"Khung giờ {rule.StartTimeSlot:hh\\:mm} - {rule.EndTimeSlot:hh\\:mm} đã trùng với một quy tắc giá khác cho loại bàn này!");
        }
        _context.HourlyPricingRules.Add(rule);
        _context.SaveChanges();
    }

    public void Update(HourlyPricingRule rule)
    {
        if (!rule.TableTypeID.HasValue)
        {
            throw new InvalidOperationException("Loại bàn không được để trống!");
        }
        if (HasOverlappingTimeSlot(rule.TableTypeID.Value, rule.StartTimeSlot, rule.EndTimeSlot, rule.ID))
        {
            throw new InvalidOperationException($"Khung giờ {rule.StartTimeSlot:hh\\:mm} - {rule.EndTimeSlot:hh\\:mm} đã trùng với một quy tắc giá khác cho loại bàn này!");
        }
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

