using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Username).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
        RuleFor(command => command.ApplicationData).NotEmpty();
    }
}
