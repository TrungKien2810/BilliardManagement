using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class TableManagementService
{
    private readonly TableRepository _tableRepository;
    private readonly AreaRepository _areaRepository;
    private readonly TableTypeRepository _tableTypeRepository;

    public TableManagementService()
    {
        _tableRepository = new TableRepository();
        _areaRepository = new AreaRepository();
        _tableTypeRepository = new TableTypeRepository();
    }

    // Table methods
    public List<Table> GetAllTables()
    {
        return _tableRepository.GetAll();
    }

    public Table? GetTableById(int tableId)
    {
        return _tableRepository.GetById(tableId);
    }

    public void AddTable(Table table)
    {
        _tableRepository.Add(table);
    }

    public void UpdateTable(Table table)
    {
        _tableRepository.Update(table);
    }

    public void DeleteTable(int tableId)
    {
        _tableRepository.Delete(tableId);
    }

    // Area methods
    public List<Area> GetAllAreas()
    {
        return _areaRepository.GetAll();
    }

    public Area? GetAreaById(int areaId)
    {
        return _areaRepository.GetById(areaId);
    }

    public void AddArea(Area area)
    {
        _areaRepository.Add(area);
    }

    public void UpdateArea(Area area)
    {
        _areaRepository.Update(area);
    }

    public void DeleteArea(int areaId)
    {
        _areaRepository.Delete(areaId);
    }

    // TableType methods
    public List<TableType> GetAllTableTypes()
    {
        return _tableTypeRepository.GetAll();
    }

    public TableType? GetTableTypeById(int tableTypeId)
    {
        return _tableTypeRepository.GetById(tableTypeId);
    }

    public void AddTableType(TableType tableType)
    {
        _tableTypeRepository.Add(tableType);
    }

    public void UpdateTableType(TableType tableType)
    {
        _tableTypeRepository.Update(tableType);
    }

    public void DeleteTableType(int tableTypeId)
    {
        _tableTypeRepository.Delete(tableTypeId);
    }
}

