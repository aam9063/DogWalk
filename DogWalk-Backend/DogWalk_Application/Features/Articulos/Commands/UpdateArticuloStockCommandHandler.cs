using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands
{
    public class UpdateArticuloStockCommandHandler : IRequestHandler<UpdateArticuloStockCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateArticuloStockCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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
