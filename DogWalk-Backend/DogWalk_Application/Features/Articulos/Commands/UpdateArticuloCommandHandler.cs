using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands;

public class UpdateArticuloCommandHandler : IRequestHandler<UpdateArticuloCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateArticuloCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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
        
        // Actualizar imágenes si se proporcionan
        // Nota: Aquí se debería implementar la lógica para actualizar imágenes
        // Lo ideal sería eliminar las existentes y agregar las nuevas
        
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
