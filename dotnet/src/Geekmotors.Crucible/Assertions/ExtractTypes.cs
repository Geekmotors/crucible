using CrucibleAI.Judging;

namespace CrucibleAI.Assertions;

internal static class ExtractTypes
{
    private static readonly ExtractTarget Text = new(ExtractType.String);

    private static readonly Dictionary<Type, ExtractTarget> Supported = new()
    {
        [typeof(string)] = Text,
        [typeof(double?)] = new(ExtractType.Number),
        [typeof(int?)] = new(ExtractType.Number, ExtractNumber.Int32),
        [typeof(long?)] = new(ExtractType.Number, ExtractNumber.Int64),
        [typeof(bool?)] = new(ExtractType.Boolean),
    };

    public static ExtractTarget For(Type? requested)
    {
        if (requested is null)
        {
            return Text;
        }
        return Supported.TryGetValue(requested, out var supported)
            ? supported
            : throw CrucibleException.Usage("Extract a string, int?, long?, double? or bool?: the judge may answer null.");
    }
}
