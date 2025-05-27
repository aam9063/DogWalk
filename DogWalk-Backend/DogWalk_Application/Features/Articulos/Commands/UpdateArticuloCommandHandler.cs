using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands;

/// <summary>
/// Manejador para actualizar un artículo.
/// </summary>
public class UpdateArticuloCommandHandler : IRequestHandler<UpdateArticuloCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Constructor para el manejador de comandos de actualización de artículo.
    /// </summary>
    /// <param name="unitOfWork">Unidad de trabajo.</param>
    public UpdateArticuloCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Maneja el comando de actualización de artículo.
    /// </summary>
    /// <param name="request">Comando de actualización de artículo.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task<bool> Handle(UpdateArticuloCommand request, CancellationToken cancellationToken)
    {
        var articulo = await _unitOfWork.Articulos.GetByIdAsync(request.Id);
        
        if (articulo == null)
            return false;
            
        var dto = request.ArticuloDto;
        
        // Actualizar propiedades
        articulo.ActualizarDatos(
            dto.Nombre,
            dto.Descripcion,
            Dinero.Create(dto.Precio),
            (CategoriaArticulo)dto.Categoria
        );
        
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
