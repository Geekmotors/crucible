using CrucibleAI.Text;

namespace CrucibleAI.Tests.Text;

public sealed class EcmaScriptNumberTests
{
    [Theory]
    [InlineData(1d, "1")]
    [InlineData(20d, "20")]
    [InlineData(0.9, "0.9")]
    [InlineData(0.857, "0.857")]
    [InlineData(12.5, "12.5")]
    [InlineData(-3d, "-3")]
    [InlineData(0.30000000000000004, "0.30000000000000004")]
    [InlineData(1e15, "1000000000000000")]
    [InlineData(1e17, "100000000000000000")]
    [InlineData(123456789012345680d, "123456789012345680")]
    [InlineData(1e20, "100000000000000000000")]
    [InlineData(9007199254740992d, "9007199254740992")]
    [InlineData(9007199254740994d, "9007199254740994")]
    [InlineData(18446744073709551616d, "18446744073709552000")]
    [InlineData(999999999999999900000d, "999999999999999900000")]
    [InlineData(-1e21, "-1e+21")]
    [InlineData(1.5e21, "1.5e+21")]
    [InlineData(0.3333333333333333, "0.3333333333333333")]
    [InlineData(0.7999999999999999, "0.7999999999999999")]
    [InlineData(1.23e-18, "1.23e-18")]
    [InlineData(2.2250738585072014e-308, "2.2250738585072014e-308")]
    [InlineData(1e21, "1e+21")]
    [InlineData(1.5e300, "1.5e+300")]
    [InlineData(1.7976931348623157e308, "1.7976931348623157e+308")]
    [InlineData(0.0001, "0.0001")]
    [InlineData(0.00001, "0.00001")]
    [InlineData(0.000001, "0.000001")]
    [InlineData(1.5e-6, "0.0000015")]
    [InlineData(1e-7, "1e-7")]
    [InlineData(-1.25e-7, "-1.25e-7")]
    [InlineData(5e-324, "5e-324")]
    [InlineData(0d, "0")]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    public void PrintsAsNumberToString(double value, string expected) =>
        Assert.Equal(expected, EcmaScriptNumber.Format(value));

    [Fact]
    public void PrintsNegativeZeroAsZero() => Assert.Equal("0", EcmaScriptNumber.Format(-0d));
}
