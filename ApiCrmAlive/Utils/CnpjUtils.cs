using System.Text;

namespace ApiCrmAlive.Utils;

public static class CnpjUtils
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
                sb.Append(ch);
        }
        return sb.ToString();
    }

    public static bool IsValid(string? value)
    {
        var cnpj = Normalize(value);
        if (cnpj.Length != 14)
            return false;

        // reject sequences (000... / 111... etc)
        var allSame = true;
        for (var i = 1; i < cnpj.Length; i++)
        {
            if (cnpj[i] != cnpj[0])
            {
                allSame = false;
                break;
            }
        }
        if (allSame) return false;

        int DigitAt(int idx) => cnpj[idx] - '0';

        // 1st check digit
        var weights1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += DigitAt(i) * weights1[i];
        var mod = sum % 11;
        var d1 = mod < 2 ? 0 : 11 - mod;
        if (DigitAt(12) != d1)
            return false;

        // 2nd check digit
        var weights2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += DigitAt(i) * weights2[i];
        mod = sum % 11;
        var d2 = mod < 2 ? 0 : 11 - mod;
        return DigitAt(13) == d2;
    }
}

