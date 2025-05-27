using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DogWalk_Application.Common.Behaviors;

/// <summary>
/// Comportamiento de validación para las solicitudes de MediatR.
/// </summary>
/// <typeparam name="TRequest">Tipo de solicitud</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta</typeparam>
/// <param name="validators">Validadores para la solicitud</param>
/// <returns>Respuesta de la solicitud</returns>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Constructor del comportamiento de validación.
    /// </summary>
    /// <param name="validators">Validadores para la solicitud</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Maneja la solicitud y aplica la validación.
    /// </summary>
    /// <param name="request">Solicitud</param>
    /// <param name="next">Delegado para la siguiente etapa</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }
        
        return await next();
    }
}
