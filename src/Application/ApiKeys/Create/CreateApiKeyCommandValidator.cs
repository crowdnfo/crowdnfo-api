using FluentValidation;

namespace Application.ApiKeys.Create;

internal sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
    }
}
