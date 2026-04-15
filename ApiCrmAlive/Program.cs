using ApiCrmAlive.Context;
using ApiCrmAlive.Mappers.Vehicles;
using ApiCrmAlive.Middleware;
using ApiCrmAlive.Models;
using ApiCrmAlive.Repositories;
using ApiCrmAlive.Repositories.Companies;
using ApiCrmAlive.Repositories.Customers;
using ApiCrmAlive.Repositories.Fipe;
using ApiCrmAlive.Repositories.Leads;
using ApiCrmAlive.Repositories.LeadsInterations;
using ApiCrmAlive.Repositories.LeadLossReasons;
using ApiCrmAlive.Repositories.Marketplaces;
using ApiCrmAlive.Repositories.Sales;
using ApiCrmAlive.Repositories.Users;
using ApiCrmAlive.Repositories.Vehicles;
using ApiCrmAlive.Services;
using ApiCrmAlive.Services.Analytics;
using ApiCrmAlive.Services.Companies;
using ApiCrmAlive.Services.Customers;
using ApiCrmAlive.Services.Fipe;
using ApiCrmAlive.Services.Integrations;
using ApiCrmAlive.Services.JWT;
using ApiCrmAlive.Services.LeadInteraction;
using ApiCrmAlive.Services.Leads;
using ApiCrmAlive.Services.LeadLossReasons;
using ApiCrmAlive.Services.Marketplaces;
using ApiCrmAlive.Services.Marketplaces.MercadoLivre;
using ApiCrmAlive.Services.Sales;
using ApiCrmAlive.Services.Users;
using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Utils;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Some platforms (Coolify/Heroku-like) inject a single PORT env var and expect the app to bind to it.
// Respect it unless ASPNETCORE_URLS is already explicitly set.
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (string.IsNullOrWhiteSpace(urls) &&
    !string.IsNullOrWhiteSpace(portEnv) &&
    int.TryParse(portEnv, out var port) &&
    port is > 0 and < 65536)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]!;
static void LoadEnvFromLikelyLocations()
{
    var cwd = Directory.GetCurrentDirectory();
    var direct = Path.Combine(cwd, ".env");
    // .env is a fallback; allow real environment variables (Docker/CI/host) to override.
    if (File.Exists(direct)) { Env.NoClobber().Load(direct); return; }

    var probe = cwd;
    for (int i = 0; i < 5; i++)
    {
        probe = Directory.GetParent(probe)?.FullName ?? probe;
        var candidate = Path.Combine(probe, ".env");
        if (File.Exists(candidate)) { Env.NoClobber().Load(candidate); return; }
    }

    try { Env.NoClobber().Load(); } catch { /* ignore */ }
}
LoadEnvFromLikelyLocations();

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpClient();

string? cs = builder.Configuration.GetConnectionString("DefaultConnection");

// In Development, include provider error details (e.g., which column violated constraints).
// Keep it off elsewhere to avoid leaking sensitive data.
if (builder.Environment.IsDevelopment())
{
    var csb = new NpgsqlConnectionStringBuilder(cs)
    {
        IncludeErrorDetail = true
    };
    cs = csb.ConnectionString;
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(cs, npgsql =>
    {
        npgsql.CommandTimeout(120);
        npgsql.EnableRetryOnFailure();
    })
    .LogTo(Console.WriteLine, LogLevel.Information)
);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IFipeBrandRepository, FipeBrandRepository>();
builder.Services.AddScoped<IFipeBrandService, FipeBrandService>();
builder.Services.AddScoped<IFipeModelRepository, FipeModelRepository>();
builder.Services.AddScoped<IFipeModelService, FipeModelService>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ILeadInteractionService, LeadInteractionService>();
builder.Services.AddScoped<ILeadInteractionRepository, LeadInteractionRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ILeadLossReasonRepository, LeadLossReasonRepository>();
builder.Services.AddScoped<ILeadLossReasonService, LeadLossReasonService>();

builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddHttpClient<IMercadoLivreAuthService, MercadoLivreAuthService>(client =>
{
    client.BaseAddress = new Uri("https://api.mercadolivre.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IMarketplaceConfigurationRepository, MarketplaceConfigurationRepository>();
builder.Services.AddScoped<IMarketplaceLogRepository, MarketplaceLogRepository>();
builder.Services.AddScoped<IMarketplaceRepository, MarketplaceRepository>();

builder.Services.AddScoped<IMarketplaceConfigurationService, MarketplaceConfigurationService>();
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();

// Registro do serviço de integração com Evolution WhatsApp
builder.Services.AddHttpClient<IEvolutionWhatsappService, EvolutionWhatsappService>(client =>
{
    var evolutionBaseUrl = builder.Configuration["Evolution:BaseUrl"];
    if (string.IsNullOrWhiteSpace(evolutionBaseUrl))
        throw new InvalidOperationException("Evolution:BaseUrl não está configurado.");
    client.BaseAddress = new Uri(evolutionBaseUrl);
    client.DefaultRequestHeaders.Add("apikey", builder.Configuration["Evolution:ApiKey"]);
});

builder.Services.AddSingleton<VehicleMapper>();
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<IFileUploader, MinioFileUploader>();
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new NullableGuidJsonConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "API CRM Alive", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: **Bearer {seu_token_aqui}**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    // Require auth by default for all endpoints unless [AllowAnonymous] is present.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// If running behind a reverse proxy (Ingress/Nginx/Traefik/ALB), respect X-Forwarded-* so:
// - HTTPS redirection works correctly
// - generated URLs and scheme-aware behaviors are correct
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Default is loopback-only; in container deployments the proxy is usually not loopback.
// If you want to lock this down, configure KnownProxies/KnownNetworks explicitly per environment.
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Health endpoint that stays reachable over plain HTTP (no HTTPS redirect), so reverse-proxy healthchecks
// don't fail with a 30x and mark the service as down.
app.UseWhen(
    ctx => ctx.Request.Path.Equals("/healthz", StringComparison.Ordinal),
    healthApp =>
    {
        healthApp.Run(async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"status\":\"ok\"}");
        });
    });

if (string.Equals(Environment.GetEnvironmentVariable("APPLY_MIGRATIONS"), "true", StringComparison.OrdinalIgnoreCase))
{
    // For local/dev containers: apply migrations automatically, with a small retry window while db is starting.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var strictMigrations =
        string.Equals(Environment.GetEnvironmentVariable("APPLY_MIGRATIONS_STRICT"), "true", StringComparison.OrdinalIgnoreCase);

    // If the app was built without migrations compiled into the assembly, Migrate() is a no-op.
    // In strict mode, fail fast. Otherwise, fallback to EnsureCreated to avoid crash loops.
    var knownMigrations = db.Database.GetMigrations().ToList();
    if (knownMigrations.Count == 0)
    {
        var msg = "EF Core migrations not found in the application assembly. " +
                  "Ensure the Migrations folder is present and committed/built.";
        Console.Error.WriteLine($"[migrations] {msg}");
        if (strictMigrations) throw new InvalidOperationException(msg);

        try
        {
            var created = db.Database.EnsureCreated();
            Console.WriteLine($"[migrations] Fallback EnsureCreated executed. Database created: {created}.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[migrations] EnsureCreated fallback failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
    else
    {
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                db.Database.Migrate();
                lastError = null;
                break;
            }
            catch (Exception ex) when (attempt < 10)
            {
                lastError = ex;
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        if (lastError is not null)
        {
            Console.Error.WriteLine($"[migrations] Failed after retries: {lastError.GetType().Name}: {lastError.Message}");
            if (strictMigrations) throw lastError;
        }
    }
}

// Dev bootstrap: keep the API usable before multi-tenant setup is wired end-to-end.
// If the DB has no Company yet, create a default one so company-scoped features can operate.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Companies.Any())
    {
        db.Companies.Add(new Company
        {
            Name = "Default Company",
            Cnpj = "00000000000000",
            Phone = "0000000000",
            Email = "dev@local.invalid",
            UpdatedBy = Guid.Empty
        });
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API CRM Alive v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();

app.Run();
