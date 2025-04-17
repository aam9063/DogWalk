using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class PaseadorConfiguration : IEntityTypeConfiguration<Paseador>
    {
        public void Configure(EntityTypeBuilder<Paseador> builder)
        {
            builder.ToTable("Paseadores");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Apellido)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.ValoracionGeneral)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.00m);
                
            // Value Objects
            builder.OwnsOne(p => p.Dni, dni =>
            {
                dni.Property(d => d.Valor)
                   .HasColumnName("Dni")
                   .IsRequired()
                   .HasMaxLength(9);
                   
                dni.HasIndex(d => d.Valor)
                   .IsUnique();
            });
            
            builder.OwnsOne(p => p.Email, email =>
            {
                email.Property(e => e.Valor)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                email.HasIndex(e => e.Valor)
                    .IsUnique();
            });
            
            builder.OwnsOne(p => p.Password, pwd =>
            {
                pwd.Property(x => x.Hash)
                   .HasColumnName("PasswordHash")
                   .IsRequired();
                   
                pwd.Property(x => x.Salt)
                   .HasColumnName("PasswordSalt")
                   .IsRequired();
            });
            
            builder.OwnsOne(p => p.Telefono, tel =>
            {
                tel.Property(t => t.Numero)
                   .HasColumnName("Telefono")
                   .HasMaxLength(9)
                   .IsRequired(false);
                   
                tel.Property<bool>("HasTelefono")
                   .HasDefaultValue(false);
            });
            
            builder.OwnsOne(p => p.Direccion, dir =>
            {
                dir.Property(d => d.TextoCompleto)
                   .HasColumnName("Direccion")
                   .IsRequired()
                   .HasMaxLength(255);
                   
                dir.Property(d => d.Calle)
                   .HasColumnName("Calle")
                   .HasMaxLength(100);
                   
                dir.Property(d => d.Ciudad)
                   .HasColumnName("Ciudad")
                   .HasMaxLength(50);
                   
                dir.Property(d => d.CodigoPostal)
                   .HasColumnName("CodigoPostal")
                   .HasMaxLength(5);
            });
            
            builder.OwnsOne(p => p.Ubicacion, ubi =>
            {
                ubi.Property(u => u.Latitud)
                   .HasColumnName("Latitud")
                   .IsRequired();
                   
                ubi.Property(u => u.Longitud)
                   .HasColumnName("Longitud")
                   .IsRequired();
            });
            
            // Relaciones
            builder.Property(p => p.Rol)
                   .HasConversion<int>()
                   .IsRequired();
            builder.HasMany(p => p.Precios)
                   .WithOne(pr => pr.Paseador)
                   .HasForeignKey(pr => pr.PaseadorId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasMany(p => p.Reservas)
                   .WithOne(r => r.Paseador)
                   .HasForeignKey(r => r.PaseadorId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasMany(p => p.Disponibilidad)
                   .WithOne(d => d.Paseador)
                   .HasForeignKey(d => d.PaseadorId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}