// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


var serviceProvider = BuildServiceProvider();
var kernel = serviceProvider.GetRequiredService<Kernel>();

kernel.PromptRenderFilters.Add(new MyPromptFilter(serviceProvider.GetRequiredService<ILoggerFactory>()));

OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
Console.WriteLine(await kernel.InvokePromptAsync("How many days until Christmas from current date? ", new(settings)));

ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    kernelBuilder.Plugins.AddFromType<TimeInformation>();
    kernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();
    return collection.BuildServiceProvider();
}


/// <summary>
/// A plugin that returns the current time.
/// </summary>
sealed class TimeInformation
{
    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime()
    {
        return DateTime.UtcNow.ToString("R");
    }
}

/// <summary>
/// Function filter for observability.
/// </summary>
sealed class MyFunctionFilter : IFunctionInvocationFilter
{
    private readonly ILogger _logger;

    /// <summary>
    /// Function filter for observability.
    /// </summary>
    public MyFunctionFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MyFunctionFilter>();;
    }


    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        _logger.LogInformation($"Invoking {context.Function.Name}");

        await next(context);

        var metadata = context.Result?.Metadata;

        if (metadata is not null && metadata.ContainsKey("Usage"))
        {
            _logger.LogInformation($"Token usage: {metadata["Usage"]?.AsJson()}");
        }
    }
}

/// <summary>
/// Prompt filter for observability.
/// </summary>
sealed class MyPromptFilter : IPromptRenderFilter
{
    private readonly ILogger _logger;

    /// <summary>
    /// Prompt filter for observability.
    /// </summary>
    public MyPromptFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MyFunctionFilter>();;
    }


    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        _logger.LogInformation($"Rendering prompt for {context.Function.Name}");

        await next(context);

        _logger.LogInformation($"Rendered prompt: {context.RenderedPrompt}");
    }
}