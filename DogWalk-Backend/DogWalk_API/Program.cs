using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DogWalk_Domain.Entities;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.OpenApi.Models;
using DogWalk_Application.Services;
using MediatR;
using System.Reflection;
using DogWalk_Application.Common.Behaviors;
using DogWalk_Infrastructure.Persistence.Repositories;
using DogWalk_Domain.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;
using DogWalk_Infrastructure.Persistence;
using DogWalk_Infrastructure.Authentication;
using DogWalk_API.Hubs;
using DogWalk_Infrastructure.Services.Stripe;
using DogWalk_Infrastructure;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using DogWalk_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configuración de SignalR
builder.Services.AddSignalR(options => 
{
    options.EnableDetailedErrors = true; 
});
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DogWalk API", Version = "v1" });
    
    // Configuración para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// Configuración CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL del frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // SignalR
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

// Registra los repositorios individuales
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPaseadorRepository, PaseadorRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
// ... otros repositorios


// Servicios para seguridad mejorada
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "DogWalk_Keys")))
    .SetApplicationName("DogWalk");

// Configuración de Rate Limiting para prevención de abusos en endpoints de autenticación
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context => {
        // Solo aplicar rate limiting a endpoints de autenticación
        if (context.Request.Path.StartsWithSegments("/api/Auth"))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anónimo",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                });
        }
        return RateLimitPartition.GetNoLimiter("default");
    });

    options.OnRejected = async (context, _) => {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Demasiadas solicitudes. Por favor, inténtalo de nuevo más tarde.");
    };
});

// Registra el contexto de la base de datos
builder.Services.AddDbContext<DogWalkDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra servicios de la aplicación
builder.Services.AddScoped<AdminService>();
// ... otros servicios

// Añade la configuración de MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(DogWalk_Application.Features.Admin.Commands.CreateAdminCommand).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Registra manualmente JwtProvider
builder.Services.AddScoped<JwtProvider>();

// IMPORTANTE: deshabilita la configuración de autenticación en Infrastructure 
// ya que la hemos configurado manualmente arriba
builder.Services.AddInfrastructure(builder.Configuration, configureAuthentication: false);

// En Program.cs, configura las opciones de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DogWalk API v1"));
}

// Inicializar datos
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Inicialización de datos si es necesario
}

// En Program.cs, después de app.UseRouting()
app.Use(async (context, next) =>
{
    // Seguridad adicional para prevenir ataques
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    if (!builder.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    await next();
});


app.UseHttpsRedirection();
app.UseHsts();

// CORS debe ir antes de Routing
app.UseCors("AllowSignalR");

app.UseRouting();
// En Program.cs, después de app.UseRouting()
app.UseRateLimiter(new RateLimiterOptions
{
    GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Limita las solicitudes a los endpoints de autenticación
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anónimo",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1)
                });
        }
        return RateLimitPartition.GetNoLimiter("default");
    }),
    OnRejected = (context, _) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        return ValueTask.CompletedTask;
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AntiforgeryMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();