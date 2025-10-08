using ApiCrmAlive.Context;
using ApiCrmAlive.Mappers.Vehicles;
using ApiCrmAlive.Repositories;
using ApiCrmAlive.Repositories.Customers;
using ApiCrmAlive.Repositories.Leads;
using ApiCrmAlive.Repositories.LeadsInterations;
using ApiCrmAlive.Repositories.Marketplaces;
using ApiCrmAlive.Repositories.Sales;
using ApiCrmAlive.Repositories.Users;
using ApiCrmAlive.Repositories.Vehicles;
using ApiCrmAlive.Services;
using ApiCrmAlive.Services.Customers;
using ApiCrmAlive.Services.Integrations;
using ApiCrmAlive.Services.JWT;
using ApiCrmAlive.Services.LeadInteraction;
using ApiCrmAlive.Services.Leads;
using ApiCrmAlive.Services.Marketplaces;
using ApiCrmAlive.Services.Marketplaces.MercadoLivre;
using ApiCrmAlive.Services.Sales;
using ApiCrmAlive.Services.Users;
using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Utils;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"]!;
static void LoadEnvFromLikelyLocations()
{
    var cwd = Directory.GetCurrentDirectory();
    var direct = Path.Combine(cwd, ".env");
    if (File.Exists(direct)) { Env.Load(direct); return; }

    var probe = cwd;
    for (int i = 0; i < 5; i++)
    {
        probe = Directory.GetParent(probe)?.FullName ?? probe;
        var candidate = Path.Combine(probe, ".env");
        if (File.Exists(candidate)) { Env.Load(candidate); return; }
    }

    try { Env.Load(); } catch { /* ignore */ }
}
LoadEnvFromLikelyLocations();

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpClient();

string? cs = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(cs))
{
    var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER");
    var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

    if (string.IsNullOrWhiteSpace(dbHost) ||
        string.IsNullOrWhiteSpace(dbName) ||
        string.IsNullOrWhiteSpace(dbUser) ||
        string.IsNullOrWhiteSpace(dbPass))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' ausente e variáveis de ambiente DB_* não definidas.");
    }

    cs = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPass};Ssl Mode=Require;Trust Server Certificate=true";
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
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ILeadInteractionService, LeadInteractionService>();
builder.Services.AddScoped<ILeadInteractionRepository, LeadInteractionRepository>();

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
builder.Services.AddSingleton<SupabaseFileUploader>();
builder.Services.AddControllers();
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

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API CRM Alive v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
