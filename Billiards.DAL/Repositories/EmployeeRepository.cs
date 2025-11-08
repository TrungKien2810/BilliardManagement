using Microsoft.EntityFrameworkCore;
using Billiards.DAL.Models;

namespace Billiards.DAL.Repositories;

public class EmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository()
    {
        _context = new AppDbContext();
    }

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Employee> GetAll()
    {
        return _context.Employees
            .Include(e => e.Account)
            .OrderBy(e => e.FullName)
            .ToList();
    }

    public Employee? GetById(int employeeId)
    {
        return _context.Employees
            .Include(e => e.Account)
            .FirstOrDefault(e => e.ID == employeeId);
    }

    public void Add(Employee employee)
    {
        _context.Employees.Add(employee);
        _context.SaveChanges();
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
        _context.SaveChanges();
    }

    public void Delete(int employeeId)
    {
        var employee = _context.Employees.FirstOrDefault(e => e.ID == employeeId);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            _context.SaveChanges();
        }
    }

    // Account methods
    public List<Account> GetAllAccounts()
    {
        return _context.Accounts
            .Include(a => a.Employee)
            .OrderBy(a => a.Username)
            .ToList();
    }

    public Account? GetAccountByUsername(string username)
    {
        return _context.Accounts
            .Include(a => a.Employee)
            .FirstOrDefault(a => a.Username == username);
    }

    public Account? GetAccountByEmployeeId(int employeeId)
    {
        return _context.Accounts
            .Include(a => a.Employee)
            .FirstOrDefault(a => a.EmployeeID == employeeId);
    }

    public void AddAccount(Account account)
    {
        _context.Accounts.Add(account);
        _context.SaveChanges();
    }

    public void UpdateAccount(Account account)
    {
        _context.Accounts.Update(account);
        _context.SaveChanges();
    }

    public void DeleteAccount(string username)
    {
        var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }
    }
}

