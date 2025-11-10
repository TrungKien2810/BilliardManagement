using System;
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

    public bool IsTableTypeNameExists(string typeName, int? excludeTableTypeId = null)
    {
        var query = _context.TableTypes.Where(t => t.TypeName.Trim().ToLower() == typeName.Trim().ToLower());
        if (excludeTableTypeId.HasValue)
        {
            query = query.Where(t => t.ID != excludeTableTypeId.Value);
        }
        return query.Any();
    }

    public void Add(TableType tableType)
    {
        if (IsTableTypeNameExists(tableType.TypeName))
        {
            throw new InvalidOperationException($"Tên loại bàn \"{tableType.TypeName}\" đã tồn tại!");
        }
        _context.TableTypes.Add(tableType);
        _context.SaveChanges();
    }

    public void Update(TableType tableType)
    {
        // Check for duplicate table type name (excluding current table type)
        if (IsTableTypeNameExists(tableType.TypeName, tableType.ID))
        {
            throw new InvalidOperationException($"Tên loại bàn \"{tableType.TypeName}\" đã tồn tại!");
        }
        _context.TableTypes.Update(tableType);
        _context.SaveChanges();
    }

    public List<Table> GetTablesByTableType(int tableTypeId)
    {
        return _context.Tables
            .Where(t => t.TypeID == tableTypeId)
            .ToList();
    }

    public List<HourlyPricingRule> GetPricingRulesByTableType(int tableTypeId)
    {
        return _context.HourlyPricingRules
            .Where(r => r.TableTypeID == tableTypeId)
            .ToList();
    }

    public void Delete(int tableTypeId)
    {
        var tableType = _context.TableTypes.FirstOrDefault(t => t.ID == tableTypeId);
        if (tableType != null)
        {
            // Check if table type is used by tables
            var tables = GetTablesByTableType(tableTypeId);
            if (tables.Any())
            {
                throw new InvalidOperationException($"Không thể xóa loại bàn \"{tableType.TypeName}\" vì còn {tables.Count} bàn đang sử dụng loại bàn này!");
            }

            // Check if table type is used by pricing rules
            var pricingRules = GetPricingRulesByTableType(tableTypeId);
            if (pricingRules.Any())
            {
                throw new InvalidOperationException($"Không thể xóa loại bàn \"{tableType.TypeName}\" vì còn {pricingRules.Count} quy tắc giá đang sử dụng loại bàn này!");
            }

            _context.TableTypes.Remove(tableType);
            _context.SaveChanges();
        }
    }
}

