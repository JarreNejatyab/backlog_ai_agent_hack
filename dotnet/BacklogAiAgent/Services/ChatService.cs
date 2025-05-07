using System;
using System.Threading.Tasks;
using BacklogAiAgent.Services;

namespace BacklogAiAgent.Services
{
    /// <summary>
    /// Service for handling chat interactions with the user
    /// </summary>
    public class ChatService
    {
        private readonly AIService _aiService;

        public ChatService(AIService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Run the main chat loop for the AI agent
        /// </summary>
        public async Task RunChatLoopAsync()
        {
            Console.WriteLine("\n=== Backlog AI Assistant ===");
            Console.WriteLine("Let's chat about a feature. Type 'exit' to quit.");
            
            // Check if AI service is configured properly
            if (!_aiService.IsConfigured())
            {
                Console.WriteLine("Error: Could not find chat completion service.");
                return;
            }
            
            while (true)
            {
                Console.Write("\nYour question: ");
                string? userInput = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit")
                {
                    break;
                }
                
                try
                {
                    // Get AI response
                    string response = await _aiService.GetAIResponseAsync(userInput);
                    
                    Console.WriteLine("\nAI Assistant: " + response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }
            }
        }   
    }
}