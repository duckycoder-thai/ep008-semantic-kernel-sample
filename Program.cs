using SemanticKernelSample.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelSample.Plugins;
using Microsoft.SemanticKernel;

namespace SemanticKernelSample;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<Worker>();
        builder.Services.AddOptions<OpenAIOptions>()
                        .Bind(builder.Configuration.GetSection("OpenAI"))
                        .ValidateDataAnnotations()
                        .ValidateOnStart();

        builder.Services.AddSingleton<IChatCompletionService>(sp =>
        {
            OpenAIOptions options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            return new OpenAIChatCompletionService(options.ChatModelId, options.ApiKey);
        });

        builder.Services.AddKeyedSingleton<MyLightPlugin>("Light1");
        builder.Services.AddKeyedSingleton<MyLightPlugin>("Light2");
        builder.Services.AddKeyedSingleton<MyLightPlugin>("Light3", (sp, key) =>
        {
            return new MyLightPlugin(true);
        });

        builder.Services.AddKeyedTransient<Kernel>("LightKernel", (sp, key) =>
        {
            KernelPluginCollection pluginCollection = new();
            pluginCollection.AddFromObject(
                sp.GetRequiredKeyedService<MyLightPlugin>("Light1"), "Light1");
            pluginCollection.AddFromObject(
                sp.GetRequiredKeyedService<MyLightPlugin>("Light2"), "Light2");
            pluginCollection.AddFromObject(
                sp.GetRequiredKeyedService<MyLightPlugin>("Light3"), "Light3");
            
            return new Kernel(sp, pluginCollection);
        });

        using IHost host = builder.Build();

        await host.RunAsync();
    }
}