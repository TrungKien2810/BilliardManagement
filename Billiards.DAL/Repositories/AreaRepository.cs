using System;
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

    public List<Area> GetAll()
    {
        return _context.Areas.OrderBy(a => a.AreaName).ToList();
    }

    public Area? GetById(int areaId)
    {
        return _context.Areas.FirstOrDefault(a => a.ID == areaId);
    }

    public bool IsAreaNameExists(string areaName, int? excludeAreaId = null)
    {
        var query = _context.Areas.Where(a => a.AreaName.Trim().ToLower() == areaName.Trim().ToLower());
        if (excludeAreaId.HasValue)
        {
            query = query.Where(a => a.ID != excludeAreaId.Value);
        }
        return query.Any();
    }

    public void Add(Area area)
    {
        if (IsAreaNameExists(area.AreaName))
        {
            throw new InvalidOperationException($"Tên khu vực \"{area.AreaName}\" đã tồn tại!");
        }
        _context.Areas.Add(area);
        _context.SaveChanges();
    }

    public void Update(Area area)
    {
        // Check for duplicate area name (excluding current area)
        if (IsAreaNameExists(area.AreaName, area.ID))
        {
            throw new InvalidOperationException($"Tên khu vực \"{area.AreaName}\" đã tồn tại!");
        }
        _context.Areas.Update(area);
        _context.SaveChanges();
    }

    public List<Table> GetTablesByArea(int areaId)
    {
        // Clear change tracker to ensure fresh data
        _context.ChangeTracker.Clear();
        
        return _context.Tables
            .AsNoTracking()
            .Where(t => t.AreaID == areaId)
            .ToList();
    }

    public List<Table> GetInUseTablesByArea(int areaId)
    {
        // Clear change tracker to ensure fresh data
        _context.ChangeTracker.Clear();
        
        return _context.Tables
            .AsNoTracking()
            .Where(t => t.AreaID == areaId && t.Status == "InUse")
            .ToList();
    }

    public void Delete(int areaId)
    {
        var area = _context.Areas.FirstOrDefault(a => a.ID == areaId);
        if (area != null)
        {
            _context.Areas.Remove(area);
            _context.SaveChanges();
        }
    }
}

