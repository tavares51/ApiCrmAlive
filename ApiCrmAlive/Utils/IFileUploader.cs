using Microsoft.AspNetCore.Http;

namespace ApiCrmAlive.Utils;

public interface IFileUploader
{
    Task<List<string>> UploadAsync(List<IFormFile> files);
}

