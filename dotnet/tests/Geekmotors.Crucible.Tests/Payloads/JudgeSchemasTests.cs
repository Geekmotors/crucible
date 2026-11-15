using Geekmotors.Crucible.Payloads;
using Geekmotors.Crucible.Tests.Constants;
using Geekmotors.Crucible.Tests.Harnesses;

namespace Geekmotors.Crucible.Tests.Payloads;

public sealed class JudgeSchemasTests
{
    [Fact]
    public void TheCoherentSystemPromptIsTheSchemaFileWordForWord() =>
        Assert.Equal(RepositorySchemas.Text(AssertionTexts.CoherentSchema), JudgeSchemas.Coherent);

    [Fact]
    public void TheExtractSystemPromptIsTheSchemaFileWordForWord() =>
        Assert.Equal(RepositorySchemas.Text(AssertionTexts.ExtractSchema), JudgeSchemas.Extract);
}
