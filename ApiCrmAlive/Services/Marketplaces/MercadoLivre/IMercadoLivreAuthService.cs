using System.Text.Json;
using ApiCrmAlive.DTOs.Marketplaces.MercadoLivre;

namespace ApiCrmAlive.Services.Marketplaces.MercadoLivre;

public interface IMercadoLivreAuthService
{
    Task<MlTokenDto> AuthorizeAsync(string code, CancellationToken ct = default);
    Task<MlTokenDto> RefreshAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<MlTokenDto?> CheckTokenAsync(CancellationToken ct = default);
    Task<JsonElement?> InsertVehicleAsync(MlPublishDto dto, CancellationToken ct = default);
    Task<MlPublishResponseDto?> UpdateVehicleAsync(string id, MlPublishDto dto, CancellationToken ct = default);
    Task<MlPublishResponseDto?> DeleteVehicleAsync(string id, CancellationToken ct = default);
    Task<MlPublishResponseDto?> GetVehicleAsync(string id, CancellationToken ct = default);
    Task<MlPublishResponseDto?> GetVehiclesAsync(CancellationToken ct = default);
    Task<bool> CreateUserTest(string id, CancellationToken ct = default);
}   
