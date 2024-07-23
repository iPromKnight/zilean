namespace Zilean.Shared.Features.Imdb;

public static class ImdbIndexer
{
    private const string ImdbEnrichPolicy = "imdb_policy";
    public const string ImdbEnrichPipeline = "imdb_enrich_pipeline";

    public static IClrTypeMapping<ImdbFile> ImdbFileDefaultMapping(ClrTypeMappingDescriptor<ImdbFile> x)
    {
        x.IndexName(ElasticSearchClient.ImdbMetadataIndex);
        x.IdProperty(p => p.ImdbId);

        return x;
    }

    public static async Task<bool> SetupImdbPipeline(ElasticClient client, ILogger<ElasticSearchClient> logger)
    {
        var createIndexResponse = await CreateImdbMetadataIndex(client);
        if (!createIndexResponse.IsValid)
        {
            logger.LogError("Failed to create imdb_metadata index");
            return false;
        }

        var createPolicyResponse = await CreateEnrichPolicy(client, logger);
        if (!createPolicyResponse)
        {
            return false;
        }

        var executePolicyResponse = await ExecuteEnrichPolicy(client, logger);
        if (!executePolicyResponse.IsValid)
        {
            return false;
        }

        var createPipelineResponse = await CreateIngestPipeline(client, logger);
        return createPipelineResponse;
    }

    private static Task<CreateIndexResponse> CreateImdbMetadataIndex(ElasticClient client) =>
        client.Indices.CreateAsync(ElasticSearchClient.ImdbMetadataIndex, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Tokenizers(t => t
                        .NGram("trigram_tokenizer", ng => ng
                            .MinGram(3)
                            .MaxGram(3)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit)
                        )
                    )
                    .Analyzers(an => an
                        .Custom("trigram_analyzer", ca => ca
                            .Tokenizer("trigram_tokenizer")
                            .Filters("lowercase")
                        )
                    )
                )
                .NumberOfReplicas(0)
                .NumberOfShards(1)
            )
            .Map<ImdbFile>(m => m
                .Properties(p => p
                    .Keyword(k => k
                        .Name(n => n.ImdbId)
                    )
                    .Text(t => t
                        .Name(n => n.Title)
                        .Analyzer("trigram_analyzer")
                    )
                    .Keyword(k => k
                        .Name(n => n.Category)
                    )
                    .Number(nu => nu
                        .Name(n => n.Year)
                        .Type(NumberType.Integer)
                    )
                    .Boolean(b => b
                        .Name(n => n.Adult)
                    )
                )
            )
        );

    private static async Task<bool> CreateEnrichPolicy(ElasticClient client, ILogger<ElasticSearchClient> logger)
    {
        var exists = await EnrichPolicyExistsAsync(client);

        if (exists)
        {
            return true;
        }

        var result = await client.Enrich.PutPolicyAsync<ImdbFile>(ImdbEnrichPolicy, p => p
            .Match(m => m
                .Indices(ElasticSearchClient.ImdbMetadataIndex)
                .MatchField(f => f.Title)
                .EnrichFields(f => f
                    .Field(ff => ff.ImdbId)
                    .Field(ff => ff.Category)
                    .Field(ff => ff.Year)
                )
            )
        );

        if (!result.IsValid)
        {
            logger.LogError("Failed to create enrich policy {PolicyName}", ImdbEnrichPolicy);
            if (result.OriginalException != null)
            {
                logger.LogError(result.OriginalException, "Error: {PolicyName}", ImdbEnrichPolicy);
            }

            return false;
        }

        return true;
    }

    private static Task<ExecuteEnrichPolicyResponse> ExecuteEnrichPolicy(ElasticClient client, ILogger<ElasticSearchClient> logger) =>
        client.Enrich.ExecutePolicyAsync(ImdbEnrichPolicy);

    private static async Task<bool> CreateIngestPipeline(ElasticClient client, ILogger<ElasticSearchClient> logger)
    {
        var exists = await PipelineExistsAsync(client);

        if (exists)
        {
            return true;
        }

        var result = await client.Ingest.PutPipelineAsync(ImdbEnrichPipeline, p => p
            .Processors(pr => pr
                .Enrich<TorrentInfo>(e => e
                    .PolicyName(ImdbEnrichPolicy)
                    .Field(f => f.Title)
                    .TargetField(f => f.ImdbId)
                    .MaxMatches(1)
                    .IgnoreMissing()
                )
            )
        );

        if (!result.IsValid)
        {
            logger.LogError("Failed to create enrich pipeline {PipelineName}", ImdbEnrichPipeline);
            if (result.OriginalException != null)
            {
                logger.LogError(result.OriginalException, "Error: {PipelineName}", ImdbEnrichPipeline);
            }

            return false;
        }

        return true;
    }

    private static async Task<bool> EnrichPolicyExistsAsync(ElasticClient client)
    {
        var response = await client.Enrich.GetPolicyAsync(ImdbEnrichPolicy);
        return response.IsValid && response.Policies.ElementAtOrDefault(0)?.Config is not null;
    }

    private static async Task<bool> PipelineExistsAsync(ElasticClient client)
    {
        var response = await client.Ingest.GetPipelineAsync(descriptor => descriptor.Id(ImdbEnrichPipeline));
        return response.IsValid;
    }
}
