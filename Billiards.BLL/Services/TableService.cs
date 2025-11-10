using Microsoft.EntityFrameworkCore.Storage;
using Billiards.DAL;
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

    /// <summary>
    /// Chuyển bàn: Chuyển hóa đơn Active từ bàn này sang bàn khác
    /// Tất cả logic được thực hiện trong một transaction để đảm bảo tính toàn vẹn dữ liệu
    /// </summary>
    /// <param name="fromTableId">ID bàn nguồn (phải đang InUse)</param>
    /// <param name="toTableId">ID bàn đích (phải đang Free)</param>
    /// <param name="employeeId">ID nhân viên thực hiện chuyển bàn (để ghi log)</param>
    /// <returns>True nếu chuyển thành công, False nếu có lỗi</returns>
    public bool TransferTable(int fromTableId, int toTableId, int employeeId)
    {
        using var context = new AppDbContext();
        using var transaction = context.Database.BeginTransaction();
        
        try
        {
            // 1. Kiểm tra điều kiện: fromTable phải "InUse", toTable phải "Free"
            var fromTable = context.Tables.Find(fromTableId);
            var toTable = context.Tables.Find(toTableId);

            if (fromTable == null)
            {
                throw new InvalidOperationException($"Không tìm thấy bàn nguồn (ID: {fromTableId}).");
            }

            if (toTable == null)
            {
                throw new InvalidOperationException($"Không tìm thấy bàn đích (ID: {toTableId}).");
            }

            if (fromTable.Status != "InUse")
            {
                throw new InvalidOperationException($"Bàn nguồn {fromTable.TableName} không đang được sử dụng (Status: {fromTable.Status}).");
            }

            if (toTable.Status != "Free")
            {
                throw new InvalidOperationException($"Bàn đích {toTable.TableName} không trống (Status: {toTable.Status}).");
            }

            // 2. Tìm hóa đơn Active của bàn nguồn
            var activeInvoice = context.Invoices
                .FirstOrDefault(i => i.TableID == fromTableId && i.Status == "Active");

            if (activeInvoice == null)
            {
                throw new InvalidOperationException($"Không tìm thấy hóa đơn Active cho bàn {fromTable.TableName}.");
            }

            // 3. Cập nhật hóa đơn: chuyển TableID sang bàn đích
            activeInvoice.TableID = toTableId;
            context.Invoices.Update(activeInvoice);

            // 4. Cập nhật bàn nguồn: giải phóng bàn cũ
            fromTable.Status = "Free";
            context.Tables.Update(fromTable);

            // 5. Cập nhật bàn đích: chiếm dụng bàn mới
            toTable.Status = "InUse";
            context.Tables.Update(toTable);

            // 6. Lưu và commit transaction
            context.SaveChanges();
            transaction.Commit();

            // (Nâng cao) Có thể ghi audit log ở đây nếu cần
            // Log: $"Nhân viên {employeeId} đã chuyển {fromTable.TableName} sang {toTable.TableName} cho Hóa đơn #{activeInvoice.ID} lúc {DateTime.Now}"

            return true;
        }
        catch (Exception)
        {
            // Rollback transaction nếu có lỗi
            transaction.Rollback();
            throw; // Re-throw để UI có thể xử lý và hiển thị thông báo lỗi
        }
    }
}

