using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class TableTypeRepository
{
    private readonly AppDbContext _context;

    public TableTypeRepository()
    {
        _context = new AppDbContext();
    }

    public TableTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<TableType> GetAll()
    {
        return _context.TableTypes.OrderBy(t => t.TypeName).ToList();
    }

    public TableType? GetById(int tableTypeId)
    {
        return _context.TableTypes.FirstOrDefault(t => t.ID == tableTypeId);
    }

    public void Add(TableType tableType)
    {
        _context.TableTypes.Add(tableType);
        _context.SaveChanges();
    }

    public void Update(TableType tableType)
    {
        _context.TableTypes.Update(tableType);
        _context.SaveChanges();
    }

    public void Delete(int tableTypeId)
    {
        var tableType = _context.TableTypes.FirstOrDefault(t => t.ID == tableTypeId);
        if (tableType != null)
        {
            _context.TableTypes.Remove(tableType);
            _context.SaveChanges();
        }
    }
}

