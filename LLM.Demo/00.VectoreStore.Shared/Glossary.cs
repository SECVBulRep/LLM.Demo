using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace _00.VectoreStore.Shared;

public sealed class Glossary
{
    [VectorStoreRecordKey]
    public string Key { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string Category { get; set; }

    [VectorStoreRecordData]
    public string Term { get; set; }

    [VectorStoreRecordData]
    public string Definition { get; set; }

    [VectorStoreRecordVector(Dimensions: 1024)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}


public static class VectorDataHelper
{
    [Experimental("SKEXP0001")]
    public static async Task<IEnumerable<string>> IngestDataIntoVectorStoreAsync(
        IVectorStoreRecordCollection<string, Glossary> collection,
        ITextEmbeddingGenerationService textEmbeddingGenerationService)
    {
        
        await collection.CreateCollectionIfNotExistsAsync();

        
        var glossaryEntries = CreateGlossaryEntries().ToList();
        var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
        {
            entry.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
        }));
        await Task.WhenAll(tasks);
        
        var upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
        return await Task.WhenAll(upsertedKeysTasks);
    }

    public static IEnumerable<Glossary> CreateGlossaryEntries()
    {
        yield return new Glossary
        {
            Key = "1",
            Category = "Software",
            Term = "API",
            Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
        };

        yield return new Glossary
        {
            Key = "2",
            Category = "Software",
            Term = "SDK",
            Definition = "Software development kit. A set of libraries and tools that allow software developers to build software more easily."
        };

        yield return new Glossary
        {
            Key = "3",
            Category = "SK",
            Term = "Connectors",
            Definition = "Semantic Kernel Connectors allow software developers to integrate with various services providing AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
        };

        yield return new Glossary
        {
            Key = "4",
            Category = "SK",
            Term = "Semantic Kernel",
            Definition = "Semantic Kernel is a set of libraries that allow software developers to more easily develop applications that make use of AI experiences."
        };

        yield return new Glossary
        {
            Key = "5",
            Category = "AI",
            Term = "RAG",
            Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
        };

        yield return new Glossary
        {
            Key = "6",
            Category = "AI",
            Term = "LLM",
            Definition = "Large language model. A type of artificial ingelligence algorithm that is designed to understand and generate human language."
        };
    }
    
}