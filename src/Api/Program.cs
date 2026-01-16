using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context with dynamic secret injection
// Note: appsettings.json contains a template with 'REPLACE_ME' for the password.
// For security, the actual password is injected from environment variables or .NET User Secrets
// at runtime, preventing sensitive credentials from being committed to the repository.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbPassword = builder.Configuration["DARKGRAVITY_DB_PASSWORD"];

if (!string.IsNullOrEmpty(dbPassword))
{
    var connectionBuilder = new SqlConnectionStringBuilder(connectionString)
    {
        Password = dbPassword
    };
    connectionString = connectionBuilder.ConnectionString;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// CORS Configuration
// Note: 'AllowedOrigins' in appsettings.json is left empty by design.
// This enforces the 'Clean Code' principle where infrastructure details stay out of the code.
// In Production, the allowed domains are loaded from:
// 1. .NET User Secrets (Development)
// 2. Environment Variables (Production / CI-CD) - e.g. AllowedOrigins__0=https://domain.com
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // In Production, restrict to known origins defined in external configuration
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            if (allowedOrigins.Any())
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }
        }
    });
});

var app = builder.Build();

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
