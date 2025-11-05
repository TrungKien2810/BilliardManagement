namespace Billiards.DAL.Models;

public class Account
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? EmployeeID { get; set; }
    public string Role { get; set; } = string.Empty; // Admin, Cashier, Staff
    
    // Navigation property
    public virtual Employee? Employee { get; set; }
}

