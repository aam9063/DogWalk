using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
    {
        public void Configure(EntityTypeBuilder<Factura> builder)
        {
            builder.ToTable("Facturas");
            
            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.FechaFactura)
                .IsRequired();
                
            builder.Property(f => f.MetodoPago)
                .IsRequired()
                .HasConversion<string>();
                
            // Value Objects
            builder.OwnsOne(f => f.Total, t =>
            {
                t.Property(d => d.Cantidad)
                   .HasColumnName("Total")
                   .IsRequired()
                   .HasPrecision(10, 2);
                   
                t.Property(d => d.Moneda)
                   .HasColumnName("Moneda")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
            
            // Relaciones
            builder.HasMany(f => f.Detalles)
                   .WithOne(d => d.Factura)
                   .HasForeignKey(d => d.FacturaId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}