using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(DogWalkDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            // Traer todos los usuarios (no eficiente pero funcionará para testing)
            var usuarios = await _dbContext.Usuarios.ToListAsync();
            return usuarios.FirstOrDefault(u => u.Email.ToString() == email);
        }

        public async Task<IEnumerable<Usuario>> GetByRolAsync(RolUsuario rol)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .Where(u => u.Rol == rol)
                .ToListAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Email.ToString() == email);
        }

        public async Task<bool> ExisteDniAsync(string dni)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Dni.ToString() == dni);
        }

        public async Task<Usuario> GetFirstOrDefaultAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Usuario>> GetAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _dbContext.Usuarios
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Guid> CreateAdminUserAsync(string email, string nombre, string apellido,
                                                  string telefono, string password)
        {
            // Hashear la contraseña
            byte[] passwordSalt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(passwordSalt);
            }
            
            // Crear el hash usando el salt
            byte[] passwordHash = GeneratePasswordHash(password, passwordSalt);
            
            var adminId = Guid.NewGuid();
            DateTime ahora = DateTime.UtcNow;
            
            // Asumiendo que el rol Admin tiene el ID 1 en tu tabla de roles
            // Ajusta este valor según el ID real en tu base de datos
            int rolAdminId = 1; 
            
            var sql = @"
                INSERT INTO Usuarios (
                    Id, Nombre, Apellido, Email, PasswordHash, PasswordSalt, 
                    Telefono, Rol, CreadoEn, ModificadoEn
                )
                VALUES (
                    @Id, @Nombre, @Apellido, @Email, @PasswordHash, @PasswordSalt, 
                    @Telefono, @Rol, @CreadoEn, @ModificadoEn
                )";
            
            var parameters = new[] {
                new Microsoft.Data.SqlClient.SqlParameter("@Id", adminId),
                new Microsoft.Data.SqlClient.SqlParameter("@Nombre", nombre),
                new Microsoft.Data.SqlClient.SqlParameter("@Apellido", apellido),
                new Microsoft.Data.SqlClient.SqlParameter("@Email", email),
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordHash", passwordHash),
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordSalt", passwordSalt),
                new Microsoft.Data.SqlClient.SqlParameter("@Telefono", telefono),
                new Microsoft.Data.SqlClient.SqlParameter("@Rol", rolAdminId),
                new Microsoft.Data.SqlClient.SqlParameter("@CreadoEn", ahora),
                new Microsoft.Data.SqlClient.SqlParameter("@ModificadoEn", ahora)
            };
            
            await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
            
            return adminId;
        }

        // Método auxiliar para generar el hash de la contraseña
        private byte[] GeneratePasswordHash(string password, byte[] salt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(salt))
            {
                return hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
