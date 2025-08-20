using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ApiCrmAlive.Utils
{
    public class SupabaseFileUploader
    {
        private readonly HttpClient _httpClient;
        private readonly string _bucket;
        private readonly string _baseUrl;
        private readonly string _serviceRoleKey;

        public SupabaseFileUploader(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _baseUrl = config["Supabase:Url"] ?? throw new Exception("Supabase URL not configured");
            _bucket = config["Supabase:Bucket"] ?? "fotos";
            _serviceRoleKey = config["Supabase:ServiceRoleKey"] ?? throw new Exception("Service role not configured");
        }

        public async Task<List<string>> UploadAsync(List<IFormFile> files)
        {
            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                try
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var path = $"uploads/{fileName}";
                    var uploadUrl = $"{_baseUrl}/storage/v1/object/{_bucket}/{path}";

                    using var stream = file.OpenReadStream();
                    using var content = new StreamContent(stream);
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
                    {
                        Content = content
                    };
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _serviceRoleKey);

                    var response = await _httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Erro ao enviar arquivo {file.FileName}: {response.StatusCode}");
                        continue; // ignora e prossegue
                    }

                    var publicUrl = $"{_baseUrl}/storage/v1/object/public/{_bucket}/{path}";
                    urls.Add(publicUrl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar {file?.FileName}: {ex.Message}");
                }
            }

            return urls;
        }
    }

}