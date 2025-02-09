using System.Reflection;

namespace LLM.Demo.SeedWork.Core;

/// <summary>
/// Resource helper to load resources embedded in the assembly. By default we embed only
/// text files, so the helper is limited to returning text.
///
/// You can find information about embedded resources here:
/// * https://learn.microsoft.com/dotnet/core/extensions/create-resource-files
/// * https://learn.microsoft.com/dotnet/api/system.reflection.assembly.getmanifestresourcestream?view=net-7.0
///
/// To know which resources are embedded, check the csproj file.
/// </summary>
public static class EmbeddedResource
{
    private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;

    public static string Read(string fileName,Assembly assembly)
    {
        // Get the current assembly. Note: this class is in the same assembly where the embedded resources are stored.
        // Assembly assembly =
        //     typeof(EmbeddedResource).GetTypeInfo().Assembly ??
        //     throw new ConfigurationNotFoundException($"[{s_namespace}] {fileName} assembly not found");

        // Resources are mapped like types, using the namespace and appending "." (dot) and the file name
        var resourceName = $"{assembly.GetName().Name}." + fileName;
        using Stream resource =
            assembly.GetManifestResourceStream(resourceName) ??
            throw new ConfigurationNotFoundException($"{resourceName} resource not found");

        // Return the resource content, in text format.
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    public static Stream? ReadStream(string fileName, Assembly assembly)
    {

        //var assembly = Assembly.GetExecutingAssembly();
        // Get the current assembly. Note: this class is in the same assembly where the embedded resources are stored.
        // Assembly assembly =
        //     typeof(EmbeddedResource).GetTypeInfo().Assembly ??
        //     throw new ConfigurationNotFoundException($"[{s_namespace}] {fileName} assembly not found");

        // Resources are mapped like types, using the namespace and appending "." (dot) and the file name
        //var resourceName = $"{s_namespace}." + fileName;
        //var resourceName = $"_{assembly.GetName().Name}." + fileName;

        //var t = assembly.GetManifestResourceNames();
        return assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Where(n => n.EndsWith(fileName)).FirstOrDefault());
    }

    internal static async Task<ReadOnlyMemory<byte>> ReadAllAsync(string fileName, Assembly assembly)
    {
        await using Stream? resourceStream = ReadStream(fileName,assembly);
        using var memoryStream = new MemoryStream();

        // Copy the resource stream to the memory stream
        await resourceStream!.CopyToAsync(memoryStream);

        // Convert the memory stream's buffer to ReadOnlyMemory<byte>
        // Note: ToArray() creates a copy of the buffer, which is fine for converting to ReadOnlyMemory<byte>
        return new ReadOnlyMemory<byte>(memoryStream.ToArray());
    }
}