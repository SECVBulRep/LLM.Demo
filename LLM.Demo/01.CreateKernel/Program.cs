// See https://aka.ms/new-console-template for more information

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

Kernel kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(
        modelId: "phi-3.1-mini-128k-instruct", 
        apiKey: null, 
        endpoint: new Uri("http://localhost:1234/v1"))
    .Build();

while (true)
{
    Console.WriteLine("Select an example to run:");
    Console.WriteLine("1. Simple prompt");
    Console.WriteLine("2. Templated prompt");
    Console.WriteLine("3. Streamed response");
    Console.WriteLine("4. Prompt with execution settings");
    Console.WriteLine("5. JSON response");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    var choice = Console.ReadLine();
    if (choice == "0") break;

    switch (choice)
    {
        case "1":
            Console.WriteLine(await kernel.InvokePromptAsync("What color is the sky?"));
            Console.WriteLine();
            break;

        case "2":
            KernelArguments arguments = new() { { "topic", "sea" } };
            Console.WriteLine(await kernel.InvokePromptAsync("What color is the {{$topic}}?", arguments));
            Console.WriteLine();
            break;

        case "3":
            arguments = new() { { "topic", "sea" } };
            await foreach (var update in kernel.InvokePromptStreamingAsync(
                               "What color is the {{$topic}}? Provide a detailed explanation.", arguments))
            {
                Console.Write(update);
            }
            Console.WriteLine();
            break;

        case "4":
            arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.5 }) { { "topic", "dogs" } };
            Console.WriteLine(await kernel.InvokePromptAsync("Tell me a story about {{$topic}}", arguments));
            Console.WriteLine();
            break;

        case "5":
            arguments = new(new OpenAIPromptExecutionSettings
            {
                //ResponseFormat = "json_object"
            }) { { "topic", "chocolate" } };
            Console.WriteLine(await kernel.InvokePromptAsync("Create a recipe for a {{$topic}} cake in JSON format", arguments));
            Console.WriteLine();
            break;

        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}
