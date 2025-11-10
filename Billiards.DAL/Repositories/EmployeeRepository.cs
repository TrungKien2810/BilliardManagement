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

    public bool IsPhoneNumberExists(string phoneNumber, int? excludeEmployeeId = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false; // Empty phone number is allowed
        }
        var query = _context.Employees.Where(e => e.PhoneNumber != null && e.PhoneNumber.Trim() == phoneNumber.Trim());
        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(e => e.ID != excludeEmployeeId.Value);
        }
        return query.Any();
    }

    public void Add(Employee employee)
    {
        if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && IsPhoneNumberExists(employee.PhoneNumber))
        {
            throw new InvalidOperationException($"Số điện thoại \"{employee.PhoneNumber}\" đã tồn tại!");
        }
        _context.Employees.Add(employee);
        _context.SaveChanges();
    }

    public void Update(Employee employee)
    {
        if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && IsPhoneNumberExists(employee.PhoneNumber, employee.ID))
        {
            throw new InvalidOperationException($"Số điện thoại \"{employee.PhoneNumber}\" đã tồn tại!");
        }
        _context.Employees.Update(employee);
        _context.SaveChanges();
    }

    public bool HasInvoices(int employeeId)
    {
        return _context.Invoices
            .Any(i => i.CreatedByEmployeeID == employeeId);
    }

    public void Delete(int employeeId)
    {
        var employee = _context.Employees.FirstOrDefault(e => e.ID == employeeId);
        if (employee != null)
        {
            // Check if employee has invoices
            if (HasInvoices(employeeId))
            {
                throw new InvalidOperationException($"Không thể xóa nhân viên \"{employee.FullName}\" vì nhân viên đã tạo các hóa đơn trong hệ thống!");
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();
        }
    }

    // Account methods
    public List<Account> GetAllAccounts()
    {
        // Use AsNoTracking to ensure fresh data and include password
        return _context.Accounts
            .AsNoTracking()
            .Include(a => a.Employee)
            .OrderBy(a => a.Username)
            .ToList();
    }

    public Account? GetAccountByUsername(string username)
    {
        // Use AsNoTracking to ensure fresh data and include password
        return _context.Accounts
            .AsNoTracking()
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

