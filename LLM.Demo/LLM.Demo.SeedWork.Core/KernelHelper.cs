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
namespace LLM.Demo.SeedWork.Core;

public static class KernelHelper
{
    const string Model = "phi-4";
    private const string Endpoint = "http://localhost:1234/v1";
    
    public static IKernelBuilder CreateBuilder()
    {
        
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: Model,
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
}