using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(r => r.TipoRol)
                .IsRequired()
                .HasConversion<int>();
                
            builder.HasIndex(r => r.Nombre)
                .IsUnique();
        }
    }
}
