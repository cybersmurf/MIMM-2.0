using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Refit;
using Serilog;
using System.Text;
using MIMM.Backend.Data;
using MIMM.Backend.Services;
using MIMM.Backend.Middleware;
using FluentValidation;
using MIMM.Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURATION: Load environment variables (for Docker) ===
builder.Configuration.AddEnvironmentVariables();

// === LOGGING (Serilog) ===
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/mimm-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// === DATABASE ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Build connection string from environment variables (for Docker production) or appsettings
    var connectionString = BuildConnectionString(builder.Configuration);
    options.UseNpgsql(connectionString)
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// === AUTHENTICATION (JWT) ===
var jwtKey = builder.Configuration["JWT_SECRET_KEY"] ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured (set JWT_SECRET_KEY or Jwt:Key)");
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer not configured (set JWT_ISSUER or Jwt:Issuer)");
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience not configured (set JWT_AUDIENCE or Jwt:Audience)");

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
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(10)
    };

    // SignalR support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "https://localhost:5001", "http://localhost:5000", "http://localhost:5173", "https://localhost:5173" };
        
        policyBuilder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// === SERVICES (Dependency Injection) ===
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEntryService, EntryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<ILastFmService, LastFmService>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddHttpClient<IMusicSearchService, MusicSearchService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient("lastfm", c => c.BaseAddress = new Uri("https://ws.audioscrobbler.com/2.0/"));

// Caching
builder.Services.AddMemoryCache();

// === HTTP CLIENTS (Refit) ===
builder.Services.AddRefitClient<ILastFmHttpClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://ws.audioscrobbler.com"));

builder.Services.AddRefitClient<ISpotifyHttpClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.spotify.com"));

// === VALIDATION (FluentValidation) ===
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// === CONTROLLERS & SIGNALR ===
builder.Services.AddControllers();
builder.Services.AddSignalR();

// === SWAGGER/OPENAPI ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MIMM API",
        Version = "v1",
        Description = "Music & Mood Journal API"
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// === CACHING (optional Redis) ===
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}

var app = builder.Build();

// === MIDDLEWARE PIPELINE ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts(); // Only enforce HSTS in production
}

// No HTTPS redirect – backend listens on HTTP only in dev
app.UseCors("AllowFrontend");

// Security middleware
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

// Custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hubs
app.MapHub<NotificationHub>("/hubs/notification");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Error endpoint
app.MapGet("/error", () => Results.Problem("An error occurred."));

// === DATABASE MIGRATION (auto-migrate on startup in all environments) ===
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    Log.Information("Database migrations applied successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "Error applying database migrations");
    // Don't crash on migration failure, backend might still serve read-only
}

Log.Information("MIMM Backend starting...");
app.Run();

// === HELPER: Build connection string from environment or appsettings ===
static string BuildConnectionString(IConfiguration config)
{
    // Try environment variables first (Docker production)
    var host = config["POSTGRES_HOST"] ?? config["ConnectionStrings:DefaultConnection"]?.Split(';').FirstOrDefault(s => s.StartsWith("Host="))?.Replace("Host=", "");
    if (string.IsNullOrEmpty(host))
        host = config.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault(s => s.StartsWith("Host="))?.Replace("Host=", "") ?? "localhost";
    
    var port = config["POSTGRES_PORT"] ?? "5432";
    var database = config["POSTGRES_DB"] ?? config.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault(s => s.StartsWith("Database="))?.Replace("Database=", "") ?? "mimm_dev";
    var username = config["POSTGRES_USER"] ?? config.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault(s => s.StartsWith("Username="))?.Replace("Username=", "") ?? "postgres";
    var password = config["POSTGRES_PASSWORD"] ?? config.GetConnectionString("DefaultConnection")?.Split(';').FirstOrDefault(s => s.StartsWith("Password="))?.Replace("Password=", "") ?? "postgres";
    
    var connStr = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
    Log.Information("Database connection: Host={Host}, Database={Database}, User={User}", host, database, username);
    return connStr;
}

// Make Program class accessible for testing
public partial class Program { }
