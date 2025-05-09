using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class DetalleFacturaConfiguration : IEntityTypeConfiguration<DetalleFactura>
    {
        public void Configure(EntityTypeBuilder<DetalleFactura> builder)
        {
            builder.ToTable("DetallesFactura");
            
            builder.HasKey(d => d.Id);
            
            builder.Property(d => d.Cantidad)
                .IsRequired();
                
            // Value Objects
            builder.OwnsOne(d => d.PrecioUnitario, p =>
            {
                p.Property(x => x.Cantidad)
                   .HasColumnName("PrecioUnitario")
                   .IsRequired()
                   .HasPrecision(10, 2);
                   
                p.Property(x => x.Moneda)
                   .HasColumnName("Moneda")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
            
            // Relaciones
            builder.HasOne(d => d.Factura)
                   .WithMany(f => f.Detalles)
                   .HasForeignKey(d => d.FacturaId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasOne(d => d.Articulo)
                   .WithMany()
                   .HasForeignKey(d => d.ArticuloId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}