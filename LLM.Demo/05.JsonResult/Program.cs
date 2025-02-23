// See https://aka.ms/new-console-template for more information

using LLM.Demo.SeedWork.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Resources;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110




 const string TutorName = "Tutor";
 const string TutorInstructions =
    """
    Think step-by-step and rate the user input on creativity and expressiveness from 1-100.

    Respond in JSON format with the following JSON schema:

    {
        "score": "integer (1-100)",
        "notes": "the reason for your score"
    }
    """;

 while (true)
 {
     Console.WriteLine("Введите команду (1 - UseKernelFunctionStrategiesWithJsonResultAsync, 0 - exit):");
     string? command = Console.ReadLine()?.Trim();

     if (command == "0") break;
     if (command == "1") await UseKernelFunctionStrategiesWithJsonResultAsync();
     else Console.WriteLine("Неизвестная команда. Попробуйте снова.");
 }
 
 async Task UseKernelFunctionStrategiesWithJsonResultAsync()
 {
     
     
     
     ChatCompletionAgent agent =
         new()
         {
             Instructions = TutorInstructions,
             Name = TutorName,
             Kernel = KernelHelper.BuildKernelAndServiceProvider(),
         };
    
     AgentGroupChat chat =
         new()
         {
             ExecutionSettings =
                 new()
                 {
                     // Здесь используется подкласс TerminationStrategy, который завершит работу,
                      // если ответ содержит оценку, равную или превышающую 70.
                     TerminationStrategy = new ThresholdTerminationStrategy()
                 }
         };
     
     await InvokeAgentAsync("The sunset is very colorful.");
     await InvokeAgentAsync("The sunset is setting over the mountains.");
     await InvokeAgentAsync("The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze.");
     await InvokeAgentAsync("The sunset is setting over the mountains and filled the sky with a deep red flame, setting the clouds ablaze. It`s so wonderfull");

     async Task InvokeAgentAsync(string input)
     {
         ChatMessageContent message = new(AuthorRole.User, input);
         chat.AddChatMessage(message);
         AgentHelper.WriteAgentChatMessage(message);

         await foreach (ChatMessageContent response in chat.InvokeAsync(agent))
         {
             await Task.Delay(100);
             AgentHelper.WriteAgentChatMessage(response);
             
         }
         
         await Task.Delay(100); // Ждем немного, чтобы чат обновился

         Console.WriteLine($"[AFTER TERMINATION CHECK: IS COMPLETED = {chat.IsComplete}]");
     }

 }
 

 record struct WritingScore(int score, string notes);

class ThresholdTerminationStrategy : TerminationStrategy
 {
     const int ScoreCompletionThreshold = 70;
     
     protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
     {
         string lastMessageContent = history[history.Count - 1].Content ?? string.Empty;

         WritingScore? result = JsonResultTranslator.Translate<WritingScore>(lastMessageContent);

         return Task.FromResult((result?.score ?? 0) >= ScoreCompletionThreshold);
     }
 }
 