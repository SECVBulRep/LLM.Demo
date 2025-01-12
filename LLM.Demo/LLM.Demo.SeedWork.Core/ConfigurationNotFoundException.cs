using Microsoft.SemanticKernel;

namespace LLM.Demo.SeedWork.Core;

public sealed class ConfigurationNotFoundException : Exception
{
    public string? Section { get; }
    public string? Key { get; }

    public ConfigurationNotFoundException(string section, string key)
        : base($"Configuration key '{section}:{key}' not found")
    {
        this.Section = section;
        this.Key = key;
    }

    public ConfigurationNotFoundException(string section)
        : base($"Configuration section '{section}' not found")
    {
        this.Section = section;
    }

    public ConfigurationNotFoundException() : base()
    {
    }

    public ConfigurationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public static class PromptYamlKernelExtensions
{
    /// <summary>
    /// Creates a <see cref="KernelFunction"/> instance for a prompt function using the specified YAML.
    /// </summary>
    /// <param name="kernel">The <see cref="Kernel"/> containing services, plugins, and other state for use throughout the operation.</param>
    /// <param name="text">YAML representation of the <see cref="PromptTemplateConfig"/> to use to create the prompt function</param>
    /// <param name="promptTemplateFactory">
    /// The <see cref="IPromptTemplateFactory"/> to use when interpreting the prompt template configuration into a <see cref="IPromptTemplate"/>.
    /// If null, a default factory will be used.
    /// </param>
    /// <returns>The created <see cref="KernelFunction"/>.</returns>
    public static KernelFunction CreateFunctionFromPromptYaml(
        this Kernel kernel,
        string text,
        IPromptTemplateFactory? promptTemplateFactory = null)
    {
        return KernelFunctionYaml.FromPromptYaml(text, promptTemplateFactory, kernel.LoggerFactory);
    }
}

