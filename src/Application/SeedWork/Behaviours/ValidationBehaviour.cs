namespace Application.SeedWork.Behaviours;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

public class ValidationBehaviour<TRequest, TRespose> : IPipelineBehavior<TRequest, TRespose>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        this.validators = validators;
    }

    public async Task<TRespose> Handle(
        TRequest request,
        RequestHandlerDelegate<TRespose> next,
        CancellationToken cancellationToken
    )
    {
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(validator => validator.ValidateAsync(context, cancellationToken))
            );

            var errors = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (errors.Count != 0)
                throw new ValidationException(errors);
        }

        return await next();
    }
}
