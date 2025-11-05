namespace Billiards.DAL.Models;

public class TableType
{
    public int ID { get; set; }
    public string TypeName { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<Table> Tables { get; set; } = new List<Table>();
    public virtual ICollection<HourlyPricingRule> HourlyPricingRules { get; set; } = new List<HourlyPricingRule>();
}

