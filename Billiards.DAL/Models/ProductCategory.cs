namespace Billiards.DAL.Models;

public class ProductCategory
{
    public int ID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    // Navigation property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

