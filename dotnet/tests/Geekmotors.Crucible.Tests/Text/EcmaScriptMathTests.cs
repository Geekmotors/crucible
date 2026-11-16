using CrucibleAI.Text;

namespace CrucibleAI.Tests.Text;

public sealed class EcmaScriptMathTests
{
    [Theory]
    [InlineData(1234.5, 1235d)]
    [InlineData(2.5, 3d)]
    [InlineData(-2.5, -2d)]
    [InlineData(999.4, 999d)]
    [InlineData(999.6, 1000d)]
    [InlineData(0.49999999999999994, 0d)]
    [InlineData(-0.5, 0d)]
    [InlineData(250d, 250d)]
    [InlineData(-1.5, -1d)]
    [InlineData(4503599627370495.5, 4503599627370496d)]
    [InlineData(-4503599627370495.5, -4503599627370495d)]
    [InlineData(1e300, 1e300)]
    public void RoundsHalvesTowardPositiveInfinity(double value, double expected) =>
        Assert.Equal(expected, EcmaScriptMath.Round(value));
}
