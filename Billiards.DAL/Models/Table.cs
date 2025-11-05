namespace Billiards.DAL.Models;

public class Table
{
    public int ID { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int? AreaID { get; set; }
    public int? TypeID { get; set; }
    public string Status { get; set; } = "Free"; // Free, InUse, Reserved, Maintenance
    
    // Navigation properties
    public virtual Area? Area { get; set; }
    public virtual TableType? TableType { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

