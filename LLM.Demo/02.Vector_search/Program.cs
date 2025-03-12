// See https://aka.ms/new-console-template for more information

using _00.VectoreStore.Shared;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
#pragma warning disable SKEXP0001


var kernel = KernelHelper.CreateBuilderWithAddOpenAiTextEmbeddingGeneration().Build();
var textEmbeddingGenerationService = kernel.Services.GetService<ITextEmbeddingGenerationService>();


while (true)
{
    Console.WriteLine("Введите команду (1 - SearchAnInMemoryVectorStoreAsync,2 - SearchAnInMemoryVectorStoreWithFilteringAsync 0 - exit):");
    string? command = Console.ReadLine()?.Trim();

    if (command == "0") break;
    if (command == "1") await SearchAnInMemoryVectorStoreAsync();
    if (command == "2") await SearchAnInMemoryVectorStoreWithFilteringAsync();
    else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
}


async Task SearchAnInMemoryVectorStoreAsync()
{
    var collection = await GetVectorStoreCollectionWithDataAsync();

// Search the vector store.
    var searchResultItem = await SearchVectorStoreAsync(
        collection,
        "Tell me about Application Programming Interface",
        textEmbeddingGenerationService);

// Write the search result with its score to the console.
    Console.WriteLine(searchResultItem.Record.Definition);
    Console.WriteLine(searchResultItem.Score);
}




static async Task<VectorSearchResult<Glossary>> SearchVectorStoreAsync(IVectorStoreRecordCollection<string, Glossary> collection, string searchString, ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
    // Generate an embedding from the search string.
    var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);

    // Search the store and get the single most relevant result.
    var searchResult = await collection.VectorizedSearchAsync(
        searchVector,
        new()
        {
            Top = 1
        });
    var searchResultItems = await searchResult.Results.ToListAsync();
    return searchResultItems.First();
}


 async Task SearchAnInMemoryVectorStoreWithFilteringAsync()
{
    var collection = await GetVectorStoreCollectionWithDataAsync();

    // Generate an embedding from the search string.
    var searchString = "How do I provide additional context to an LLM?";
    var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);

    // Search the store with a filter and get the single most relevant result.
    var searchResult = await collection.VectorizedSearchAsync(
        searchVector,
        new()
        {
            Top = 1,
            Filter = new VectorSearchFilter().EqualTo(nameof(Glossary.Category), "AI")
        });
    var searchResultItems = await searchResult.Results.ToListAsync();

    // Write the search result with its score to the console.
    Console.WriteLine(searchResultItems.First().Record.Definition);
    Console.WriteLine(searchResultItems.First().Score);
}


async Task<IVectorStoreRecordCollection<string, Glossary>> GetVectorStoreCollectionWithDataAsync()
{
    // Construct the vector store and get the collection.
    var vectorStore = new InMemoryVectorStore();
    var collection = vectorStore.GetCollection<string, Glossary>("skglossary");

    // Ingest data into the collection using the code from step 1.
    await VectorDataHelper.IngestDataIntoVectorStoreAsync(collection, textEmbeddingGenerationService);

    return collection;
}