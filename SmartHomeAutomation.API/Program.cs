using Microsoft.EntityFrameworkCore;
using SmartHomeAutomation.API.Mapping;
using SmartHomeAutomation.Core.Entities;
using SmartHomeAutomation.API.Services;
using SmartHomeAutomation.Core.Interfaces;
using SmartHomeAutomation.Infrastructure.Data;
using FluentValidation.AspNetCore;
using SmartHomeAutomation.API.Validators;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using SmartHomeAutomation.API.Interfaces;
using SmartHomeAutomation.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;
using SmartHomeAutomation.API.Middleware;
using SmartHomeAutomation.API.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/smartHomeApi-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<CreateRoomDtoValidator>();
        fv.DisableDataAnnotationsValidation = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "SmartHome-Super-Secret-Key-256-Bits-Long-For-JWT-Signing-Security";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "SmartHomeAutomation",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "SmartHomeAutomation",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add DbContext - PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("SmartHomeAutomation.API")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<SmartHomeAutomation.API.Services.ISceneService, SmartHomeAutomation.API.Services.SceneService>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddScoped<SmartHomeAutomation.API.Services.IJwtService, SmartHomeAutomation.API.Services.JwtService>();

// Add new services
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ISecurityTestService, SecurityTestService>();

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<SmartHomeAutomation.API.HealthChecks.DatabaseHealthCheck>("database");

// Add API Versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = Asp.Versioning.ApiVersionReader.Combine(
        new Asp.Versioning.QueryStringApiVersionReader("version"),
        new Asp.Versioning.HeaderApiVersionReader("X-Version"),
        new Asp.Versioning.UrlSegmentApiVersionReader()
    );
}).AddApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Add middleware to log HTTP requests
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
});

// CORS yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7047", "http://localhost:7047", "http://localhost:5292", "https://localhost:5292")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    
    // Development için daha geniş policy
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // Enable serving static files from wwwroot

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// CORS middleware'ini etkinleştir - HTTPS redirection'dan önce olmalı
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("AllowFrontend");
}

// Static dosyaları etkinleştir
app.UseDefaultFiles();
app.UseStaticFiles();

// HTTPS redirection ve security headers
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts(); // HTTP Strict Transport Security
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'";
    
    await next();
});

// Add middleware to handle OPTIONS requests for CORS preflight
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        context.Response.StatusCode = 200;
        return;
    }
    await next();
});

// Add custom middleware
app.UseMiddleware<SmartHomeAutomation.API.Middleware.RateLimitingMiddleware>();
app.UseMiddleware<SmartHomeAutomation.API.Middleware.CsrfProtectionMiddleware>();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Add Health Checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Hata yakalama middleware'i
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Bir hata oluştu.",
                Detail = app.Environment.IsDevelopment() ? ex.Message : null
            });
        }
    });
});

// Serve index.html for the root path
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/index.html");
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

try
{
    Log.Information("Starting web application");
    // Seed database with test data in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is recreated for testing
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Seed data if not exists
        if (!context.Users.Any())
        {
            var testUser = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = HashPassword("123456"),
                PhoneNumber = "1234567890",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(testUser);
            context.SaveChanges(); // Save user first to get ID
            
            var testRoom = new Room
            {
                Name = "Oturma Odası",
                Description = "Ana oturma odası",
                Floor = 1,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Rooms.Add(testRoom);
            context.SaveChanges(); // Save room to get ID
            
            var testDevice = new Device
            {
                Name = "Akıllı Lamba",
                Type = "LIGHT",
                Status = "OFF",
                IsOnline = true,
                IpAddress = "192.168.1.100",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                FirmwareVersion = "1.0.0",
                RoomId = testRoom.Id,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Devices.Add(testDevice);
            
            var testScene = new Scene
            {
                Name = "Film Gecesi",
                Description = "Film izlemek için ideal ortam",
                Icon = "film",
                IsActive = true,
                UserId = testUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Scenes.Add(testScene);
            context.SaveChanges(); // Save device and scene to get IDs
            
            var sceneDevice = new SceneDevice
            {
                SceneId = testScene.Id,
                DeviceId = testDevice.Id,
                TargetState = "ON",
                TargetValue = "50", // 50% brightness
                Order = 1
            };
            context.SceneDevices.Add(sceneDevice);
            context.SaveChanges();
        }
    }
}

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
