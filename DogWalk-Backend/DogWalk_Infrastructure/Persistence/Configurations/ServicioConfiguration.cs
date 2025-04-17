using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            builder.ToTable("Servicios");
            
            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(s => s.Descripcion)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(s => s.Tipo)
                .IsRequired()
                .HasConversion<string>();
                
            // Relaciones
            builder.HasMany(s => s.Precios)
                   .WithOne(p => p.Servicio)
                   .HasForeignKey(p => p.ServicioId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasMany(s => s.Reservas)
                   .WithOne(r => r.Servicio)
                   .HasForeignKey(r => r.ServicioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}