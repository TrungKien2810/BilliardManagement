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

    public void Add(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void Update(Product product)
    {
        _context.Products.Update(product);
        _context.SaveChanges();
    }

    public void Delete(int productId)
    {
        var product = _context.Products.FirstOrDefault(p => p.ID == productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}

