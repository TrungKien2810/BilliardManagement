using Billiards.DAL;
using Billiards.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Billiards.BLL.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService()
    {
        _context = new AppDbContext();
    }

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public Account? Login(string username, string password)
    {
        try
        {
            var account = _context.Accounts
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.Username == username);

            if (account == null)
            {
                return null;
            }

            // Tạm thời so sánh password trực tiếp (chưa hash)
            if (account.Password == password)
            {
                return account;
            }

            return null;
        }
        catch
        {
            // Re-throw để UI layer xử lý
            throw;
        }
    }
}

