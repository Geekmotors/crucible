using Geekmotors.Crucible.Judging;

namespace Geekmotors.Crucible.Assertions;

internal static class ExtractTypes
{
    private static readonly Dictionary<Type, ExtractType> Supported = new()
    {
        [typeof(string)] = ExtractType.String,
        [typeof(double)] = ExtractType.Number,
        [typeof(bool)] = ExtractType.Boolean,
    };

    public static ExtractType For(Type? requested)
    {
        if (requested is null)
        {
            return ExtractType.String;
        }
        var type = Nullable.GetUnderlyingType(requested) ?? requested;
        return Supported.TryGetValue(type, out var supported)
            ? supported
            : throw CrucibleException.Usage("Extract a string, a double or a bool.");
    }
}
