using Geekmotors.Crucible.Options;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Options;

public sealed class RunOptionsValidatorTests
{
    private readonly RunOptionsValidator _validator = new();

    [Fact]
    public void AcceptsDeclaredOptionsUnchanged()
    {
        var options = new RunOptions { Runs = OptionValues.Runs, Threshold = OptionValues.Threshold };

        Assert.Equal(options, _validator.Validate(options, RunOptionsSource.Test));
    }

    [Fact]
    public void RejectsANotANumberThresholdInATestAsUsage()
    {
        var error = Assert.Throws<CrucibleException>(
            () => _validator.Validate(new RunOptions { Threshold = double.NaN }, RunOptionsSource.Test));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void RejectsZeroRunsInAGroupAsUsage()
    {
        var error = Assert.Throws<CrucibleException>(
            () => _validator.Validate(new RunOptions { Runs = OptionValues.NoRuns }, RunOptionsSource.Group));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void RejectsAnInfiniteThresholdInTestDefaultsAsConfig()
    {
        var error = Assert.Throws<CrucibleException>(
            () => _validator.Validate(new RunOptions { Threshold = double.PositiveInfinity }, RunOptionsSource.Config));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }

    [Fact]
    public void RejectsMissingTestOptionsAsUsage()
    {
        var error = Assert.Throws<CrucibleException>(() => _validator.Validate(null, RunOptionsSource.Test));

        Assert.Equal(CrucibleErrorKind.Usage, error.Kind);
    }

    [Fact]
    public void AcceptsMissingTestDefaultsAsNothingDeclared() =>
        Assert.Equal(new RunOptions(), _validator.Validate(null, RunOptionsSource.Config));
}
