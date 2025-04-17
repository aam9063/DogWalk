using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class PerroConfiguration : IEntityTypeConfiguration<Perro>
    {
        public void Configure(EntityTypeBuilder<Perro> builder)
        {
            builder.ToTable("Perros");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Raza)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Edad)
                .IsRequired();
                
            builder.Property(p => p.GpsUbicacion)
                .HasMaxLength(255);
                
            // Relaciones
            builder.HasMany(p => p.Fotos)
                   .WithOne(f => f.Perro)
                   .HasForeignKey(f => f.PerroId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasMany(p => p.Opiniones)
                   .WithOne(o => o.Perro)
                   .HasForeignKey(o => o.PerroId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasMany(p => p.Reservas)
                   .WithOne(r => r.Perro)
                   .HasForeignKey(r => r.PerroId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}