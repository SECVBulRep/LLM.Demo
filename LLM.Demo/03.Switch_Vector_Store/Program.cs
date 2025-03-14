

using _00.VectoreStore.Shared;
using LLM.Demo.SeedWork.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Embeddings;
using Npgsql;
using StackExchange.Redis;
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0001

var kernel = KernelHelper.CreateBuilderWithAddOpenAiTextEmbeddingGeneration().Build();
var textEmbeddingGenerationService = kernel.Services.GetService<ITextEmbeddingGenerationService>();

NpgsqlDataSourceBuilder dataSourceBuilder = new("Host=irinka.webs.ru; Database=vector; Pooling=true");
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();

var collection = new PostgresVectorStoreRecordCollection<string, Glossary>(dataSource, "skglossaries");

await IngestDataIntoVectorStoreAsync(collection, textEmbeddingGenerationService);

var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync("What is an Application Programming Interface?");


var searchResult = await collection.VectorizedSearchAsync(
    searchVector,
    new()
    {
        Top = 1
    });
var searchResultItems = await searchResult.Results.ToListAsync();
var searchResultItem =  searchResultItems.First();

Console.WriteLine(searchResultItem.Record.Definition);
Console.WriteLine(searchResultItem.Score);

async Task<IEnumerable<string>> IngestDataIntoVectorStoreAsync(
    IVectorStoreRecordCollection<string, Glossary> collection,
    ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
   
    await collection.CreateCollectionIfNotExistsAsync();
    
    var glossaryEntries = VectorDataHelper.CreateGlossaryEntries().ToList();
    var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
    {
        entry.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
    }));
    await Task.WhenAll(tasks);

   
    var upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
    return await Task.WhenAll(upsertedKeysTasks);
}

