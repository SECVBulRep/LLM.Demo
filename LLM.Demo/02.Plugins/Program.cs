// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
#pragma warning disable SKEXP0110

const string HostName = "Host";
const string HostInstructions = "Answer questions about the menu.";

while (true)
{
    Console.WriteLine("Введите команду (1 - UseChatCompletionWithPluginAgentAsync, 0 - exit):");
    string? command = Console.ReadLine()?.Trim();

    if (command == "0") break;
    if (command == "1") await UseChatCompletionWithPluginAgentAsync();
    else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
}


async Task UseChatCompletionWithPluginAgentAsync()
{
    var agentKernel = BuildServiceProvider().GetRequiredService<Kernel>();
    
    ChatCompletionAgent agent =
        new()
        {
            Instructions = HostInstructions,
            Name = HostName,
            Kernel = agentKernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
        };
    
    
    KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
    agent.Kernel.Plugins.Add(plugin);
    
    ChatHistory chat = [];

    // Respond to user input, invoking functions where appropriate.
    await InvokeAgentAsync("Hello");
    await InvokeAgentAsync("What is the special soup and it`s price?");
    await InvokeAgentAsync("What is the special drink?");
    await InvokeAgentAsync("Thank you");

    // Local function to invoke agent and display the conversation messages.
    async Task InvokeAgentAsync(string input)
    {
        ChatMessageContent message = new(AuthorRole.User, input);
        chat.Add(message);
        AgentHelper.WriteAgentChatMessage(message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
        {
            chat.Add(response);

            AgentHelper.WriteAgentChatMessage(response);
        }
    }
}

ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    return collection.BuildServiceProvider();
}

class MenuPlugin
{
    [KernelFunction, Description("Provides a list of specials from the menu.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Too smart")]
    public string GetSpecials() =>
        """
        Special Soup: Clam Chowder
        Special Salad: Cobb Salad
        Special Drink: Chai Tea
        """;

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice(
        [Description("The name of the menu item.")]
        string menuItem) =>
        "$9.99";
}