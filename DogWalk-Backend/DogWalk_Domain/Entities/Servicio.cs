using DogWalk_Domain.Common.Enums;
using System.Collections.Generic;


namespace DogWalk_Domain.Entities;

public class Servicio : EntityBase
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public TipoServicio Tipo { get; private set; }
        
        private readonly List<Precio> _precios = new();
        public IReadOnlyCollection<Precio> Precios => _precios.AsReadOnly();
        
        private readonly List<Reserva> _reservas = new();
        public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();
        
        private Servicio() : base() { } 
        
        public Servicio(
            Guid id,
            string nombre,
            string descripcion,
            TipoServicio tipo
        ) : base(id)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Tipo = tipo;
        }
        
        public void ActualizarDatos(string nombre, string descripcion, TipoServicio tipo)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Tipo = tipo;
            ActualizarFechaModificacion();
        }
    }
