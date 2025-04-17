using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class OpinionPerroConfiguration : IEntityTypeConfiguration<OpinionPerro>
    {
        public void Configure(EntityTypeBuilder<OpinionPerro> builder)
        {
            builder.ToTable("OpinionesPerros");
            
            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.Comentario)
                .HasMaxLength(255);
                
            // Value Objects
            builder.OwnsOne(o => o.Valoracion, v =>
            {
                v.Property(x => x.Puntuacion)
                   .HasColumnName("Valoracion")
                   .IsRequired();
            });
            
            // Relaciones
            builder.HasOne(o => o.Perro)
                   .WithMany(p => p.Opiniones)
                   .HasForeignKey(o => o.PerroId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(o => o.Paseador)
                   .WithMany(p => p.OpinionesDadas)
                   .HasForeignKey(o => o.PaseadorId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            // Cada paseador solo puede valorar una vez a cada perro
            builder.HasIndex(o => new { o.PaseadorId, o.PerroId }).IsUnique();
        }
    }
}
