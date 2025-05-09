using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ItemCarritoConfiguration : IEntityTypeConfiguration<ItemCarrito>
    {
        public void Configure(EntityTypeBuilder<ItemCarrito> builder)
        {
            builder.ToTable("ItemsCarrito");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.ArticuloId)
                .IsRequired();
                
            builder.Property(i => i.Cantidad)
                .IsRequired();
                
            // Value Objects
            builder.OwnsOne(i => i.PrecioUnitario, p =>
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
            builder.HasOne(i => i.Usuario)
                   .WithMany(u => u.Carrito)
                   .HasForeignKey(i => i.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Articulo)
                   .WithMany()
                   .HasForeignKey(i => i.ArticuloId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}