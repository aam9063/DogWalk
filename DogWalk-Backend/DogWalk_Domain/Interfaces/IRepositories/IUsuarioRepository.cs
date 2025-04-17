using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> GetByEmailAsync(string email);
        Task<IEnumerable<Usuario>> GetByRolAsync(RolUsuario rol);
        Task<bool> ExisteEmailAsync(string email);
        Task<bool> ExisteDniAsync(string dni);
        Task<Guid> CreateAdminUserAsync(string email, string nombre, string apellido, 
                                      string telefono, string password);
        Task<Usuario> GetFirstOrDefaultAsync(Expression<Func<Usuario, bool>> predicate);
        Task<IEnumerable<Usuario>> GetAsync(Expression<Func<Usuario, bool>> predicate);
    }
