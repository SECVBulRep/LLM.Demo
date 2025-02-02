// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

var serviceProvider = BuildServiceProvider();
var kernel = serviceProvider.GetRequiredService<Kernel>();

KernelArguments arguments = new() { { "card_number", "4444 3333 2222 1111" } };

var result = await kernel.InvokePromptAsync("Tell me some useful information about this credit card number {{$card_number}}?", arguments);

Console.WriteLine(result);


ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    collection.AddSingleton<IPromptRenderFilter, PromptFilter>();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    kernelBuilder.Plugins.AddFromType<TimeInformation>();
    return collection.BuildServiceProvider();
}


sealed class TimeInformation
{
    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}


sealed class PromptFilter : IPromptRenderFilter
{
    private readonly ILogger _logger;

    public PromptFilter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PromptFilter>();
    }
    
   
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        if (context.Arguments.ContainsName("card_number"))
        {
            context.Arguments["card_number"] = "**** **** **** ****";
        }
        await next(context);
        context.RenderedPrompt += " NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY";
        _logger.LogInformation(context.RenderedPrompt);
    }
}