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
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

// Configuración de la autenticación JWT
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
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
    };
});

builder.Services.AddAuthorization();

// Registra las implementaciones de los repositorios y servicios
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registra los repositorios individuales si es necesario
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
// Ajusta el namespace para que apunte al proyecto donde están tus comandos/queries
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(DogWalk_Application.Features.Admin.Commands.CreateAdminCommand).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddSingleton<JwtProvider>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DogWalk API v1"));
    
  
}

// Inicializar datos (esto debe estar DESPUÉS de builder.Build())
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
   
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
