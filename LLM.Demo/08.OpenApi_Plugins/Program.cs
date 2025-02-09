// See https://aka.ms/new-console-template for more information

using System.Reflection;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

//using Microsoft.SemanticKernel;


var serviceProvider = BuildServiceProvider();
var kernel = serviceProvider.GetRequiredService<Kernel>();

// var stream = EmbeddedResource.ReadStream("repair-service.json", Assembly.GetExecutingAssembly());
// var plugin = await kernel.ImportPluginFromOpenApiAsync("RepairService", stream!);

// просто вызов метода апи
// PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
// Console.WriteLine(await kernel.InvokePromptAsync("List all of the repairs .", new(settings)));




var stream = EmbeddedResource.ReadStream("repair-service.json", Assembly.GetExecutingAssembly());
var plugin = await kernel.CreatePluginFromOpenApiAsync("RepairService", stream!);
//вызов с трансформацией 
kernel.Plugins.Add(TransformPlugin(plugin));

PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
Console.WriteLine(await kernel.InvokePromptAsync("Create a repair:drain the old engine oil and replace it with fresh oil.", new(settings)));



ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    collection.AddSingleton<IMechanicService>(new FakeMechanicService());
    return collection.BuildServiceProvider();
}


static KernelPlugin TransformPlugin(KernelPlugin plugin)
{
    List<KernelFunction>? functions = [];

    foreach (KernelFunction function in plugin)
    {
        if (function.Name == "createRepair")
        {
            functions.Add(CreateRepairFunction(function));
        }
        else
        {
            functions.Add(function);
        }
    }

    return KernelPluginFactory.CreateFromFunctions(plugin.Name, plugin.Description, functions);
}


static KernelFunction CreateRepairFunction(KernelFunction function)
{
    var method = (
        Kernel kernel,
        KernelFunction currentFunction,
        KernelArguments arguments,
        [FromKernelServices] IMechanicService mechanicService,
        CancellationToken cancellationToken) =>
    {
        arguments.Add("assignedTo", mechanicService.GetMechanic());
        arguments.Add("date", DateTime.UtcNow.ToString("R"));

        return function.InvokeAsync(kernel, arguments, cancellationToken);
    };

    var options = new KernelFunctionFromMethodOptions()
    {
        FunctionName = function.Name,
        Description = function.Description,
        Parameters = function.Metadata.Parameters.Where(p => p.Name == "title" || p.Name == "description").ToList(),
        ReturnParameter = function.Metadata.ReturnParameter,
    };

    return KernelFunctionFactory.CreateFromMethod(method, options);
}


public interface IMechanicService
{
    string GetMechanic();
}


public class FakeMechanicService : IMechanicService
{
    public string GetMechanic() => "Bob";
}
