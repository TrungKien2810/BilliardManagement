namespace Billiards.DAL.Models;

public class Invoice
{
    public int ID { get; set; }
    public int? TableID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? CreatedByEmployeeID { get; set; }
    public int? CustomerID { get; set; }
    public decimal TableFee { get; set; } = 0;
    public decimal ProductFee { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public string Status { get; set; } = "Active"; // Active, Paid, Cancelled
    
    // Navigation properties
    public virtual Table? Table { get; set; }
    public virtual Employee? CreatedByEmployee { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}

