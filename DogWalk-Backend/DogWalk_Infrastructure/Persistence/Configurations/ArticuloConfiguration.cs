using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ArticuloConfiguration : IEntityTypeConfiguration<Articulo>
    {
        public void Configure(EntityTypeBuilder<Articulo> builder)
        {
            builder.ToTable("Articulos");
            
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.Nombre)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(a => a.Descripcion)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(a => a.Stock)
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(a => a.Categoria)
                .IsRequired()
                .HasConversion<string>();
                
            // Value Objects
            builder.OwnsOne(a => a.Precio, p =>
            {
                p.Property(d => d.Cantidad)
                   .HasColumnName("Precio")
                   .IsRequired()
                   .HasPrecision(10, 2);
                   
                p.Property(d => d.Moneda)
                   .HasColumnName("Moneda")
                   .IsRequired()
                   .HasMaxLength(3)
                   .HasDefaultValue("EUR");
            });
            
            // Relaciones
            builder.HasMany(a => a.Imagenes)
                   .WithOne(i => i.Articulo)
                   .HasForeignKey(i => i.ArticuloId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relación con ItemCarrito
            builder.HasMany<ItemCarrito>()
                   .WithOne()
                   .HasForeignKey(i => i.ItemId)
                   .HasPrincipalKey(a => a.Id)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired()
                   .HasConstraintName("FK_ItemCarrito_Articulo");

            // Relación con DetalleFactura
            builder.HasMany<DetalleFactura>()
                   .WithOne()
                   .HasForeignKey(d => d.ItemId)
                   .HasPrincipalKey(a => a.Id)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired()
                   .HasConstraintName("FK_DetalleFactura_Articulo");
        }
    }
}