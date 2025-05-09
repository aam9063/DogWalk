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
            var usuarios = await _context.Usuarios.ToListAsync();
            return usuarios.FirstOrDefault(u => u.Email.ToString() == email);
        }

        public async Task<IEnumerable<Usuario>> GetByRolAsync(RolUsuario rol)
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.Rol == rol)
                .ToListAsync();
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(u => u.Email.ToString() == email);
        }

        public async Task<bool> ExisteDniAsync(string dni)
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(u => u.Dni.ToString() == dni);
        }

        public async Task<Usuario> GetFirstOrDefaultAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Usuario>> GetAsync(Expression<Func<Usuario, bool>> predicate)
        {
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Guid> CreateAdminUserAsync(string email, string nombre, string apellido,
                                                  string telefono, string password)
        {
            // Crear el objeto Password usando la lógica de dominio
            var passwordObj = DogWalk_Domain.Common.ValueObjects.Password.Create(password);
            
            var adminId = Guid.NewGuid();
            DateTime ahora = DateTime.UtcNow;
            
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
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordHash", passwordObj.Hash),
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordSalt", passwordObj.Salt),
                new Microsoft.Data.SqlClient.SqlParameter("@Telefono", telefono),
                new Microsoft.Data.SqlClient.SqlParameter("@Rol", (int)DogWalk_Domain.Common.Enums.RolUsuario.Administrador),
                new Microsoft.Data.SqlClient.SqlParameter("@CreadoEn", ahora),
                new Microsoft.Data.SqlClient.SqlParameter("@ModificadoEn", ahora)
            };
            
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            
            return adminId;
        }

        public async Task UpdatePasswordAsync(Guid userId, string newPassword)
        {
            // Crear un nuevo objeto Password con el método del dominio
            var passwordObj = DogWalk_Domain.Common.ValueObjects.Password.Create(newPassword);
            
            // Actualizar directamente en la base de datos
            string sql = @"
                UPDATE Usuarios
                SET PasswordHash = @PasswordHash, 
                    PasswordSalt = @PasswordSalt,
                    ModificadoEn = @ModificadoEn
                WHERE Id = @UserId";
            
            var parameters = new[]
            {
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordHash", passwordObj.Hash),
                new Microsoft.Data.SqlClient.SqlParameter("@PasswordSalt", passwordObj.Salt),
                new Microsoft.Data.SqlClient.SqlParameter("@ModificadoEn", DateTime.UtcNow),
                new Microsoft.Data.SqlClient.SqlParameter("@UserId", userId)
            };
            
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task<Usuario> GetByIdWithCarritoAsync(Guid id)
        {
            return await _context.Usuarios
                .Include(u => u.Carrito)
                    .ThenInclude(c => c.Articulo)
                        .ThenInclude(a => a.Imagenes)
                .AsTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
