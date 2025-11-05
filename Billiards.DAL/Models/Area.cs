namespace Billiards.DAL.Models;

public class Area
{
    public int ID { get; set; }
    public string AreaName { get; set; } = string.Empty;
    
    // Navigation property
    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
}

