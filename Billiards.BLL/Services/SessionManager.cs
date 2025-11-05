using Billiards.DAL.Models;

namespace Billiards.BLL.Services;

public class SessionManager
{
    private static SessionManager? _instance;
    private static readonly object _lock = new object();

    public static SessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SessionManager();
                    }
                }
            }
            return _instance;
        }
    }

    private SessionManager() { }

    public Account? CurrentAccount { get; private set; }
    public Employee? CurrentEmployee { get; private set; }

    public bool IsLoggedIn => CurrentAccount != null;

    public void SetSession(Account account, Employee? employee)
    {
        CurrentAccount = account;
        CurrentEmployee = employee;
    }

    public void Logout()
    {
        CurrentAccount = null;
        CurrentEmployee = null;
    }
}

