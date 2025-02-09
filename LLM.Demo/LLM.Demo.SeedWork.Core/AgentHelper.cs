using LLM.Demo.SeedWork.Core;
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
public class AgentHelper
{
    public static void WriteAgentChatMessage(ChatMessageContent message)
    {
        var authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";

        var contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
        var isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;
        var codeMarker = isCode ? "\n  [CODE]\n" : " ";
        Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");


        foreach (var item in message.Items)
            if (item is AnnotationContent annotation)
                Console.WriteLine($"  [{item.GetType().Name}] {annotation.Quote}: File #{annotation.FileId}");
            else if (item is FileReferenceContent fileReference)
                Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
            else if (item is ImageContent image)
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
            else if (item is FunctionCallContent functionCall)
                Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
            else if (item is FunctionResultContent functionResult)
                Console.WriteLine(
                    $"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}");

        if (message.Metadata?.TryGetValue("Usage", out var usage) ?? false)
        {
            if (usage is RunStepTokenUsage assistantUsage)
                WriteUsage(assistantUsage.TotalTokenCount, assistantUsage.InputTokenCount,
                    assistantUsage.OutputTokenCount);
            else if (usage is ChatTokenUsage chatUsage)
                WriteUsage(chatUsage.TotalTokenCount, chatUsage.InputTokenCount, chatUsage.OutputTokenCount);
        }

        void WriteUsage(int totalTokens, int inputTokens, int outputTokens)
        {
            Console.WriteLine($"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}");
        }
    }
}