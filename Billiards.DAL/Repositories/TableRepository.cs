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

    public bool IsTableNameExists(string tableName, int? excludeTableId = null)
    {
        var query = _context.Tables.Where(t => t.TableName.Trim().ToLower() == tableName.Trim().ToLower());
        if (excludeTableId.HasValue)
        {
            query = query.Where(t => t.ID != excludeTableId.Value);
        }
        return query.Any();
    }

    public void Add(Table table)
    {
        if (IsTableNameExists(table.TableName))
        {
            throw new InvalidOperationException($"Tên bàn \"{table.TableName}\" đã tồn tại!");
        }
        _context.Tables.Add(table);
        _context.SaveChanges();
    }

    public void Update(Table table)
    {
        // Check for duplicate table name (excluding current table)
        if (IsTableNameExists(table.TableName, table.ID))
        {
            throw new InvalidOperationException($"Tên bàn \"{table.TableName}\" đã tồn tại!");
        }

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

    public bool HasActiveInvoice(int tableId)
    {
        // Clear change tracker to ensure fresh data
        _context.ChangeTracker.Clear();
        
        return _context.Invoices
            .AsNoTracking()
            .Any(i => i.TableID == tableId && i.Status == "Active");
    }

    public void Delete(int tableId)
    {
        // Clear change tracker to ensure fresh data
        _context.ChangeTracker.Clear();
        
        var table = _context.Tables
            .AsNoTracking()
            .FirstOrDefault(t => t.ID == tableId);
        
        if (table != null)
        {
            // Only prevent deletion if table status is InUse
            // If table is Free, allow deletion even if there might be orphaned Active invoices
            // (This handles cases where checkout didn't properly close invoices)
            if (table.Status == "InUse")
            {
                throw new InvalidOperationException($"Không thể xóa bàn \"{table.TableName}\" vì bàn đang được sử dụng (Status: InUse)! Vui lòng đảm bảo bàn đã được giải phóng trước khi xóa.");
            }

            // If table status is not InUse, proceed with deletion
            // Note: Any orphaned Active invoices will have their TableID set to NULL by FK constraint
            _context.ChangeTracker.Clear();
            var tableToDelete = _context.Tables.FirstOrDefault(t => t.ID == tableId);
            if (tableToDelete != null)
            {
                _context.Tables.Remove(tableToDelete);
                _context.SaveChanges();
            }
        }
    }
}

