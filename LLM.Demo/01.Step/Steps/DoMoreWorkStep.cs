// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
#pragma warning disable SKEXP0080

namespace _01.Step.Steps;

public sealed class DoMoreWorkStep : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask ExecuteAsync(KernelProcessStepContext context)
    {
        Console.WriteLine("Step 3 - Doing Yet More Work...\n");
    }
}
