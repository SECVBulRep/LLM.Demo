// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0080

namespace _01.Step.Steps;

public sealed class LastStep : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("Step 4 - This is the Final Step...\n");
    }
}
