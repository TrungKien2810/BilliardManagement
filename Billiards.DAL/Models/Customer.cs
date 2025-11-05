namespace Billiards.DAL.Models;

public class Customer
{
    public int ID { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public int LoyaltyPoints { get; set; } = 0;
    
    // Navigation property
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

