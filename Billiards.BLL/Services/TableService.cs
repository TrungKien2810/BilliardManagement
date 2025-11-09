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

    public List<Table> GetTableMapByArea(int areaId)
    {
        return _tableRepository.GetTablesByArea(areaId);
    }

    public void UpdateTableStatus(int tableId, string newStatus)
    {
        _tableRepository.UpdateTableStatus(tableId, newStatus);
    }

    public Invoice StartSession(int tableId, int employeeId)
    {
        // Kiểm tra tình trạng hiện tại từ DB
        var table = _tableRepository.GetById(tableId);
        var existingActive = _invoiceRepository.GetActiveInvoiceByTable(tableId);

        // Nếu có hóa đơn Active nhưng bàn lại đang Free (trạng thái lệch), đóng hóa đơn cũ an toàn rồi mở phiên mới
        if (existingActive != null && table != null && table.Status == "Free")
        {
            _invoiceRepository.CloseActiveInvoiceForTable(tableId);
            existingActive = null;
        }

        // Nếu sau khi kiểm tra vẫn còn Active invoice -> tiếp tục phiên cũ
        if (existingActive != null)
        {
            _tableRepository.UpdateTableStatus(tableId, "InUse");
            return existingActive;
        }

        // Update table status to InUse
        _tableRepository.UpdateTableStatus(tableId, "InUse");

        // Create new invoice
        var invoice = _invoiceRepository.CreateNewInvoice(tableId, employeeId);

        return invoice;
    }
}

