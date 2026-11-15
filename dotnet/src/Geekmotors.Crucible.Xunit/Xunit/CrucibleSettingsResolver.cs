using System.Reflection;
using CrucibleAI.Options;

namespace CrucibleAI.Xunit;

internal sealed class CrucibleSettingsResolver
{
    private readonly RunOptionsValidator _validator = new();

    public GatePlan Resolve(Type testClass, ICrucibleSettings? method)
    {
        var groups = Groups(testClass).ToArray();
        var declared = method?.Settings;
        var sequential = groups.Append(declared).LastOrDefault(settings => settings?.Sequential is not null)?.Sequential ?? false;
        return new GatePlan(
            groups.Select(settings => _validator.Validate(settings.Options, RunOptionsSource.Group)).ToArray(),
            _validator.Validate(declared?.Options ?? new RunOptions(), RunOptionsSource.Test),
            sequential);
    }

    private static IEnumerable<CrucibleSettings> Groups(Type testClass)
    {
        var assembly = testClass.Assembly.GetCustomAttribute<CrucibleAttribute>();
        var classes = Nesting(testClass).Reverse().SelectMany(type => Inheritance(type).Reverse());
        return classes
            .Select(type => type.GetCustomAttribute<CrucibleAttribute>(false))
            .Prepend(assembly)
            .OfType<ICrucibleSettings>()
            .Select(attribute => attribute.Settings);
    }

    private static IEnumerable<Type> Nesting(Type type)
    {
        for (Type? current = type; current is not null; current = current.DeclaringType)
        {
            yield return current;
        }
    }

    private static IEnumerable<Type> Inheritance(Type type)
    {
        for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            yield return current;
        }
    }
}
