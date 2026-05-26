using System.Reflection;
using System.Runtime.CompilerServices;
using Shouldly;

namespace ArchitectureTests.Domain;

/// <summary>
/// Guards the encapsulated-domain decision: Domain entities must not expose public setters
/// (mutation goes through factory methods / behavior). Init-only setters are allowed so value
/// objects modeled as records remain valid. Fails the build if anemic public setters creep in.
/// </summary>
public class EncapsulationTests : BaseTest
{
    [Fact]
    public void DomainTypes_ShouldNotExposePublicSetters()
    {
        List<string> violations = [];

        foreach (Type type in DomainAssembly.GetTypes())
        {
            if (type.IsEnum || type.IsInterface || type.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
            {
                continue;
            }

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (PropertyInfo property in properties)
            {
                if (property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter))
                {
                    violations.Add($"{type.FullName}.{property.Name}");
                }
            }
        }

        violations.ShouldBeEmpty(
            "Domain types must not expose public setters (use private set / init-only). Offenders: " +
            string.Join(", ", violations));
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        Array.Exists(
            setter.ReturnParameter.GetRequiredCustomModifiers(),
            modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");
}
