using System.Text;

namespace ApiCrmAlive.Utils;

public static class PhoneUtils
{
    /// <summary>
    /// Normaliza telefone BR para comparação/armazenamento:
    /// - mantém apenas dígitos
    /// - garante prefixo "55" (DDI) quando ausente
    /// </summary>
    public static string NormalizeBrazilPhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            if (char.IsDigit(ch))
                sb.Append(ch);
        }

        var digits = sb.ToString();
        if (digits.Length == 0)
            return string.Empty;

        return digits.StartsWith("55") ? digits : $"55{digits}";
    }
}

