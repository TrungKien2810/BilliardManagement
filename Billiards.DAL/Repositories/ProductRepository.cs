using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class ProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository()
    {
        _context = new AppDbContext();
    }

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<ProductCategory> GetCategories()
    {
        return _context.ProductCategories.ToList();
    }

    public List<Product> GetProductsByCategory(int categoryId)
    {
        return _context.Products
            .Where(p => p.CategoryID == categoryId)
            .ToList();
    }

    public List<Product> GetAll()
    {
        return _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.ProductName)
            .ToList();
    }

    public Product? GetById(int productId)
    {
        return _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.ID == productId);
    }

    public bool IsProductNameExists(string productName, int? excludeProductId = null)
    {
        var query = _context.Products.Where(p => p.ProductName.Trim().ToLower() == productName.Trim().ToLower());
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.ID != excludeProductId.Value);
        }
        return query.Any();
    }

    public void Add(Product product)
    {
        if (IsProductNameExists(product.ProductName))
        {
            throw new InvalidOperationException($"Tên sản phẩm \"{product.ProductName}\" đã tồn tại!");
        }
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void Update(Product product)
    {
        if (IsProductNameExists(product.ProductName, product.ID))
        {
            throw new InvalidOperationException($"Tên sản phẩm \"{product.ProductName}\" đã tồn tại!");
        }
        _context.Products.Update(product);
        _context.SaveChanges();
    }

    public bool IsProductInInvoiceDetails(int productId)
    {
        return _context.InvoiceDetails
            .Any(id => id.ProductID == productId);
    }

    public void Delete(int productId)
    {
        var product = _context.Products.FirstOrDefault(p => p.ID == productId);
        if (product != null)
        {
            // Check if product is used in any invoice details
            if (IsProductInInvoiceDetails(productId))
            {
                throw new InvalidOperationException($"Không thể xóa sản phẩm \"{product.ProductName}\" vì sản phẩm đã được sử dụng trong các hóa đơn!");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}

