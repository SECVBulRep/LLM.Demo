using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070
namespace LLM.Demo.SeedWork.Core;

public static class KernelHelper
{
    const string Model = "NousResearch/Hermes-3-Llama-3.1-8B";
    const string ModelWithAddOpenAITextEmbeddingGeneration = "BAAI/bge-m3";
    private const string Endpoint = "http://lomonosov-mini:8000/v1";
    
    
    
    
    
    // const string Model = "phi-4";
    // private const string Endpoint = "http://localhost:1234/v1";
    

    // public static IKernelBuilder CreateBuilder()
    // {
    //     return Kernel.CreateBuilder()
    //         .AddOpenAIChatCompletion(
    //             modelId: Model,
    //             apiKey: null,
    //             endpoint: new Uri(Endpoint));
    //
    // }

    public static IKernelBuilder CreateBuilderWithAddOpenAiTextEmbeddingGeneration()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(Endpoint) };


        var kernel = Kernel.CreateBuilder()
            .AddOpenAITextEmbeddingGeneration(
                modelId: ModelWithAddOpenAITextEmbeddingGeneration, // Модель должна поддерживать embeddings
                apiKey: "123", // API-ключ не нужен для локального vLLM
                httpClient: httpClient);

        return kernel;
    }

    public static IKernelBuilder CreateBuilder()
    {
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: Model,
                apiKey: null,
                endpoint: new Uri(Endpoint));
        // .AddOpenAITextEmbeddingGeneration(
        //     modelId: Model,  // Должна быть модель с поддержкой эмбеддингов
        //     apiKey: null,
        //     endpoint: new Uri(Endpoint));
    }

    

    [Experimental("SKEXP0070")]
    public static IKernelBuilder CreateBuilderHuggingFace()
    {
        return Kernel.CreateBuilder()
            .AddHuggingFaceTextGeneration(Model,
                apiKey: null,
                endpoint: new Uri(Endpoint))
            .AddHuggingFaceChatCompletion(
                Model,
                apiKey: null,
                endpoint: new Uri(Endpoint));
    }


    public static IKernelBuilder AddKernelAndAddOpenAIChatCompletion(this ServiceCollection collection)
    {
        var serilog = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();
        var loggerFactory = new LoggerFactory().AddSerilog(serilog);

        collection.AddSingleton<ILoggerFactory>(loggerFactory);

        var kernel = collection.AddKernel();
        collection.AddOpenAIChatCompletion(
            modelId: Model,
            apiKey: null,
            endpoint: new Uri(Endpoint));

        return kernel;
    }

    public static Kernel BuildKernelAndServiceProvider()
    {
        var collection = new ServiceCollection();
        var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
        return collection.BuildServiceProvider().GetRequiredService<Kernel>();
    }
}