namespace Billiards.DAL.Models;

public class HourlyPricingRule
{
    public int ID { get; set; }
    public int? TableTypeID { get; set; }
    public TimeSpan StartTimeSlot { get; set; }
    public TimeSpan EndTimeSlot { get; set; }
    public decimal PricePerHour { get; set; }
    
    // Navigation property
    public virtual TableType? TableType { get; set; }
}

