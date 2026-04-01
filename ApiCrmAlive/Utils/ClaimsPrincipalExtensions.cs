using System.Security.Claims;
using ApiCrmAlive.Context;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Utils;

public static class ClaimsPrincipalExtensions
{
    public const string CompanyIdClaim = "company_id";

    public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var id))
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        return id;
    }

    public static Guid GetCompanyIdOrThrow(this ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(CompanyIdClaim);
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var id))
            throw new UnauthorizedAccessException("Empresa do usuário não encontrada no token.");
        return id;
    }

    public static async Task<Guid> GetCompanyIdOrThrowAsync(
        this ClaimsPrincipal user,
        AppDbContext ctx,
        CancellationToken ct = default)
    {
        // Prefer explicit claim when present (fast path).
        var raw = user.FindFirstValue(CompanyIdClaim);
        if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var companyId))
            return companyId;

        // Backward-compatible fallback: resolve company from DB using authenticated user id.
        var userId = user.GetUserIdOrThrow();
        var dbCompanyId = await ctx.Users
            .Where(u => u.Id == userId)
            .Select(u => u.CompanyId)
            .SingleOrDefaultAsync(ct);

        if (dbCompanyId.HasValue)
            return dbCompanyId.Value;

        // Dev/single-tenant fallback: if the user isn't linked to a company yet, use the first company in DB.
        // This keeps the API usable during early setup while still using the company-scoped schema.
        var firstCompanyId = await ctx.Companies
            .Select(c => c.Id)
            .OrderBy(id => id)
            .FirstOrDefaultAsync(ct);

        if (firstCompanyId != Guid.Empty)
            return firstCompanyId;

        throw new UnauthorizedAccessException("Empresa não encontrada.");
    }

    public static string? GetRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role);
}
