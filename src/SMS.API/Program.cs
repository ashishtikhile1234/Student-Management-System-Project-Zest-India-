using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SMS.API.Middleware;
using SMS.Application.Interfaces;
using SMS.Application.Services;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Repositories;

// ── Serilog early bootstrap logger ────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Student Management System API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog from appsettings.json ──────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

    // ── Database ───────────────────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ── Repositories ───────────────────────────────────────────────────────────
    builder.Services.AddScoped<IStudentRepository, StudentRepository>();
    builder.Services.AddScoped<UserRepository>();

    // ── Services ───────────────────────────────────────────────────────────────
    builder.Services.AddScoped<IStudentService, StudentService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ── JWT Authentication ─────────────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey   = jwtSettings["SecretKey"]
                      ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew                = TimeSpan.Zero  // No tolerance on expiry
        };
    });

    builder.Services.AddAuthorization();

    // ── Controllers ────────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            // Return standard ApiResponse for model validation errors
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    success = false,
                    message = "Validation failed.",
                    errors
                };

                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
            };
        });

    // ── Swagger / OpenAPI ──────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "Student Management System API",
            Version     = "v1",
            Description = "ASP.NET Core 8 Web API for managing student records. Secured with JWT Bearer authentication.",
            Contact     = new OpenApiContact
            {
                Name  = "Ashish Tikhile",
                Email = "ashishtikhile1234@github.com"
            }
        });

        // Enable XML comments for Swagger
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);

        // JWT Bearer token in Swagger UI
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── CORS ────────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // ─────────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────────

    // ── Global Exception Middleware (must be first) ────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // ── Swagger (development only) ─────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Management System API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();  // Logs every HTTP request
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Auto-apply EF Core migrations on startup ───────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }

    Log.Information("SMS API started. Swagger: http://localhost:5000/swagger");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
