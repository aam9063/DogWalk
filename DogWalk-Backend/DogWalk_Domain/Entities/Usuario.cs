using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System.Collections.Generic;

namespace DogWalk_Domain.Entities;

 public class Usuario : EntityBase
    {
        public Dni Dni { get; private set; }
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public Direccion Direccion { get; private set; }
        public Email Email { get; private set; }
        public Password Password { get; private set; }
        public Telefono Telefono { get; private set; }
        public string FotoPerfil { get; private set; }
        public RolUsuario Rol { get; private set; }
        
        // Relaciones
        private readonly List<Perro> _perros = new();
        public IReadOnlyCollection<Perro> Perros => _perros.AsReadOnly();
        
        private readonly List<Reserva> _reservas = new();
        public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();
        
        private readonly List<Factura> _facturas = new();
        public IReadOnlyCollection<Factura> Facturas => _facturas.AsReadOnly();
        
        private readonly List<ItemCarrito> _carrito = new();
        public IReadOnlyCollection<ItemCarrito> Carrito => _carrito.AsReadOnly();
        
        private readonly List<RankingPaseador> _valoracionesDadas = new();
        public IReadOnlyCollection<RankingPaseador> ValoracionesDadas => _valoracionesDadas.AsReadOnly();
        
        private readonly List<ChatMensaje> _mensajesEnviados = new();
        public IReadOnlyCollection<ChatMensaje> MensajesEnviados => _mensajesEnviados.AsReadOnly();
        
        private Usuario() : base() { } // Para EF Core
        
        public Usuario(
            Guid id,
            Dni dni,
            string nombre,
            string apellido,
            Direccion direccion,
            Email email,
            Password password,
            Telefono telefono,
            RolUsuario rol = RolUsuario.Usuario
        ) : base(id)
        {
            Dni = dni;
            Nombre = nombre;
            Apellido = apellido;
            Direccion = direccion;
            Email = email;
            Password = password;
            Telefono = telefono;
            Rol = rol;
        }
        
        public void ActualizarFotoPerfil(string urlFoto)
        {
            FotoPerfil = urlFoto;
            ActualizarFechaModificacion();
        }
        
        public void ActualizarDatos(
            string nombre,
            string apellido,
            Direccion direccion,
            Telefono telefono
        )
        {
            Nombre = nombre;
            Apellido = apellido;
            Direccion = direccion;
            Telefono = telefono;
            ActualizarFechaModificacion();
        }
        
        public void CambiarPassword(string passwordActual, string nuevaPassword)
        {
            if (!Password.Verify(passwordActual))
                throw new InvalidOperationException("La contraseña actual es incorrecta");
                
            Password = Password.Create(nuevaPassword);
            ActualizarFechaModificacion();
        }
        
        public void AgregarPerro(Perro perro)
        {
            _perros.Add(perro);
            ActualizarFechaModificacion();
        }
        
        public void AgregarItemCarrito(ItemCarrito item)
        {
            var existente = _carrito.FirstOrDefault(x => 
                x.ItemId == item.ItemId && x.TipoItem == item.TipoItem);
            
            if (existente != null)
            {
                existente.ActualizarCantidad(existente.Cantidad + item.Cantidad);
            }
            else
            {
                _carrito.Add(item);
            }
            ActualizarFechaModificacion();
        }
        
        public void ActualizarCantidadItemCarrito(Guid itemCarritoId, int nuevaCantidad)
        {
            var item = _carrito.FirstOrDefault(x => x.Id == itemCarritoId);
            if (item == null)
                throw new InvalidOperationException("El ítem no existe en el carrito");
            
            item.ActualizarCantidad(nuevaCantidad);
            ActualizarFechaModificacion();
        }
        
        public void EliminarItemCarrito(Guid itemCarritoId)
        {
            var item = _carrito.FirstOrDefault(x => x.Id == itemCarritoId);
            if (item != null)
            {
                _carrito.Remove(item);
                ActualizarFechaModificacion();
            }
        }
        
        public ItemCarrito ObtenerItemCarrito(Guid itemCarritoId)
        {
            return _carrito.FirstOrDefault(x => x.Id == itemCarritoId);
        }
        
        public void VaciarCarrito()
        {
            _carrito.Clear();
            ActualizarFechaModificacion();
        }
    }
