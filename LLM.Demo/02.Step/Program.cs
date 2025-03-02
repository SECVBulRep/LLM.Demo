// See https://aka.ms/new-console-template for more information

using _00.SharedStep.Events;
using _00.SharedStep.SharedSteps;
using LLM.Demo.SeedWork.Core;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0080


var kernelBuilder = KernelHelper.CreateBuilder();
var kernel = kernelBuilder.Build();


ProcessBuilder process = new("ChatBot");
var introStep = process.AddStepFromType<IntroStep>();


internal sealed class IntroStep : KernelProcessStep
{
    [KernelFunction]
    public void PrintIntroMessage()
    {
        Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
    }
}


internal sealed class ChatUserInputStep : ScriptedUserInputStep
{
    public override void PopulateUserInputs(UserInputState state)
    {
        state.UserInputs.Add("Hello");
        state.UserInputs.Add("How tall is the tallest mountain?");
        state.UserInputs.Add("How low is the lowest valley?");
        state.UserInputs.Add("How wide is the widest river?");
        state.UserInputs.Add("exit");
        state.UserInputs.Add("This text will be ignored because exit process condition was already met at this point.");
    }

    public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        var userMessage = GetNextUserMessage();

        if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
        {
            // exit condition met, emitting exit event
            await context.EmitEventAsync(new KernelProcessEvent { Id = ChatBotEvents.Exit, Data = userMessage });
            return;
        }

        // emitting userInputReceived event
        await context.EmitEventAsync(new KernelProcessEvent
            { Id = CommonEvents.UserInputReceived, Data = userMessage });
    }
}

static class ChatBotEvents
{
    public const string StartProcess = "startProcess";
    public const string IntroComplete = "introComplete";
    public const string AssistantResponseGenerated = "assistantResponseGenerated";
    public const string Exit = "exit";
}