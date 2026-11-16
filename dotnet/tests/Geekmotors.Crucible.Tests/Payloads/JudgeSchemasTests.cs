using CrucibleAI.Payloads;
using CrucibleAI.Tests.Constants;
using CrucibleAI.Tests.Harnesses;

namespace CrucibleAI.Tests.Payloads;

public sealed class JudgeSchemasTests
{
    [Fact]
    public void TheCoherentSystemPromptIsTheSchemaFileWordForWord() =>
        Assert.Equal(RepositorySchemas.Text(AssertionTexts.CoherentSchema), JudgeSchemas.Coherent);

    [Fact]
    public void TheExtractSystemPromptIsTheSchemaFileWordForWord() =>
        Assert.Equal(RepositorySchemas.Text(AssertionTexts.ExtractSchema), JudgeSchemas.Extract);
}
