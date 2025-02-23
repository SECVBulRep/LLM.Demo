// See https://aka.ms/new-console-template for more information

using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110

// пример окогда решением и стратгией управляет тоже ИИ


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
    Never delimit the response with quotation marks.
    You're laser focused on the goal at hand.
    Don't waste time with chit chat.
    Consider suggestions when refining an idea.
    """;


while (true)
{
    Console.WriteLine("Введите команду (1 - UseKernelFunctionStrategiesWithAgentGroupChatAsync, 0 - exit):");
    string? command = Console.ReadLine()?.Trim();

    if (command == "0") break;
    if (command == "1") await UseKernelFunctionStrategiesWithAgentGroupChatAsync();
    else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
}

async Task UseKernelFunctionStrategiesWithAgentGroupChatAsync()
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

    // задаем условие терминации
    KernelFunction terminationFunction =
        AgentGroupChat.CreatePromptFunctionForStrategy(
            """
            Determine if the copy has been approved.  If so, respond with a single word: yes

            History:
            {{$history}}
            """,
            safeParameterNames: "history");
    
    
    // задааем стратегию выбора ответов 
    KernelFunction selectionFunction =
        AgentGroupChat.CreatePromptFunctionForStrategy(
            $$$"""
               Determine which participant takes the next turn in a conversation based on the the most recent participant.
               State only the name of the participant to take the next turn.
               No participant should take more than one turn in a row.

               Choose only from these participants:
               - {{{ReviewerName}}}
               - {{{CopyWriterName}}}

               Always follow these rules when selecting the next participant:
               - After {{{CopyWriterName}}}, it is {{{ReviewerName}}}'s turn.
               - After {{{ReviewerName}}}, it is {{{CopyWriterName}}}'s turn.

               History:
               {{$history}}
               """,
            safeParameterNames: "history");
    
    
       // Ограничьте историю, используемую для выбора и завершения, только самым последним сообщением.
        ChatHistoryTruncationReducer strategyReducer = new(1);

       
        AgentGroupChat chat =
            new(agentWriter, agentReviewer)
            {
                ExecutionSettings =
                    new()
                    {
                        // Здесь KernelFunctionTerminationStrategy завершится,
                        // когда арт-директор даст своё одобрение.
                        TerminationStrategy =
                            new KernelFunctionTerminationStrategy(terminationFunction, BuildServiceProvider().GetRequiredService<Kernel>())
                            {
                                // Only the art-director may approve.
                                Agents = [agentReviewer],
                              
                                ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                                HistoryVariableName = "history",
                                // Limit total number of turns
                                MaximumIterations = 10,
                                HistoryReducer = strategyReducer,
                            },
                        
                        // Здесь KernelFunctionSelectionStrategy выбирает агентов на основе функции запроса.
                        SelectionStrategy =
                            new KernelFunctionSelectionStrategy(selectionFunction, BuildServiceProvider().GetRequiredService<Kernel>())
                            {
                                InitialAgent = agentWriter,
                                ResultParser = (result) => result.GetValue<string>() ?? CopyWriterName,
                                HistoryVariableName = "history",
                                HistoryReducer = strategyReducer,
                                EvaluateNameOnly = true,
                            },
                    }
            };
    
        
        ChatMessageContent message = new(AuthorRole.User, "concept: The bird is flying towards the sea.");
        chat.AddChatMessage(message);
        AgentHelper.WriteAgentChatMessage(message);

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