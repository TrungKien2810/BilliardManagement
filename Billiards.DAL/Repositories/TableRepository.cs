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
            .AsNoTracking() // Disable tracking to ensure fresh data
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .ToList();
    }

    public List<Table> GetTablesByArea(int areaId)
    {
        return _context.Tables
            .AsNoTracking() // Disable tracking to ensure fresh data
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .Where(t => t.AreaID == areaId)
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

    public List<Table> GetAll()
    {
        return _context.Tables
            .AsNoTracking() // Disable tracking to ensure fresh data
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .OrderBy(t => t.TableName)
            .ToList();
    }

    public Table? GetById(int tableId)
    {
        // Ensure fresh read from DB
        _context.ChangeTracker.Clear();

        return _context.Tables
            .AsNoTracking()
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .FirstOrDefault(t => t.ID == tableId);
    }

    public void Add(Table table)
    {
        _context.Tables.Add(table);
        _context.SaveChanges();
    }

    public void Update(Table table)
    {
        // Find the existing table in the current context
        var existingTable = _context.Tables.FirstOrDefault(t => t.ID == table.ID);
        if (existingTable != null)
        {
            // Update properties
            existingTable.TableName = table.TableName;
            existingTable.AreaID = table.AreaID;
            existingTable.TypeID = table.TypeID;
            existingTable.Status = table.Status;
            _context.SaveChanges();
        }
        else
        {
            // If not found, attach and update
            _context.Tables.Update(table);
            _context.SaveChanges();
        }
    }

    public void Delete(int tableId)
    {
        var table = _context.Tables.FirstOrDefault(t => t.ID == tableId);
        if (table != null)
        {
            _context.Tables.Remove(table);
            _context.SaveChanges();
        }
    }
}

