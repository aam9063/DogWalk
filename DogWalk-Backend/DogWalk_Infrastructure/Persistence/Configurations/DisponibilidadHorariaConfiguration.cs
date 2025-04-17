using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class DisponibilidadHorariaConfiguration : IEntityTypeConfiguration<DisponibilidadHoraria>
    {
        public void Configure(EntityTypeBuilder<DisponibilidadHoraria> builder)
        {
            builder.ToTable("DisponibilidadHoraria");
            
            builder.HasKey(d => d.Id);
            
            builder.Property(d => d.FechaHora)
                .IsRequired();
                
            builder.Property(d => d.Estado)
                .IsRequired()
                .HasConversion<string>();
        }
    }
}
