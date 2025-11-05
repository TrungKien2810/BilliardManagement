using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class TableService
{
    private readonly TableRepository _tableRepository;

    public TableService()
    {
        _tableRepository = new TableRepository();
    }

    public TableService(TableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    public List<Table> GetTableMap()
    {
        return _tableRepository.GetAllTables();
    }

    public void UpdateTableStatus(int tableId, string newStatus)
    {
        _tableRepository.UpdateTableStatus(tableId, newStatus);
    }
}

