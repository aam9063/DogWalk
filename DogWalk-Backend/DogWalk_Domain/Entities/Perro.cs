using System.Collections.Generic;

namespace DogWalk_Domain.Entities;

 public class Perro : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public string Nombre { get; private set; }
        public string Raza { get; private set; }
        public int Edad { get; private set; }
        public string GpsUbicacion { get; private set; }
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        
        private readonly List<FotoPerro> _fotos = new();
        public IReadOnlyCollection<FotoPerro> Fotos => _fotos.AsReadOnly();
        
        private readonly List<OpinionPerro> _opiniones = new();
        public IReadOnlyCollection<OpinionPerro> Opiniones => _opiniones.AsReadOnly();
        
        private readonly List<Reserva> _reservas = new();
        public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();
        
        private Perro() : base() { } // Para EF Core
        
        public Perro(
            Guid id,
            Guid usuarioId,
            string nombre,
            string raza,
            int edad,
            string gpsUbicacion = null
        ) : base(id)
        {
            UsuarioId = usuarioId;
            Nombre = nombre;
            Raza = raza;
            Edad = edad;
            GpsUbicacion = gpsUbicacion;
        }
        
        public void ActualizarDatos(string nombre, string raza, int edad)
        {
            Nombre = nombre;
            Raza = raza;
            Edad = edad;
            ActualizarFechaModificacion();
        }
        
        public void ActualizarUbicacion(string ubicacion)
        {
            GpsUbicacion = ubicacion;
            ActualizarFechaModificacion();
        }
        
        public void AgregarFoto(FotoPerro foto)
        {
            _fotos.Add(foto);
            ActualizarFechaModificacion();
        }
    }
