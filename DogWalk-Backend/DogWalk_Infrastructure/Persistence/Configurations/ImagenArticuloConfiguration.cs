using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class ImagenArticuloConfiguration : IEntityTypeConfiguration<ImagenArticulo>
    {
        public void Configure(EntityTypeBuilder<ImagenArticulo> builder)
        {
            builder.ToTable("ImagenesArticulos");
            
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.UrlImagen)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(i => i.EsPrincipal)
                .IsRequired()
                .HasDefaultValue(false);
        }
    }
}