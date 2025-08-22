using ApiCrmAlive.Services.Users;
using ApiCrmAlive.Context;
using ApiCrmAlive.Repositories.Users;
using ApiCrmAlive.Repositories;
using ApiCrmAlive.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using ApiCrmAlive.Services.Customers;
using ApiCrmAlive.Repositories.Customers;
using ApiCrmAlive.Mappers.Vehicles;
using ApiCrmAlive.Services.Vehicles;
using ApiCrmAlive.Repositories.Vehicles;
using ApiCrmAlive.Utils;
using ApiCrmAlive.Repositories.Leads;
using ApiCrmAlive.Services.Leads;

var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddSingleton<VehicleMapper>();
builder.Services.AddSingleton<SupabaseFileUploader>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API CRM Alive",
        Version = "v1",
        Description = "Endpoints da API CRM Alive"
    });
    c.EnableAnnotations();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
    );
});

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
