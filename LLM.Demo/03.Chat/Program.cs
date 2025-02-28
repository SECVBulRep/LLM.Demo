// See https://aka.ms/new-console-template for more information

using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110

const string ReviewerName = "ArtDirector";
const string ReviewerInstructions =
    """
    You are an art director who has opinions about copywriting born of a love for Leonardo da Vinci.
    The goal is to determine if the given copy is acceptable to print.
    If so, state that it is approved.
    If not, provide insight on how to refine suggested copy without example.
    """;

const string CopyWriterName = "CopyWriter";
const string CopyWriterInstructions =
    """
    You are a copywriter with ten years of experience and are known for brevity and a dry humor.
    The goal is to refine and decide on the single best copy as an expert in the field.
    Only provide a single proposal per response.
    You're laser focused on the goal at hand.
    Don't waste time with chit chat.
    Consider suggestions when refining an idea.
    """;


while (true)
{
    Console.WriteLine("Введите команду (1 - UseAgentGroupChatWithTwoAgentsAsync, 0 - exit):");
    string? command = Console.ReadLine()?.Trim();

    if (command == "0") break;
    if (command == "1") await UseAgentGroupChatWithTwoAgentsAsync();
    else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
}

async Task UseAgentGroupChatWithTwoAgentsAsync()
{
    var reviewerKernel =BuildServiceProvider().GetRequiredService<Kernel>();
    
    ChatCompletionAgent agentReviewer =
        new()
        {
            Instructions = ReviewerInstructions,
            Name = ReviewerName,
            Kernel = reviewerKernel,
        };

    var copyWriterKernel  =  BuildServiceProvider().GetRequiredService<Kernel>();
    
    ChatCompletionAgent agentWriter =
        new()
        {
            Instructions = CopyWriterInstructions,
            Name = CopyWriterName,
            Kernel = copyWriterKernel,
        };
    
    
    AgentGroupChat chat =
        new(agentWriter, agentReviewer)
        {
            ExecutionSettings =
                new()
                {
                   
                    TerminationStrategy =
                        new ApprovalTerminationStrategy()
                        {
                            Agents = [agentReviewer],
                            MaximumIterations = 20,
                        }
                }
        };
 
    
    ChatMessageContent input = new(AuthorRole.User, "The bird is flying towards the sea");
    chat.AddChatMessage(input);
    AgentHelper.WriteAgentChatMessage(input);

    await foreach (ChatMessageContent response in chat.InvokeAsync())
    {
        AgentHelper.WriteAgentChatMessage(response);
    }

    Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
}

ServiceProvider BuildServiceProvider()
{
    var collection = new ServiceCollection();
    var kernelBuilder = collection.AddKernelAndAddOpenAIChatCompletion();
    return collection.BuildServiceProvider();
}

class ApprovalTerminationStrategy : TerminationStrategy
{
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
    {
        return Task.FromResult(history[history.Count - 1].Content
            ?.Contains("approv", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}