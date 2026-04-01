namespace ApiCrmAlive.Utils;

public static class RoleUtils
{
    public static bool IsManagerOrAdmin(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return false;
        return string.Equals(role, "gerente", StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, "administrador", StringComparison.OrdinalIgnoreCase);
    }
}

