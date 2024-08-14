namespace Zilean.Tests.Collections;

[CollectionDefinition(nameof(ElasticTestCollectionDefinition))]
public class ElasticTestCollectionDefinition : ICollectionFixture<PostgresLifecycleFixture>;
