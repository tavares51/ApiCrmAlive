namespace ApiCrmAlive.Utils;

public static class PasswordHasher
{
    public static string Hash(string value) => BCrypt.Net.BCrypt.HashPassword(value);
    public static bool Verify(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
}
