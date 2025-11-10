using Billiards.BLL.Services;

namespace Billiards.BLL.Services;

public static class AuthorizationHelper
{
    public static bool IsAdmin()
    {
        var session = SessionManager.Instance;
        return session.IsLoggedIn && session.CurrentAccount?.Role == "Admin";
    }

    public static bool IsCashier()
    {
        var session = SessionManager.Instance;
        return session.IsLoggedIn && session.CurrentAccount?.Role == "Cashier";
    }

    /// <summary>
    /// [DEPRECATED] Staff role has been removed. Use IsCashier() instead.
    /// Cashier now has all permissions (manage tables, orders, and checkout).
    /// </summary>
    [Obsolete("Staff role has been removed. Cashier now handles all staff and cashier responsibilities.")]
    public static bool IsStaff()
    {
        var session = SessionManager.Instance;
        return session.IsLoggedIn && session.CurrentAccount?.Role == "Staff";
    }

    public static bool HasPermission(string role)
    {
        var session = SessionManager.Instance;
        if (!session.IsLoggedIn || session.CurrentAccount == null)
            return false;

        return session.CurrentAccount.Role == role;
    }

    public static bool HasAnyPermission(params string[] roles)
    {
        var session = SessionManager.Instance;
        if (!session.IsLoggedIn || session.CurrentAccount == null)
            return false;

        return roles.Contains(session.CurrentAccount.Role);
    }
}

