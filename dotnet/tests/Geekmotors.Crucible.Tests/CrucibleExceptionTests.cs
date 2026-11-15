namespace CrucibleAI.Tests;

public sealed class CrucibleExceptionTests
{
    [Fact]
    public void AFatalInfraErrorIsNotRetryable() =>
        Assert.False(CrucibleException.Infra(nameof(CrucibleExceptionTests), retryable: false).Retryable);

    [Fact]
    public void ATransientInfraErrorIsRetryable() =>
        Assert.True(CrucibleException.Infra(nameof(CrucibleExceptionTests), retryable: true).Retryable);

    [Fact]
    public void AUsageErrorHasNoRetryable() =>
        Assert.Null(CrucibleException.Usage(nameof(CrucibleExceptionTests)).Retryable);

    [Fact]
    public void AConfigErrorHasNoRetryable() =>
        Assert.Null(CrucibleException.Config(nameof(CrucibleExceptionTests)).Retryable);
}
