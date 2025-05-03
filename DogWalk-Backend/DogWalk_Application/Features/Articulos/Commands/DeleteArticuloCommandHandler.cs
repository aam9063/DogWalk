using DogWalk_Domain.Interfaces.IRepositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Features.Articulos.Commands
{
    public class DeleteArticuloCommandHandler : IRequestHandler<DeleteArticuloCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteArticuloCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

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
