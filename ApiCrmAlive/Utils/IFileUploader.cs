using Microsoft.AspNetCore.Http;

namespace ApiCrmAlive.Utils;

public interface IFileUploader
{
    /// <summary>Faz upload dos arquivos e retorna os nomes dos objetos (paths no storage).</summary>
    Task<List<string>> UploadAsync(List<IFormFile> files);

    /// <summary>Gera URLs de acesso (presigned ou públicas) a partir dos nomes dos objetos.</summary>
    Task<List<string>> GetUrlsAsync(List<string> objectNames);
}

