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
            
            builder.Property(i => i.TipoItem)
                .IsRequired()
                .HasConversion<string>();
                
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

            builder
                .HasOne<Articulo>()
                .WithMany()
                .HasForeignKey(i => i.ItemId)
                .HasPrincipalKey(a => a.Id)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false)
                .HasConstraintName("FK_ItemCarrito_Articulo");

            builder
                .HasOne<Servicio>()
                .WithMany()
                .HasForeignKey(i => i.ItemId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false)
                .HasConstraintName("FK_ItemCarrito_Servicio");
        }
    }
}