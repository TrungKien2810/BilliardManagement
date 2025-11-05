namespace Billiards.DAL.Models;

public class Employee
{
    public int ID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    
    // Navigation properties
    public virtual Account? Account { get; set; }
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

