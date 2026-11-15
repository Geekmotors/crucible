using CrucibleAI.Json;
using CrucibleAI.Tests.Constants;

namespace CrucibleAI.Tests.Json;

public sealed class JsoncReaderTests
{
    [Fact]
    public void AnOverflowingNumberIsAConfigError()
    {
        var error = Assert.Throws<CrucibleException>(() => new JsoncReader().Read(JsonTexts.Overflowing));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }
}
