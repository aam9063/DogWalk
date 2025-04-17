using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class RankingPaseadorConfiguration : IEntityTypeConfiguration<RankingPaseador>
    {
        public void Configure(EntityTypeBuilder<RankingPaseador> builder)
        {
            builder.ToTable("RankingPaseadores");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Comentario)
                .HasMaxLength(255);
                
            // Value Objects
            builder.OwnsOne(r => r.Valoracion, v =>
            {
                v.Property(x => x.Puntuacion)
                   .HasColumnName("Valoracion")
                   .IsRequired();
            });
            
            // Relaciones
            builder.HasOne(r => r.Usuario)
                   .WithMany(u => u.ValoracionesDadas)
                   .HasForeignKey(r => r.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(r => r.Paseador)
                   .WithMany(p => p.ValoracionesRecibidas)
                   .HasForeignKey(r => r.PaseadorId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            // Cada usuario solo puede valorar una vez a cada paseador
            builder.HasIndex(r => new { r.UsuarioId, r.PaseadorId }).IsUnique();
        }
    }
}