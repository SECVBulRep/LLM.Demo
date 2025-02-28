using System.ComponentModel;
using System.Text.Json.Serialization;
using LLM.Demo.SeedWork.Core;
using Microsoft.OpenApi.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
#pragma warning disable SKEXP0070

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052


var kernelBuilder = KernelHelper.CreateBuilder();
kernelBuilder.Plugins.AddFromType<TimeInformation>();
kernelBuilder.Plugins.AddFromType<WidgetFactory>();

var kernel = kernelBuilder.Build();
kernel.ImportPluginFromType<WeatherPlugin1>();




while (true)
{
    Console.WriteLine("Select an example to run:");
    Console.WriteLine(
        "1. Invoke the kernel with a prompt that asks the AI for information it cannot provide and may hallucinate");
    Console.WriteLine("2. Invoke the kernel with a templated prompt that invokes a plugin and display the result");
    Console.WriteLine("3. Invoke the kernel with a prompt and allow the AI to automatically invoke functions");
    Console.WriteLine(
        "4. Invoke the kernel with a prompt and allow the AI to automatically invoke functions that use enumerations");
    Console.WriteLine("5. What is the current weather?");
    Console.Write("Enter your choice: ");
    
    
    
    var choice = Console.ReadLine();
    if (choice == "0") break;

    OpenAIPromptExecutionSettings settings;
    settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
    
    // settings = new()
    // {
    //     ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    // };

    switch (choice)
    {
        case "1":
            Console.WriteLine(await kernel.InvokePromptAsync("How many days until Christmas?"));
            break;
        case "2":
            Console.WriteLine(await kernel.InvokePromptAsync(
                "The current time is {{TimeInformation.GetCurrentUtcTime}}. How many days until Christmas?"));
            break;
        case "3":
          
            Console.WriteLine(await kernel.InvokePromptAsync("How many days until Christmas?", new(settings)));
            break;
        case "4":
           
            Console.WriteLine(await kernel.InvokePromptAsync("Create a handy lime colored widget for me.",
                new(settings)));
            Console.WriteLine(await kernel.InvokePromptAsync("Create a beautiful scarlet colored widget for me.",
                new(settings)));
            break;
        
        case "5":
           
            FunctionResult result = await kernel.InvokePromptAsync("What is the current weather?", new(settings));
            Console.WriteLine(result);
            break;

        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}


sealed class WeatherPlugin1
{
    [KernelFunction]
    [Description("Returns current weather: Data1 - Temperature (°C), Data2 - Humidity (%), Data3 - Dew Point (°C), Data4 - Wind Speed (km/h)")]
    public WeatherData GetWeatherData()
    {
        return new WeatherData()
        {
            Data1 = 35.0,  // Temperature in degrees Celsius  
            Data2 = 20.0,  // Humidity in percentage  
            Data3 = 10.0,  // Dew point in degrees Celsius  
            Data4 = 15.0   // Wind speed in kilometers per hour
        };
    }
    public sealed class WeatherData
    {
        public double Data1 { get; set; }
        public double Data2 { get; set; }
        public double Data3 { get; set; }
        public double Data4 { get; set; }
    }
}


/// <summary>
/// A plugin that returns the current time.
/// </summary>
public class TimeInformation
{
    [KernelFunction]
    [Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime()
    {
        return DateTime.UtcNow.ToString("R");
    }
}

/// <summary>
/// A plugin that creates widgets.
/// </summary>
public class WidgetFactory
{
    [KernelFunction]
    [Description("Creates a new widget of the specified type and colors")]
    public WidgetDetails CreateWidget(
        [Description("The type of widget to be created")]
        WidgetType widgetType,
        [Description("The colors of the widget to be created")]
        WidgetColor[] widgetColors)
    {
        var colors = string.Join('-', widgetColors.Select(c => c.GetDisplayName()).ToArray());
        return new()
        {
            SerialNumber = $"{widgetType}-{colors}-{Guid.NewGuid()}",
            Type = widgetType,
            Colors = widgetColors
        };
    }
}

/// <summary>
/// A <see cref="JsonConverter"/> is required to correctly convert enum values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetType
{
    [Description("A widget that is useful.")]
    Useful,

    [Description("A widget that is decorative.")]
    Decorative
}

/// <summary>
/// A <see cref="JsonConverter"/> is required to correctly convert enum values.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WidgetColor
{
    [Description("Use when creating a red item.")]
    Red,

    [Description("Use when creating a green item.")]
    Green,

    [Description("Use when creating a blue item.")]
    Blue
}

public class WidgetDetails
{
    public string SerialNumber { get; init; }
    public WidgetType Type { get; init; }
    public WidgetColor[] Colors { get; init; }
}