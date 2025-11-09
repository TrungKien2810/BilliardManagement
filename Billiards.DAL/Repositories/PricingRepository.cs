using Billiards.DAL.Models;
using Microsoft.EntityFrameworkCore;

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

    public List<HourlyPricingRule> GetRules(int tableTypeId)
    {
        return _context.HourlyPricingRules
            .Where(r => r.TableTypeID == tableTypeId)
            .OrderBy(r => r.StartTimeSlot)
            .ToList();
    }
}

