using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System.Collections.Generic;


namespace DogWalk_Domain.Entities;

public class Articulo : EntityBase
    {
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public Dinero Precio { get; private set; }
        public int Stock { get; private set; }
        public CategoriaArticulo Categoria { get; private set; }
        
        // Relaciones
        private readonly List<ImagenArticulo> _imagenes = new();
        public IReadOnlyCollection<ImagenArticulo> Imagenes => _imagenes.AsReadOnly();
        
        private Articulo() : base() { } // Para EF Core
        
        public Articulo(
            Guid id,
            string nombre,
            string descripcion,
            Dinero precio,
            int stock,
            CategoriaArticulo categoria
        ) : base(id)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            Stock = stock;
            Categoria = categoria;
        }
        
        public void ActualizarDatos(
            string nombre,
            string descripcion,
            Dinero precio,
            CategoriaArticulo categoria
        )
        {
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
            Categoria = categoria;
            ActualizarFechaModificacion();
        }
        
        public void ActualizarStock(int cantidad)
        {
            if (Stock + cantidad < 0)
                throw new InvalidOperationException("No hay suficiente stock disponible");
                
            Stock += cantidad;
            ActualizarFechaModificacion();
        }
        
        public void AgregarImagen(ImagenArticulo imagen)
        {
            _imagenes.Add(imagen);
            ActualizarFechaModificacion();
        }
        
        public bool ReducirStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser positiva", nameof(cantidad));
                
            if (Stock < cantidad)
                return false;
                
            Stock -= cantidad;
            ActualizarFechaModificacion();
            return true;
        }
    }
