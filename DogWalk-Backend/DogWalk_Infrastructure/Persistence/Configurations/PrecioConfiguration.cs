using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class PrecioConfiguration : IEntityTypeConfiguration<Precio>
    {
        public void Configure(EntityTypeBuilder<Precio> builder)
        {
            builder.ToTable("Precios");
            
            builder.HasKey(p => p.Id);
            
            // Value Objects
            builder.OwnsOne(p => p.Valor, v =>
            {
                v.Property(x => x.Cantidad)
                   .HasColumnName("Precio")
                   .IsRequired()
                   .HasPrecision(10, 2);
                   
                v.Property(x => x.Moneda)
                   .HasColumnName("Moneda")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
            
            // Relaciones - ya configuradas en PaseadorConfiguration y ServicioConfiguration
        }
    }
}