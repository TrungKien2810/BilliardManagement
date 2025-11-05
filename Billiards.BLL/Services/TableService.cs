using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class TableService
{
    private readonly TableRepository _tableRepository;
    private readonly InvoiceRepository _invoiceRepository;

    public TableService()
    {
        _tableRepository = new TableRepository();
        _invoiceRepository = new InvoiceRepository();
    }

    public TableService(TableRepository tableRepository, InvoiceRepository invoiceRepository)
    {
        _tableRepository = tableRepository;
        _invoiceRepository = invoiceRepository;
    }

    public List<Table> GetTableMap()
    {
        return _tableRepository.GetAllTables();
    }

    public void UpdateTableStatus(int tableId, string newStatus)
    {
        _tableRepository.UpdateTableStatus(tableId, newStatus);
    }

    public Invoice StartSession(int tableId, int employeeId)
    {
        // Update table status to InUse
        _tableRepository.UpdateTableStatus(tableId, "InUse");

        // Create new invoice
        var invoice = _invoiceRepository.CreateNewInvoice(tableId, employeeId);

        return invoice;
    }
}

