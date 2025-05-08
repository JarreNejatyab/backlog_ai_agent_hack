using System;
using System.Threading.Tasks;
using BacklogAiAgent.Config;
using BacklogAiAgent.Services.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Agents;
using Azure;

namespace BacklogAiAgent.Services
{
    /// <summary>
    /// Service for AI operations using Semantic Kernel
    /// </summary>
    public class AIService
    {
        private readonly BacklogAiAgent.Config.ConfigurationManager _config;
        private readonly Kernel _kernel;
        private readonly ChatCompletionAgent _agent ;

        public AIService(BacklogAiAgent.Config.ConfigurationManager config)
        {
            _config = config;
            _kernel = InitializeSemanticKernel();
            
            // Register knowlege search plugin
            // _kernel.Plugins.AddFromObject(new BacklogKnowlegePlugin(_kernel,_config.AzureAISearchCollectionName), "BacklogKnowlegePlugin");

            // Register work item plugin for Azure DevOps integration
            _kernel.Plugins.AddFromObject(new WorkItemPlugin(
                _config.AzureDevOpsOrganizationUrl,
                _config.AzureDevOpsProject,
                _config.AzureDevOpsPersonalAccessToken), 
                "WorkItemPlugin");

            _agent = new()
            {
                Name = "backlog-ai-assistant",
                Instructions = instructions,
                Kernel = _kernel,
                Arguments = // Specify the service-identifier via the KernelArguments
                new KernelArguments(
                    new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
            };

        }

        /// <summary>
        /// Initialize the Semantic Kernel with appropriate settings
        /// </summary>
        private Kernel InitializeSemanticKernel()
        {
            Console.WriteLine("Initializing Semantic Kernel...");
            
            var builder = Kernel.CreateBuilder();
            
            // Configure AI service - either Azure OpenAI or OpenAI
            if (_config.ShouldUseAzureOpenAI())
            {
                // Use Azure OpenAI
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: _config.AzureOpenAIDeploymentName,
                    endpoint: _config.AzureOpenAIEndpoint,
                    apiKey: _config.AzureOpenAIApiKey,
                    serviceId: "AzureOpenAIChatCompletion"
                );
            }
            else
            {
                // Use OpenAI
                builder.AddOpenAIChatCompletion(
                    modelId: "gpt-4",
                    apiKey: _config.OpenAIApiKey
                );
            }

            builder.Services.AddAzureAISearchVectorStore(new Uri(_config.AzureAISearchUri), new AzureKeyCredential(_config.AzureAISearchKey));

            return builder.Build();
        }

        const string instructions = "You are an ai assistant that helps users with backlog ideas:\n\n";

        /// <summary>
        /// Gets the response from the AI model for a given prompt
        /// </summary>
        public async Task<string> GetAIResponseAsync(string userInput)
        {
            try
            {

                var result = "";
                await foreach (ChatMessageContent response in _agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, userInput)))
                {
                    result += response.Content;
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating AI response: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Check if the AI service is properly configured
        /// </summary>
        public bool IsConfigured()
        {
            try
            {
                var openAIClient = _kernel.GetRequiredService<IChatCompletionService>();
                return openAIClient != null;
            }
            catch
            {
                return false;
            }
        }
    }
}