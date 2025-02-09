// See https://aka.ms/new-console-template for more information

using System.Reflection;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using OpenAI.Chat;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

#pragma warning disable SKEXP0001
#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110

const string ParrotName = "Parrot";
const string ParrotInstructions = "Repeat the user message in the voice of a pirate and then end with a parrot sound.";

while (true)
{
    Console.WriteLine("Введите команду (1 - single, 2 - template, 0 - exit):");
    string? command = Console.ReadLine()?.Trim();

    if (command == "0") break;
    if (command == "1") await UseSingleChatCompletionAgentAsync();
    else if (command == "2") await UseTemplateForChatCompletionAgentAsync();
    else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
}

async Task UseSingleChatCompletionAgentAsync()
{
    var serviceProvider = BuildServiceProvider();
    var kernel = serviceProvider.GetRequiredService<Kernel>();

    var agentKernel = BuildServiceProvider().GetRequiredService<Kernel>();
    

    ChatCompletionAgent agent =
        new()
        {
            Name = ParrotName,
            Instructions = ParrotInstructions,
            Kernel = agentKernel
        };

    ChatHistory chat = [];


    await InvokeAgentAsync("Fortune favors the bold.");
    await InvokeAgentAsync("I came, I saw, I conquered.");
    await InvokeAgentAsync("Practice makes perfect.");


    async Task InvokeAgentAsync(string input)
    {
        ChatMessageContent message = new(AuthorRole.User, input);
        chat.Add(message);
        AgentHelper.WriteAgentChatMessage(message);

        await foreach (var response in agent.InvokeAsync(chat))
        {
            chat.Add(response);

            AgentHelper.WriteAgentChatMessage(response);
        }
    }
}



async Task UseTemplateForChatCompletionAgentAsync()
{
    string generateStoryYaml = EmbeddedResource.Read2("GenerateStory.yaml", Assembly.GetExecutingAssembly());
    PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(generateStoryYaml);
    
    var agentKernel = BuildServiceProvider().GetRequiredService<Kernel>();
    
    
    ChatCompletionAgent agent =
        new(templateConfig, new KernelPromptTemplateFactory())
        {
            Kernel = agentKernel,
            Arguments = new KernelArguments()
            {
                { "topic", "Dog" },
                { "length", "3" },
            }
        };
    
    ChatHistory chat = [];

   
    await InvokeAgentAsync();

    
    await InvokeAgentAsync(
        new()
        {
            { "topic", "Cat" },
            { "length", "3" },
        });

    
    async Task InvokeAgentAsync(KernelArguments? arguments = null)
    {
        await foreach (ChatMessageContent content in agent.InvokeAsync(chat, arguments))
        {
            chat.Add(content);
            AgentHelper.WriteAgentChatMessage(content);
        }
    }
}



ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    return collection.BuildServiceProvider();
}

