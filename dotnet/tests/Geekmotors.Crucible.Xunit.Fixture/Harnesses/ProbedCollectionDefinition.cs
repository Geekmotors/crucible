using CrucibleAI.Xunit.Fixture.Constants;

namespace CrucibleAI.Xunit.Fixture.Harnesses;

[CollectionDefinition(FixtureCollections.Probed)]
public sealed class ProbedCollectionDefinition : ICollectionFixture<CollectionFixtureProbe>;
