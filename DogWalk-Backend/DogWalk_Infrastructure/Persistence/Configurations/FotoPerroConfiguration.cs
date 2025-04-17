using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class FotoPerroConfiguration : IEntityTypeConfiguration<FotoPerro>
    {
        public void Configure(EntityTypeBuilder<FotoPerro> builder)
        {
            builder.ToTable("FotosPerros");
            
            builder.HasKey(f => f.Id);
            
            builder.Property(f => f.UrlFoto)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(f => f.Descripcion)
                .HasMaxLength(255);
        }
    }
}