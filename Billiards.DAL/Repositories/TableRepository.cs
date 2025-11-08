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
        return _context.Tables
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

    public List<Table> GetAll()
    {
        return _context.Tables
            .Include(t => t.Area)
            .Include(t => t.TableType)
            .OrderBy(t => t.TableName)
            .ToList();
    }

    public Table? GetById(int tableId)
    {
        return _context.Tables
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
        _context.Tables.Update(table);
        _context.SaveChanges();
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

