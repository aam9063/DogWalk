using System;
using DogWalk_Domain.Common.Enums;

namespace DogWalk_Domain.Entities;

public class Role : EntityBase
    {
        public RolUsuario TipoRol { get; private set; }
        public string Nombre { get; private set; }
        
        public ICollection<Usuario> Usuarios { get; private set; } = new List<Usuario>();
        public ICollection<Paseador> Paseadores { get; private set; } = new List<Paseador>();
        
        private Role() : base() { } 
        
        public Role(RolUsuario tipoRol, string nombre) : base()
        {
            TipoRol = tipoRol;
            Nombre = nombre;
        }
        
        public Role(Guid id, RolUsuario tipoRol, string nombre) : base(id)
        {
            TipoRol = tipoRol;
            Nombre = nombre;
        }
    }