using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class ProductCategoryRepository
{
    private readonly AppDbContext _context;

    public ProductCategoryRepository()
    {
        _context = new AppDbContext();
    }

    public ProductCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<ProductCategory> GetAll()
    {
        return _context.ProductCategories.OrderBy(c => c.CategoryName).ToList();
    }

    public ProductCategory? GetById(int categoryId)
    {
        return _context.ProductCategories.FirstOrDefault(c => c.ID == categoryId);
    }

    public void Add(ProductCategory category)
    {
        _context.ProductCategories.Add(category);
        _context.SaveChanges();
    }

    public void Update(ProductCategory category)
    {
        _context.ProductCategories.Update(category);
        _context.SaveChanges();
    }

    public void Delete(int categoryId)
    {
        var category = _context.ProductCategories.FirstOrDefault(c => c.ID == categoryId);
        if (category != null)
        {
            _context.ProductCategories.Remove(category);
            _context.SaveChanges();
        }
    }
}

