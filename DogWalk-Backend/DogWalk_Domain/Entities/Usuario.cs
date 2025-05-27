using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System.Collections.Generic;

namespace DogWalk_Domain.Entities;

/// <summary>
/// Representa un usuario en la aplicación.
/// </summary>
public class Usuario : EntityBase
    {
        /// <summary>
        /// Obtiene o establece el DNI del usuario.
        /// </summary>
        public Dni Dni { get; private set; }

        /// <summary>
        /// Obtiene o establece el nombre del usuario.  
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Obtiene o establece el apellido del usuario.
        /// </summary>
        public string Apellido { get; private set; }

        /// <summary>   
        /// Obtiene o establece la dirección del usuario.
        /// </summary>
        public Direccion Direccion { get; private set; }

        /// <summary>
        /// Obtiene o establece el email del usuario.
        /// </summary>
        public Email Email { get; private set; }

        /// <summary>
        /// Obtiene o establece la contraseña del usuario.
        /// </summary>  
        public Password Password { get; private set; }

        /// <summary>
        /// Obtiene o establece el teléfono del usuario.
        /// </summary>
        public Telefono Telefono { get; private set; }

        /// <summary>
        /// Obtiene o establece la foto de perfil del usuario.
        /// </summary>
        public string FotoPerfil { get; private set; }

        /// <summary>
        /// Obtiene o establece el rol del usuario.
        /// </summary>
        public RolUsuario Rol { get; private set; }
        
        
        /// <summary>
        /// Lista de perros del usuario.
        /// </summary>
        private readonly List<Perro> _perros = new();

        /// <summary>
        /// Obtiene la lista de perros del usuario.
        /// </summary>
        public IReadOnlyCollection<Perro> Perros => _perros.AsReadOnly();
        
        /// <summary>
        /// Lista de reservas del usuario.
        /// </summary>
        private readonly List<Reserva> _reservas = new();

        /// <summary>
        /// Obtiene la lista de reservas del usuario.
        /// </summary>
        public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();
        
        /// <summary>
        /// Lista de facturas del usuario.
        /// </summary>
        private readonly List<Factura> _facturas = new();

        /// <summary>
        /// Obtiene la lista de facturas del usuario.
        /// </summary>
        public IReadOnlyCollection<Factura> Facturas => _facturas.AsReadOnly();
        
        /// <summary>
        /// Lista de artículos en el carrito del usuario.
        /// </summary>
        private readonly List<ItemCarrito> _carrito = new();

        /// <summary>
        /// Obtiene la lista de artículos en el carrito del usuario.
        /// </summary>
        public IReadOnlyCollection<ItemCarrito> Carrito => _carrito.AsReadOnly();
        
        /// <summary>
        /// Lista de valoraciones dadas por el usuario.
        /// </summary>
        private readonly List<RankingPaseador> _valoracionesDadas = new();

        /// <summary>
        /// Obtiene la lista de valoraciones dadas por el usuario.
        /// </summary>
        public IReadOnlyCollection<RankingPaseador> ValoracionesDadas => _valoracionesDadas.AsReadOnly();
        
        /// <summary>
        /// Lista de mensajes enviados por el usuario.
        /// </summary>
        private readonly List<ChatMensaje> _mensajesEnviados = new();

        /// <summary>
        /// Obtiene la lista de mensajes enviados por el usuario.
        /// </summary>
        public IReadOnlyCollection<ChatMensaje> MensajesEnviados => _mensajesEnviados.AsReadOnly();
        
        /// <summary>
        /// Constructor de la clase Usuario.
        /// </summary>
        private Usuario() : base() { } 
        
        /// <summary>
        /// Constructor de la clase Usuario.
        /// </summary>
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

        /// <summary>
        /// Actualiza la foto de perfil del usuario.
        /// </summary>
        public void ActualizarFotoPerfil(string urlFoto)
        {
            FotoPerfil = urlFoto;
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Actualiza los datos del usuario.
        /// </summary>
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

        /// <summary>
        /// Cambia la contraseña del usuario.
        /// </summary>
        public void CambiarPassword(string passwordActual, string nuevaPassword)
        {
            if (!Password.Verify(passwordActual))
                throw new InvalidOperationException("La contraseña actual es incorrecta");
                
            Password = Password.Create(nuevaPassword);
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Agrega un perro al usuario.
        /// </summary>
        public void AgregarPerro(Perro perro)
        {
            _perros.Add(perro);
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Agrega un artículo al carrito del usuario.
        /// </summary>
        public void AgregarItemCarrito(ItemCarrito item)
        {
            var itemExistente = _carrito.FirstOrDefault(i => i.ArticuloId == item.ArticuloId);
            
            if (itemExistente != null)
            {
                itemExistente.ActualizarCantidad(itemExistente.Cantidad + item.Cantidad);
            }
            else
            {
                _carrito.Add(item);
            }
            
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Obtiene un artículo del carrito del usuario.
        /// </summary>
        public ItemCarrito ObtenerItemCarrito(Guid itemId)
        {
            return _carrito.FirstOrDefault(i => i.Id == itemId);
        }

        /// <summary>
        /// Actualiza la cantidad de un artículo en el carrito del usuario.
        /// </summary>
        public void ActualizarCantidadItemCarrito(Guid itemId, int cantidad)
        {
            var item = ObtenerItemCarrito(itemId);
            if (item == null)
                throw new InvalidOperationException("El ítem no existe en el carrito");

            item.ActualizarCantidad(cantidad);
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Elimina un artículo del carrito del usuario.
        /// </summary>
        public void EliminarItemCarrito(Guid itemId)
        {
            var item = ObtenerItemCarrito(itemId);
            if (item != null)
            {
                _carrito.Remove(item);
                ActualizarFechaModificacion();
            }
        }

        /// <summary>
        /// Vacia el carrito del usuario.
        /// </summary>
        public void VaciarCarrito()
        {
            _carrito.Clear();
            ActualizarFechaModificacion();
        }
    }
