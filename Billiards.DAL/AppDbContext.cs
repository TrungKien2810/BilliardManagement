using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Area> Areas { get; set; }
    public DbSet<TableType> TableTypes { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    public DbSet<HourlyPricingRule> HourlyPricingRules { get; set; }

    public static string? ConnectionString { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use connection string from App if available, otherwise use default
            var connectionString = ConnectionString ?? "Server=.\\SQLEXPRESS;Database=BilliardsDB;Trusted_Connection=True;TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Area
        modelBuilder.Entity<Area>(entity =>
        {
            entity.ToTable("Areas");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.AreaName).HasColumnName("AreaName").HasMaxLength(100).IsRequired();
        });

        // Configure TableType
        modelBuilder.Entity<TableType>(entity =>
        {
            entity.ToTable("TableTypes");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.TypeName).HasColumnName("TypeName").HasMaxLength(100).IsRequired();
        });

        // Configure Table
        modelBuilder.Entity<Table>(entity =>
        {
            entity.ToTable("Tables");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.TableName).HasColumnName("TableName").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(50).IsRequired().HasDefaultValue("Free");
            
            entity.HasOne(e => e.Area)
                .WithMany(a => a.Tables)
                .HasForeignKey(e => e.AreaID)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.TableType)
                .WithMany(tt => tt.Tables)
                .HasForeignKey(e => e.TypeID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ProductCategory
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.CategoryName).HasColumnName("CategoryName").HasMaxLength(100).IsRequired();
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.ProductName).HasColumnName("ProductName").HasMaxLength(200).IsRequired();
            entity.Property(e => e.SalePrice).HasColumnName("SalePrice").HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.StockQuantity).HasColumnName("StockQuantity").IsRequired();
            entity.Property(e => e.MinimumStock).HasColumnName("MinimumStock").IsRequired().HasDefaultValue(10);
            
            entity.HasOne(e => e.Category)
                .WithMany(pc => pc.Products)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(200).IsRequired();
            entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
            entity.Property(e => e.Address).HasColumnName("Address").HasMaxLength(500);
        });

        // Configure Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Username);
            entity.Property(e => e.Username).HasColumnName("Username").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Password).HasColumnName("Password").IsRequired();
            entity.Property(e => e.Role).HasColumnName("Role").HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Employee)
                .WithOne(emp => emp.Account)
                .HasForeignKey<Account>(e => e.EmployeeID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.FullName).HasColumnName("FullName").HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
            entity.Property(e => e.LoyaltyPoints).HasColumnName("LoyaltyPoints").HasDefaultValue(0);
            
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
        });

        // Configure Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.StartTime).HasColumnName("StartTime").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("EndTime");
            entity.Property(e => e.TableFee).HasColumnName("TableFee").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.ProductFee).HasColumnName("ProductFee").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.Discount).HasColumnName("Discount").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.TotalAmount).HasColumnName("TotalAmount").HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.Status).HasColumnName("Status").HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Table)
                .WithMany(t => t.Invoices)
                .HasForeignKey(e => e.TableID)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.CreatedByEmployee)
                .WithMany(emp => emp.Invoices)
                .HasForeignKey(e => e.CreatedByEmployeeID)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.CustomerID)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure InvoiceDetail
        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.ToTable("InvoiceDetails");
            entity.HasKey(e => new { e.InvoiceID, e.ProductID });
            entity.Property(e => e.Quantity).HasColumnName("Quantity").IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)").IsRequired();
            
            entity.HasOne(e => e.Invoice)
                .WithMany(inv => inv.InvoiceDetails)
                .HasForeignKey(e => e.InvoiceID)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.InvoiceDetails)
                .HasForeignKey(e => e.ProductID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure HourlyPricingRule
        modelBuilder.Entity<HourlyPricingRule>(entity =>
        {
            entity.ToTable("HourlyPricingRules");
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.StartTimeSlot).HasColumnName("StartTimeSlot").IsRequired();
            entity.Property(e => e.EndTimeSlot).HasColumnName("EndTimeSlot").IsRequired();
            entity.Property(e => e.PricePerHour).HasColumnName("PricePerHour").HasColumnType("decimal(18,2)").IsRequired();
            
            entity.HasOne(e => e.TableType)
                .WithMany(tt => tt.HourlyPricingRules)
                .HasForeignKey(e => e.TableTypeID)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

