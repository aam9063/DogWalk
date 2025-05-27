using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DogWalk_Domain.Interfaces.IRepositories;

/// <summary>
/// Interfaz para el repositorio de usuarios.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Obtiene un usuario por su email.
    /// </summary>
    Task<Usuario> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene un usuario por su rol.
    /// </summary>
    Task<IEnumerable<Usuario>> GetByRolAsync(RolUsuario rol);

    /// <summary>
    /// Verifica si existe un usuario con un email.
    /// </summary>  
    Task<bool> ExisteEmailAsync(string email);

    /// <summary>
    /// Verifica si existe un usuario con un DNI.
    /// </summary>
    Task<bool> ExisteDniAsync(string dni);

    /// <summary>
    /// Crea un usuario administrador.
    /// </summary>
    Task<Guid> CreateAdminUserAsync(string email, string nombre, string apellido, 
                                  string telefono, string password);

    /// <summary>
    /// Obtiene un usuario por una expresión.
    /// </summary>
    Task<IEnumerable<Usuario>> GetAsync(Expression<Func<Usuario, bool>> predicate);

    /// <summary>
    /// Actualiza la contraseña de un usuario.
    /// </summary>
    Task UpdatePasswordAsync(Guid userId, string newPassword);

    /// <summary>
    /// Obtiene un usuario por su ID con su carrito.
    /// </summary>
    Task<Usuario> GetByIdWithCarritoAsync(Guid id);
}
