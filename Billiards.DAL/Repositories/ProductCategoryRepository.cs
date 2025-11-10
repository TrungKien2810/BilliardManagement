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

    public bool IsCategoryNameExists(string categoryName, int? excludeCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return false;
        }
        
        // Clear change tracker to ensure fresh data
        _context.ChangeTracker.Clear();
        
        var normalizedName = categoryName.Trim().ToLower();
        var query = _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.CategoryName != null && c.CategoryName.Trim().ToLower() == normalizedName);
        
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.ID != excludeCategoryId.Value);
        }
        
        return query.Any();
    }

    public List<Product> GetProductsByCategory(int categoryId)
    {
        return _context.Products
            .Where(p => p.CategoryID == categoryId)
            .ToList();
    }

    public void Add(ProductCategory category)
    {
        if (IsCategoryNameExists(category.CategoryName))
        {
            throw new InvalidOperationException($"Tên danh mục \"{category.CategoryName}\" đã tồn tại!");
        }
        _context.ProductCategories.Add(category);
        _context.SaveChanges();
    }

    public void Update(ProductCategory category)
    {
        if (IsCategoryNameExists(category.CategoryName, category.ID))
        {
            throw new InvalidOperationException($"Tên danh mục \"{category.CategoryName}\" đã tồn tại!");
        }
        _context.ProductCategories.Update(category);
        _context.SaveChanges();
    }

    public void Delete(int categoryId)
    {
        var category = _context.ProductCategories.FirstOrDefault(c => c.ID == categoryId);
        if (category != null)
        {
            // Check if category has products
            var products = GetProductsByCategory(categoryId);
            if (products.Any())
            {
                throw new InvalidOperationException($"Không thể xóa danh mục \"{category.CategoryName}\" vì còn {products.Count} sản phẩm đang sử dụng danh mục này!");
            }

            _context.ProductCategories.Remove(category);
            _context.SaveChanges();
        }
    }
}

