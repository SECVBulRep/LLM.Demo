// See https://aka.ms/new-console-template for more information

using _00.VectoreStore.Shared;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010


var kernel = KernelHelper.CreateBuilderWithAddOpenAiTextEmbeddingGeneration().Build();
var embeddingGenerator = kernel.Services.GetService<ITextEmbeddingGenerationService>();

var vectorStore = new InMemoryVectorStore();
var collection = vectorStore.GetCollection<string, Glossary>("skglossary");

await VectorDataHelper.IngestDataIntoVectorStoreAsync(collection, embeddingGenerator);

var record = await collection.GetAsync("1");
Console.WriteLine(record!.Definition);