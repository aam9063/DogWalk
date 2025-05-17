using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DogWalk_Infrastructure.Persistence.Context
{
    public class DogWalkDbContext : DbContext
    {
        public DogWalkDbContext(DbContextOptions<DogWalkDbContext> options) : base(options) { }
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Paseador> Paseadores { get; set; }
        public DbSet<Perro> Perros { get; set; }
        public DbSet<FotoPerro> FotosPerros { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Precio> Precios { get; set; }
        public DbSet<DisponibilidadHoraria> DisponibilidadHoraria { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<ImagenArticulo> ImagenesArticulos { get; set; }
        public DbSet<ItemCarrito> ItemsCarrito { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<ChatMensaje> ChatMensajes { get; set; }
        public DbSet<RankingPaseador> RankingPaseadores { get; set; }
        public DbSet<OpinionPerro> OpinionesPerros { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Aplicar todas las configuraciones de entidades en el assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Configuraciones directas si son necesarias
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Paseador>().ToTable("Paseadores");
            modelBuilder.Entity<Perro>().ToTable("Perros");
            modelBuilder.Entity<FotoPerro>().ToTable("FotosPerros");
            modelBuilder.Entity<Servicio>().ToTable("Servicios");
            modelBuilder.Entity<Precio>().ToTable("Precios");
            modelBuilder.Entity<DisponibilidadHoraria>().ToTable("DisponibilidadHoraria");
            modelBuilder.Entity<Reserva>().ToTable("Reservas");
            modelBuilder.Entity<Articulo>().ToTable("Articulos");
            modelBuilder.Entity<ImagenArticulo>().ToTable("ImagenesArticulos");
            modelBuilder.Entity<ItemCarrito>().ToTable("ItemsCarrito");
            modelBuilder.Entity<Factura>().ToTable("Facturas");
            modelBuilder.Entity<DetalleFactura>().ToTable("DetallesFactura");
            modelBuilder.Entity<ChatMensaje>().ToTable("ChatMensajes");
            modelBuilder.Entity<RankingPaseador>().ToTable("RankingPaseadores");
            modelBuilder.Entity<OpinionPerro>().ToTable("OpinionesPerros");
            modelBuilder.Entity<Role>().ToTable("Roles");

            // Configuración para RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(rt => rt.Id);

                entity.Property(rt => rt.Token).IsRequired();
                entity.Property(rt => rt.JwtId).IsRequired();

                entity.HasOne(rt => rt.Usuario)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Datos semilla
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role(Guid.Parse("8f7779b5-e30e-4e38-bdf4-79c533696187"), RolUsuario.Administrador, "Administrador"),
                new Role(Guid.Parse("c2fbf3a7-adfa-4ac4-b384-14874661c995"), RolUsuario.Usuario, "Usuario"),
                new Role(Guid.Parse("95b2b3ff-f0c1-4819-a842-0d0b6e111c0d"), RolUsuario.Paseador, "Paseador")
            );
            
            // Servicios
            modelBuilder.Entity<Servicio>().HasData(
                new Servicio(
                    Guid.Parse("dbc1c3f6-6230-46c9-a344-7d5d647738be"),
                    "Paseo estándar",
                    "Paseo de 30 minutos con un paseador profesional",
                    TipoServicio.Paseo
                ),
                new Servicio(
                    Guid.Parse("7a5d6a55-43d0-4825-b5c0-7ce22ebd142c"),
                    "Paseo premium",
                    "Paseo de 60 minutos con un paseador profesional",
                    TipoServicio.Paseo
                ),
                new Servicio(
                    Guid.Parse("e0da5de2-b1e3-4c4d-b03d-b35c7f12c5d7"),
                    "Guardería diurna",
                    "Cuidado durante el día en casa del paseador (8 horas)",
                    TipoServicio.GuarderiaDia
                ),
                new Servicio(
                    Guid.Parse("d21b1406-2dce-4cd9-9b0f-e1c366ac6c4c"),
                    "Guardería nocturna",
                    "Cuidado durante la noche en casa del paseador (12 horas)",
                    TipoServicio.GuarderiaNoche
                )
            );
        }
    }
}