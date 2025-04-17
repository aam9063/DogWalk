using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ChatMensajeConfiguration : IEntityTypeConfiguration<ChatMensaje>
    {
        public void Configure(EntityTypeBuilder<ChatMensaje> builder)
        {
            builder.ToTable("ChatMensajes");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Mensaje)
                .IsRequired()
                .HasColumnType("TEXT");
                
            builder.Property(c => c.FechaHora)
                .IsRequired();
                
            builder.Property(c => c.LeidoPorUsuario)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(c => c.LeidoPorPaseador)
                .IsRequired()
                .HasDefaultValue(false);
                
            // Relaciones
            builder.HasOne(c => c.Usuario)
                   .WithMany(u => u.MensajesEnviados)
                   .HasForeignKey(c => c.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasOne(c => c.Paseador)
                   .WithMany(p => p.MensajesRecibidos)
                   .HasForeignKey(c => c.PaseadorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
