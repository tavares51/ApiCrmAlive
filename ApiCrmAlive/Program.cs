using ApiCrmAlive.Services.Users;
using ApiCrmAlive.Context;
using ApiCrmAlive.Repositories.Users;
using ApiCrmAlive.Repositories;
using ApiCrmAlive.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// HttpClient
builder.Services.AddHttpClient();

// Connection string
string cs = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

// DbContext Pool + Npgsql
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(cs, npgsql =>
    {
        npgsql.CommandTimeout(120);
        npgsql.EnableRetryOnFailure();
    })
    .LogTo(Console.WriteLine, LogLevel.Information)
);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI (ESSENCIAL para gerar "openapi": "3.x.y")
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API CRM Alive",
        Version = "v1",
        Description = "Endpoints de Usuários"
    });
    c.EnableAnnotations(); // opcional
});

var app = builder.Build();

// Swagger SEM proteção/redirect (evita HTML no lugar do JSON)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // tem que bater com o nome do doc acima ("v1")
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API CRM Alive v1");
    c.RoutePrefix = "swagger"; // abre em /swagger
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
