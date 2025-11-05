namespace Billiards.DAL.Models;

public class Product
{
    public int ID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? CategoryID { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    
    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}

