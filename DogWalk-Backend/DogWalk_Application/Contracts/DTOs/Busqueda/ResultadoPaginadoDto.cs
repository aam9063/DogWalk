using System;

namespace DogWalk_Application.Contracts.DTOs.Busqueda
{
    /// <summary>
    /// DTO para representar un resultado paginado.
    /// </summary>
    public class ResultadoPaginadoDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public int ElementosPorPagina { get; set; }
    }
}
