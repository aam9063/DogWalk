using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System.Collections.Generic;

namespace DogWalk_Domain.Entities;

public class Paseador : EntityBase
{
    public RolUsuario Rol { get; private set; }
    public Dni Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public Direccion Direccion { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public Telefono Telefono { get; private set; }
    public decimal ValoracionGeneral { get; private set; } = 0;
    public Coordenadas Ubicacion { get; private set; }
    public string FotoPerfil { get; private set; }

    // Relaciones
    private readonly List<Precio> _precios = new();
    public IReadOnlyCollection<Precio> Precios => _precios.AsReadOnly();

    private readonly List<Reserva> _reservas = new();
    public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

    private readonly List<RankingPaseador> _valoracionesRecibidas = new();
    public IReadOnlyCollection<RankingPaseador> ValoracionesRecibidas => _valoracionesRecibidas.AsReadOnly();

    private readonly List<OpinionPerro> _opinionesDadas = new();
    public IReadOnlyCollection<OpinionPerro> OpinionesDadas => _opinionesDadas.AsReadOnly();

    private readonly List<DisponibilidadHoraria> _disponibilidad = new();
    public IReadOnlyCollection<DisponibilidadHoraria> Disponibilidad => _disponibilidad.AsReadOnly();

    private readonly List<ChatMensaje> _mensajesRecibidos = new();
    public IReadOnlyCollection<ChatMensaje> MensajesRecibidos => _mensajesRecibidos.AsReadOnly();

    private Paseador() : base() { } 

    public Paseador(
        Guid id,
        Dni dni,
        string nombre,
        string apellido,
        Direccion direccion,
        Email email,
        Password password,
        Coordenadas ubicacion,
        Telefono telefono = null
    ) : base(id)
    {
        Dni = dni;
        Nombre = nombre;
        Apellido = apellido;
        Direccion = direccion;
        Email = email;
        Password = password;
        Telefono = telefono;
        Ubicacion = ubicacion;
    }

    public void ActualizarInformacionPersonal(
        string nombre,
        string apellido,
        Direccion direccion,
        Telefono telefono)
    {
        if (!string.IsNullOrEmpty(nombre))
            Nombre = nombre;
        if (!string.IsNullOrEmpty(apellido))
            Apellido = apellido;
        if (direccion != null)
            Direccion = direccion;
        if (telefono != null)
            Telefono = telefono;

        ActualizarFechaModificacion();
    }

    public void ActualizarFotoPerfil(string urlFoto)
    {
        FotoPerfil = urlFoto;
        ActualizarFechaModificacion();
    }

    public void ActualizarUbicacion(Coordenadas ubicacion)
    {
        Ubicacion = ubicacion;
        ActualizarFechaModificacion();
    }

    public void AgregarPrecio(Precio precio)
    {
        // Verificar que el precio sea para este paseador
        if (precio.PaseadorId != Id)
        {
            throw new ArgumentException("El precio debe ser para este paseador");
        }

        // Verificar que no exista ya un precio para este servicio
        if (_precios.Any(p => p.ServicioId == precio.ServicioId))
        {
            throw new InvalidOperationException("Ya existe un precio para este servicio");
        }

        _precios.Add(precio);
        ActualizarFechaModificacion();
    }

    public void AgregarDisponibilidad(DisponibilidadHoraria disponibilidad)
    {
        _disponibilidad.Add(disponibilidad);
        ActualizarFechaModificacion();
    }

    public void ActualizarValoracion()
    {
        if (!_valoracionesRecibidas.Any())
        {
            ValoracionGeneral = 0;
            return;
        }

        ValoracionGeneral = (decimal)_valoracionesRecibidas.Average(v => v.Valoracion.Puntuacion);
        ValoracionGeneral = Math.Round(ValoracionGeneral, 2);
        ActualizarFechaModificacion();
    }
}
