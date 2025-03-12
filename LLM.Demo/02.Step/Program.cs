// See https://aka.ms/new-console-template for more information

using _00.SharedStep;
using _00.SharedStep.Events;
using _00.SharedStep.SharedSteps;
using LLM.Demo.SeedWork.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Process.Tools;

#pragma warning disable SKEXP0080


var kernelBuilder = KernelHelper.CreateBuilder();
var kernel = kernelBuilder.Build();


ProcessBuilder process = new("ChatBot");
var introStep = process.AddStepFromType<IntroStep>();
var userInputStep = process.AddStepFromType<ChatUserInputStep>();
var responseStep = process.AddStepFromType<ChatBotResponseStep>();


process
    .OnInputEvent(ChatBotEvents.StartProcess)
    .SendEventTo(new ProcessFunctionTargetBuilder(introStep));


introStep
    .OnFunctionResult()
    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));


userInputStep
    .OnEvent(ChatBotEvents.Exit)
    .StopProcess();


userInputStep
    .OnEvent(CommonEvents.UserInputReceived)
    .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));


responseStep
    .OnEvent(ChatBotEvents.AssistantResponseGenerated)
    .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));


KernelProcess kernelProcess = process.Build();


using var runningProcess =
    await kernelProcess.StartAsync(kernel, new KernelProcessEvent() { Id = ChatBotEvents.StartProcess, Data = null });


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
        state.UserInputs.Add("How are you?");
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
            await context.EmitEventAsync(new KernelProcessEvent { Id = ChatBotEvents.Exit, Data = userMessage });
            return;
        }

        // emitting userInputReceived event
        await context.EmitEventAsync(new KernelProcessEvent
            { Id = CommonEvents.UserInputReceived, Data = userMessage });
    }
}

sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
{
    public static class Functions
    {
        public const string GetChatResponse = nameof(GetChatResponse);
    }

    internal ChatBotState? _state;


    public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    [KernelFunction(Functions.GetChatResponse)]
    public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
    {
        _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response =
            await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false);
        if (response == null)
        {
            throw new InvalidOperationException("Failed to get a response from the chat completion service.");
        }

        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"ASSISTANT: {response.Content}");
        System.Console.ResetColor();

        _state.ChatMessages.Add(response);

        await context.EmitEventAsync(new KernelProcessEvent
            { Id = ChatBotEvents.AssistantResponseGenerated, Data = response });
    }
}


sealed class ChatBotState
{
    internal ChatHistory ChatMessages { get; } = new();
}

static class ChatBotEvents
{
    public const string StartProcess = "startProcess";
    public const string IntroComplete = "introComplete";
    public const string AssistantResponseGenerated = "assistantResponseGenerated";
    public const string Exit = "exit";
}