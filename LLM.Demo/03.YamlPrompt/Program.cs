// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text;
using LLM.Demo.SeedWork.Core;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052


var info = Assembly.GetExecutingAssembly().GetName();

var name = info.Name;
await using var stream = Assembly
    .GetExecutingAssembly()
    .GetManifestResourceStream($"_{name}.Resources.generateStory.yaml")!;
using var streamReader = new StreamReader(stream, Encoding.UTF8);
var generateStoryYaml =  streamReader.ReadToEnd();


var kernelBuilder = KernelHelper.CreateBuilder();
var kernel = kernelBuilder.Build();

// Load prompt from resource
var function = kernel.CreateFunctionFromPromptYaml(generateStoryYaml);

// Invoke the prompt function and display the result
Console.WriteLine(await kernel.InvokeAsync(function, arguments: new()
{
    { "topic", "Dog" },
    { "length", "3" },
}));