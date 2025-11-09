using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class CustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository()
    {
        _context = new AppDbContext();
    }

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Customer> GetAll()
    {
        return _context.Customers.OrderBy(c => c.FullName).ToList();
    }

    public Customer? GetById(int customerId)
    {
        return _context.Customers.FirstOrDefault(c => c.ID == customerId);
    }

    public Customer? GetByPhoneNumber(string phoneNumber)
    {
        return _context.Customers.FirstOrDefault(c => c.PhoneNumber == phoneNumber);
    }

    public void Add(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
        _context.SaveChanges();
    }

    public void Delete(int customerId)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.ID == customerId);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            _context.SaveChanges();
        }
    }
}

