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
              .AllowCredentials(); // Importante para SignalR
    });
});

// IMPORTANTE: Configura la autenticación ANTES de AddInfrastructure
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
        ClockSkew = TimeSpan.Zero
    };
});

// Registra los repositorios individuales
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPaseadorRepository, PaseadorRepository>();
// ... otros repositorios

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

// IMPORTANTE: deshabilita la configuración de autenticación en Infrastructure
builder.Services.AddInfrastructure(builder.Configuration, configureAuthentication: false);

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

app.UseHttpsRedirection();

// CORS debe ir antes de Routing
app.UseCors("AllowSignalR");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();