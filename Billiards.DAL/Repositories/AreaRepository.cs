using Billiards.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Billiards.DAL.Repositories;

public class AreaRepository
{
    private readonly AppDbContext _context;

    public AreaRepository()
    {
        _context = new AppDbContext();
    }

    public AreaRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Area> GetAllAreas()
    {
        return _context.Areas.OrderBy(a => a.ID).ToList();
    }
}

