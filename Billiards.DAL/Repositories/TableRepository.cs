using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class TableRepository
{
    private readonly AppDbContext _context;

    public TableRepository()
    {
        _context = new AppDbContext();
    }

    public TableRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Table> GetAllTables()
    {
        // Dùng AsNoTracking để đảm bảo lấy dữ liệu mới nhất từ DB, không dùng cache
        return _context.Tables
            .AsNoTracking()
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .ToList();
    }

    public void UpdateTableStatus(int tableId, string newStatus)
    {
        var table = _context.Tables.FirstOrDefault(t => t.ID == tableId);
        if (table != null)
        {
            table.Status = newStatus;
            _context.SaveChanges();
        }
    }

    public Table? GetById(int tableId)
    {
        return _context.Tables
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .FirstOrDefault(t => t.ID == tableId);
    }
}

