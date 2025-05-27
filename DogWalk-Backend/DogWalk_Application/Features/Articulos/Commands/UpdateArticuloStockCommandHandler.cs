using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands
{
    /// <summary>
    /// Manejador para actualizar el stock de un artículo.
    /// </summary>
    public class UpdateArticuloStockCommandHandler : IRequestHandler<UpdateArticuloStockCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de comandos de actualización de stock de artículo.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public UpdateArticuloStockCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja el comando de actualización de stock de artículo.
        /// </summary>
        /// <param name="request">Comando de actualización de stock de artículo.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<bool> Handle(UpdateArticuloStockCommand request, CancellationToken cancellationToken)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(request.Id);
            
            if (articulo == null)
                return false;
                
            // Actualizar stock
            articulo.ActualizarStock(request.Cantidad);
            
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
