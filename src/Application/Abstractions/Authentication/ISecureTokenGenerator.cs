namespace Application.Abstractions.Authentication;

public interface ISecureTokenGenerator
{
    string Generate();
}
