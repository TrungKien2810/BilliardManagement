using Billiards.DAL;
using Billiards.DAL.Models;
using Billiards.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Billiards.BLL.Services;

public class OrderService
{
    private readonly ProductRepository _productRepository;
    private readonly InvoiceRepository _invoiceRepository;
    private readonly AppDbContext _context;

    public OrderService()
    {
        _context = new AppDbContext();
        _productRepository = new ProductRepository(_context);
        _invoiceRepository = new InvoiceRepository(_context);
    }

    public List<ProductCategory> GetMenuCategories()
    {
        return _productRepository.GetCategories();
    }

    public List<Product> GetMenuProducts(int categoryId)
    {
        return _productRepository.GetProductsByCategory(categoryId);
    }

    public void AddProductToInvoice(int invoiceId, int productId, int quantity)
    {
        // Get product to check stock
        var product = _productRepository.GetById(productId);
        if (product == null)
        {
            throw new Exception("Sản phẩm không tồn tại!");
        }

        // Check stock
        if (product.StockQuantity < quantity)
        {
            throw new Exception($"Không đủ hàng trong kho! Hiện tại chỉ còn {product.StockQuantity} sản phẩm.");
        }

        // Get invoice
        var invoice = _invoiceRepository.GetById(invoiceId);
        if (invoice == null)
        {
            throw new Exception("Hóa đơn không tồn tại!");
        }

        // Check if product already exists in invoice
        var existingDetail = _context.InvoiceDetails
            .FirstOrDefault(id => id.InvoiceID == invoiceId && id.ProductID == productId);

        if (existingDetail != null)
        {
            // Update quantity
            existingDetail.Quantity += quantity;
        }
        else
        {
            // Create new invoice detail
            var invoiceDetail = new InvoiceDetail
            {
                InvoiceID = invoiceId,
                ProductID = productId,
                Quantity = quantity,
                UnitPrice = product.SalePrice
            };
            _context.InvoiceDetails.Add(invoiceDetail);
        }

        // Update stock
        product.StockQuantity -= quantity;
        _productRepository.Update(product);

        // Calculate and update ProductFee
        var totalProductFee = _context.InvoiceDetails
            .Where(id => id.InvoiceID == invoiceId)
            .Sum(id => id.Quantity * id.UnitPrice);

        invoice.ProductFee = totalProductFee;
        _invoiceRepository.Update(invoice);

        _context.SaveChanges();
    }
}

