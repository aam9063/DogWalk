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
            
            builder.Property(d => d.TipoItem)
                .IsRequired()
                .HasConversion<string>();
                
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
                   .HasColumnName("MonedaPrecio")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
            
            builder.OwnsOne(d => d.Subtotal, s =>
            {
                s.Property(x => x.Cantidad)
                   .HasColumnName("Subtotal")
                   .IsRequired()
                   .HasPrecision(10, 2);
                   
                s.Property(x => x.Moneda)
                   .HasColumnName("MonedaSubtotal")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
        }
    }
}