using Supabase;

public static class SupabaseClientFactory
{
    public static Supabase.Client CreateClient()
    {
        var url = "https://<sua-url>.supabase.co"; // Substitua pela URL do seu Supabase
        var key = "<sua-chave-api>"; // Substitua pela sua chave API
        return new Supabase.Client(url, key);
    }
}