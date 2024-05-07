using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelSample;

internal sealed class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly Kernel _kernel;

    public Worker(IHostApplicationLifetime hostApplicationLifetime,
        [FromKeyedServices("LightKernel")] Kernel kernel)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _kernel = kernel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings promptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        Console.Write("> ");

        string? input = null;
        while ((input = Console.ReadLine()) != null)
        {
            Console.WriteLine();

            ChatMessageContent chatResult = await chatCompletionService
                .GetChatMessageContentAsync(
                    input, promptExecutionSettings, _kernel, stoppingToken);
            Console.Write($"\n>>> Results: {chatResult}\n\n> ");
        }

        _hostApplicationLifetime.StopApplication();
    }
}