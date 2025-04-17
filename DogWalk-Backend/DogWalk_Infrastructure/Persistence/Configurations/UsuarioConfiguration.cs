using DogWalk_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogWalk_Infrastructure.Persistence.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(u => u.Apellido)
                .IsRequired()
                .HasMaxLength(50);
                
            // Value Objects
            builder.Property(u => u.Rol)
                .HasConversion<int>()
                .IsRequired();
            builder.OwnsOne(u => u.Dni, dni =>
            {
                dni.Property(d => d.Valor)
                   .HasColumnName("Dni")
                   .IsRequired()
                   .HasMaxLength(9);
                   
                dni.HasIndex(d => d.Valor)
                   .IsUnique();
            });
            
            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Valor)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                email.HasIndex(e => e.Valor)
                    .IsUnique();
            });
            
            builder.OwnsOne(u => u.Password, pwd =>
            {
                pwd.Property(p => p.Hash)
                   .HasColumnName("PasswordHash")
                   .IsRequired();
                   
                pwd.Property(p => p.Salt)
                   .HasColumnName("PasswordSalt")
                   .IsRequired();
            });
            
            builder.OwnsOne(u => u.Telefono, tel =>
            {
                tel.Property(t => t.Numero)
                   .HasColumnName("Telefono")
                   .IsRequired()
                   .HasMaxLength(9);
            });
            
            builder.OwnsOne(u => u.Direccion, dir =>
            {
                dir.Property(d => d.TextoCompleto)
                   .HasColumnName("Direccion")
                   .IsRequired()
                   .HasMaxLength(255);
                   
                dir.Property(d => d.Calle)
                   .HasColumnName("Calle")
                   .HasMaxLength(100);
                   
                dir.Property(d => d.Ciudad)
                   .HasColumnName("Ciudad")
                   .HasMaxLength(50);
                   
                dir.Property(d => d.CodigoPostal)
                   .HasColumnName("CodigoPostal")
                   .HasMaxLength(5);
            });
            
            // Relaciones
            builder.HasMany(u => u.Perros)
                   .WithOne(p => p.Usuario)
                   .HasForeignKey(p => p.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);
                   
            builder.HasMany(u => u.Reservas)
                   .WithOne(r => r.Usuario)
                   .HasForeignKey(r => r.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
                   
            builder.HasMany(u => u.Facturas)
                   .WithOne(f => f.Usuario)
                   .HasForeignKey(f => f.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}