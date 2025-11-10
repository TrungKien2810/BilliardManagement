namespace Billiards.DAL.Models;

public class Product
{
    public int ID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? CategoryID { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; } = 10; // Tồn kho tối thiểu, mặc định 10
    
    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    
    // Helper property để kiểm tra tồn kho thấp
    public bool IsLowStock => StockQuantity <= MinimumStock;
}

