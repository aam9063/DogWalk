using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands
{
    /// <summary>
    /// Manejador para eliminar un artículo.
    /// </summary>
    public class DeleteArticuloCommandHandler : IRequestHandler<DeleteArticuloCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor para el manejador de comandos de eliminación de artículo.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo.</param>
        public DeleteArticuloCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Maneja el comando de eliminación de artículo.
        /// </summary>
        /// <param name="request">Comando de eliminación de artículo.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        public async Task<bool> Handle(DeleteArticuloCommand request, CancellationToken cancellationToken)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(request.Id);
            
            if (articulo == null)
                return false;
                
            await _unitOfWork.Articulos.DeleteAsync(articulo);
            await _unitOfWork.SaveChangesAsync();
            
            return true;
        }
    }
}
