using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
    {
        public void Configure(EntityTypeBuilder<Reserva> builder)
        {
            builder.ToTable("Reservas");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.FechaReserva)
                .IsRequired();
                
            builder.Property(r => r.Estado)
                .IsRequired()
                .HasConversion<string>();
                
            // Value Objects
            builder.OwnsOne(r => r.Precio, p =>
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
        }
    }
}