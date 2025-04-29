// DogWalk_Infrastructure/DependencyInjection.cs (actualizado)
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Authentication;
using DogWalk_Infrastructure.Persistence;
using DogWalk_Infrastructure.Persistence.Context;
using DogWalk_Infrastructure.Persistence.Repositories;
using DogWalk_Infrastructure.Services.Email;
using DogWalk_Infrastructure.Services.FileStorage;
using DogWalk_Infrastructure.Services.Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace DogWalk_Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool configureAuthentication = true)
        {
            // DbContext
            services.AddDbContext<DogWalkDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(DogWalkDbContext).Assembly.FullName)));
            
            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IPaseadorRepository, PaseadorRepository>();
            services.AddScoped<IPerroRepository, PerroRepository>();
            services.AddScoped<IArticuloRepository, ArticuloRepository>();
            services.AddScoped<IServicioRepository, ServicioRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<IChatMensajeRepository, ChatMensajeRepository>();
            services.AddScoped<IDisponibilidadHorariaRepository, DisponibilidadHorariaRepository>();
            services.AddScoped<IOpinionPerroRepository, OpinionPerroRepository>();
            services.AddScoped<IRankingPaseadorRepository, RankingPaseadorRepository>();
            
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // Autenticación y sesión de usuario
            if (configureAuthentication)
            {
                services.Configure<AuthOptions>(configuration.GetSection("JwtSettings"));
                services.AddScoped<JwtProvider>();
                services.AddScoped<UserSession>();
                services.AddHttpContextAccessor();
                
                // JWT Authentication
                var jwtOptions = configuration.GetSection("JwtSettings").Get<AuthOptions>();
                if (jwtOptions != null)
                {
                    var key = Encoding.ASCII.GetBytes(jwtOptions.Key ?? jwtOptions.Key);
                    
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = true,
                            ValidIssuer = jwtOptions.Issuer,
                            ValidateAudience = true,
                            ValidAudience = jwtOptions.Audience,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            NameClaimType = ClaimTypes.NameIdentifier
                        };
                    });
                }
            }
            
            // Servicio de Stripe
            services.Configure<StripeOptions>(configuration.GetSection("Stripe"));
            services.AddScoped<StripeService>();
            
            // Servicio de almacenamiento de archivos
            services.Configure<FileStorageOptions>(configuration.GetSection("FileStorage"));
            services.AddScoped<FileStorageService>();
            
            // Servicio de Email
            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services.AddScoped<EmailService>();
            
            return services;
        }
    }
}