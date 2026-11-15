using Geekmotors.Crucible.Json;
using Geekmotors.Crucible.Tests.Constants;

namespace Geekmotors.Crucible.Tests.Json;

public sealed class JsoncReaderTests
{
    [Fact]
    public void AnOverflowingNumberIsAConfigError()
    {
        var error = Assert.Throws<CrucibleException>(() => new JsoncReader().Read(JsonTexts.Overflowing));

        Assert.Equal(CrucibleErrorKind.Config, error.Kind);
    }
}
