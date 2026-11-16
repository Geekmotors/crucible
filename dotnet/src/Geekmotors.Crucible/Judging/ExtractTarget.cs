namespace CrucibleAI.Judging;

internal sealed record ExtractTarget(ExtractType Type, ExtractNumber Number = ExtractNumber.Any)
{
    private const double Int64Bound = 9223372036854775808d;

    public bool Accepts(double value) => Number switch
    {
        ExtractNumber.Int32 => Math.Floor(value) == value && value >= int.MinValue && value <= int.MaxValue,
        ExtractNumber.Int64 => Math.Floor(value) == value && value >= -Int64Bound && value < Int64Bound,
        _ => true,
    };

    public object? Converted(object? value) => (Number, value) switch
    {
        (ExtractNumber.Int32, double number) => (int)number,
        (ExtractNumber.Int64, double number) => (long)number,
        _ => value,
    };
}
