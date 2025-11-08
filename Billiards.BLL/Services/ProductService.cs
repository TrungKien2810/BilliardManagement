using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly ProductCategoryRepository _categoryRepository;

    public ProductService()
    {
        _productRepository = new ProductRepository();
        _categoryRepository = new ProductCategoryRepository();
    }

    public ProductService(ProductRepository productRepository, ProductCategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // Product methods
    public List<Product> GetAllProducts()
    {
        return _productRepository.GetAll();
    }

    public Product? GetProductById(int productId)
    {
        return _productRepository.GetById(productId);
    }

    public void AddProduct(Product product)
    {
        _productRepository.Add(product);
    }

    public void UpdateProduct(Product product)
    {
        _productRepository.Update(product);
    }

    public void DeleteProduct(int productId)
    {
        _productRepository.Delete(productId);
    }

    // Category methods
    public List<ProductCategory> GetAllCategories()
    {
        return _categoryRepository.GetAll();
    }

    public ProductCategory? GetCategoryById(int categoryId)
    {
        return _categoryRepository.GetById(categoryId);
    }

    public void AddCategory(ProductCategory category)
    {
        _categoryRepository.Add(category);
    }

    public void UpdateCategory(ProductCategory category)
    {
        _categoryRepository.Update(category);
    }

    public void DeleteCategory(int categoryId)
    {
        _categoryRepository.Delete(categoryId);
    }
}

