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


PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
Console.WriteLine(await kernel.InvokePromptAsync("Greet the current user by name and tell him current time ", new KernelArguments(settings)));


ServiceProvider BuildServiceProvider()
{
    var serilog = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
        .CreateLogger();
    var loggerFactory = new LoggerFactory().AddSerilog(serilog);

    var collection = new ServiceCollection();

    collection.AddSingleton<ILoggerFactory>(loggerFactory);
    collection.AddSingleton<IUserService>(new FakeUserService());

    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    
    kernelBuilder.Plugins.AddFromType<TimeInformation>();
    kernelBuilder.Plugins.AddFromType<UserInformation>();

    return collection.BuildServiceProvider();
}

Console.ReadKey();

public class TimeInformation
{
    private readonly ILogger _logger;

    public TimeInformation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TimeInformation>();
    }

    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime()
    {
        var utcNow = DateTime.UtcNow.ToString("R");
        _logger.LogInformation("Returning current time {0}", utcNow);
        return utcNow;
    }
}


public class UserInformation(IUserService userService)
{
    [KernelFunction]
    [Description("Retrieves the current users name.")]
    public string GetUsername()
    {
        return userService.GetCurrentUsername();
    }
}

public interface IUserService
{
    string GetCurrentUsername();
}

public class FakeUserService : IUserService
{
    public string GetCurrentUsername()
    {
        return "Bulat";
    }
}