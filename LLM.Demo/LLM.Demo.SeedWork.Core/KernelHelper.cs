using Microsoft.SemanticKernel;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
namespace LLM.Demo.SeedWork.Core;

public class KernelHelper
{
    public static IKernelBuilder CreateBuilder()
    {
        
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "phi-4",
                apiKey: null,
                endpoint: new Uri("http://localhost:1234/v1"));
    }
}