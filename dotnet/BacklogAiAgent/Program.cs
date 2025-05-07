using System;
using System.Threading.Tasks;
using BacklogAiAgent.Config;
using BacklogAiAgent.Services;


Console.WriteLine("Starting Backlog AI Agent...");

try
{
    // Initialize configuration
    var config = new ConfigurationManager();

    // Initialize services
    var aiService = new AIService(config);

    var chatService = new ChatService(aiService);

    // Run chat loop
    await chatService.RunChatLoopAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
