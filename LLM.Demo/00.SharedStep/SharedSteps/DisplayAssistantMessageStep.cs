// Copyright (c) Microsoft. All rights reserved.

using _00.SharedStep.Events;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0080

namespace _00.SharedStep.SharedSteps;

/// <summary>
/// Step used in the Processes Samples:
/// - Step_02_AccountOpening.cs
/// </summary>
public class DisplayAssistantMessageStep : KernelProcessStep
{
    public static class Functions
    {
        public const string DisplayAssistantMessage = nameof(DisplayAssistantMessage);
    }

    [KernelFunction(Functions.DisplayAssistantMessage)]
    public async ValueTask DisplayAssistantMessageAsync(KernelProcessStepContext context, string assistantMessage)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ASSISTANT: {assistantMessage}\n");
        Console.ResetColor();

        // Emit the assistantMessageGenerated
        await context.EmitEventAsync(new() { Id = CommonEvents.AssistantResponseGenerated, Data = assistantMessage });
    }
}
