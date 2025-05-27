using DogWalk_Application.Contracts.DTOs.Busqueda;
using DogWalk_Application.Contracts.DTOs.Paseadores;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Paseadores.Queries
{
    /// <summary>
    /// Manejador para buscar paseadores.
    /// </summary>
    public class BuscarPaseadoresQueryHandler : IRequestHandler<BuscarPaseadoresQuery, ResultadoPaginadoDto<PaseadorMapDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de consultas de paseadores.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public BuscarPaseadoresQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja la consulta para buscar paseadores.
        /// </summary>
        /// <param name="request">Consulta para buscar paseadores.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<ResultadoPaginadoDto<PaseadorMapDto>> Handle(BuscarPaseadoresQuery request, CancellationToken cancellationToken)
        {
            // Obtener todos los paseadores
            var todosLosPaseadores = await _unitOfWork.Paseadores.GetAllAsync();
            var paseadoresFiltrados = todosLosPaseadores.AsQueryable();

            // Aplicar filtros
            // 1. Filtrar por valoración mínima
            if (request.ValoracionMinima.HasValue)
            {
                paseadoresFiltrados = paseadoresFiltrados.Where(p => p.ValoracionGeneral >= request.ValoracionMinima.Value);
            }

            // 2. Filtrar por disponibilidad de fecha
            if (request.FechaEntrega.HasValue)
            {
                var paseadoresDisponibles = await _unitOfWork.Paseadores.GetWithDisponibilidadAsync(request.FechaEntrega.Value);
                var paseadoresIds = paseadoresDisponibles.Select(p => p.Id).ToList();
                paseadoresFiltrados = paseadoresFiltrados.Where(p => paseadoresIds.Contains(p.Id));
            }

            // 3. Filtrar por servicio
            if (request.ServicioId.HasValue)
            {
                var paseadoresConServicio = await _unitOfWork.Paseadores.GetWithServicioAsync(request.ServicioId.Value);
                var paseadoresIds = paseadoresConServicio.Select(p => p.Id).ToList();
                paseadoresFiltrados = paseadoresFiltrados.Where(p => paseadoresIds.Contains(p.Id));
            }

            // 4. Filtrar por ubicación/distancia
            if (request.Latitud.HasValue && request.Longitud.HasValue && request.DistanciaMaxima.HasValue)
            {
                var paseadoresCercanos = await _unitOfWork.Paseadores.GetByDistanciaAsync(
                    request.Latitud.Value,
                    request.Longitud.Value,
                    request.DistanciaMaxima.Value);
                
                var paseadoresIds = paseadoresCercanos.Select(p => p.Id).ToList();
                paseadoresFiltrados = paseadoresFiltrados.Where(p => paseadoresIds.Contains(p.Id));
            }

            // 5. Filtrar por código postal (si existe)
            if (!string.IsNullOrEmpty(request.CodigoPostal))
            {
                paseadoresFiltrados = paseadoresFiltrados.Where(p => 
                    p.Direccion.ToString().Contains(request.CodigoPostal));
            }

            // Total de elementos que coinciden con los filtros
            var totalItems = paseadoresFiltrados.Count();

            // Aplicar paginación
            var paseadoresPaginados = paseadoresFiltrados
                .Skip((request.Pagina - 1) * request.ElementosPorPagina)
                .Take(request.ElementosPorPagina)
                .ToList();

            // Mapear a DTOs
            var paseadoresDto = new List<PaseadorMapDto>();
            
            foreach (var paseador in paseadoresPaginados)
            {
                // Obtener los servicios y precios para este paseador
                var precios = paseador.Precios.ToList();
                var servicios = new List<ServicioPrecioSimpleDto>();
                
                foreach (var precio in precios)
                {
                    servicios.Add(new ServicioPrecioSimpleDto
                    {
                        ServicioId = precio.ServicioId,
                        NombreServicio = precio.Servicio.Nombre,
                        Precio = precio.Valor.Cantidad
                    });
                }

                // Calcular precio base
                var precioBase = precios.Any() ? precios.Min(p => p.Valor.Cantidad) : 0;
                
                // Generar etiquetas
                var etiquetas = new List<string>();
                if (paseador.ValoracionGeneral >= 4.5m)
                    etiquetas.Add("Excelente valoración");
                
                if (precios.Any(p => p.Servicio.Nombre.Contains("Alojamiento")))
                    etiquetas.Add("Alojamiento");
                    
                if (precios.Any(p => p.Servicio.Nombre.Contains("Paseo")))
                    etiquetas.Add("Paseo");

                // Contar valoraciones
                int cantidadValoraciones = paseador.ValoracionesRecibidas.Count;
                
                paseadoresDto.Add(new PaseadorMapDto
                {
                    Id = paseador.Id,
                    Nombre = paseador.Nombre,
                    Apellido = paseador.Apellido,
                    FotoPerfil = paseador.FotoPerfil,
                    ValoracionGeneral = paseador.ValoracionGeneral,
                    CantidadValoraciones = cantidadValoraciones,
                    Latitud = paseador.Ubicacion.Latitud,
                    Longitud = paseador.Ubicacion.Longitud,
                    Servicios = servicios,
                    PrecioBase = precioBase,
                    Etiquetas = etiquetas
                });
            }

            // Calcular paginación
            int totalPaginas = (int)Math.Ceiling(totalItems / (double)request.ElementosPorPagina);

            // Devolver resultado paginado
            return new ResultadoPaginadoDto<PaseadorMapDto>
            {
                Items = paseadoresDto,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                PaginaActual = request.Pagina,
                ElementosPorPagina = request.ElementosPorPagina
            };
        }
    }
}
