using System;
using DotNetEnv;

namespace BacklogAiAgent.Config
{
    /// <summary>
    /// Manages application configuration settings from environment variables
    /// </summary>
    public class ConfigurationManager
    {
        // AI service settings
        public string OpenAIApiKey { get; }
        public string AzureOpenAIEndpoint { get; }
        public string AzureOpenAIApiKey { get; }
        public string AzureOpenAIDeploymentName { get; }
        public string AzureAISearchUri { get; }
        public string AzureAISearchKey { get; }
        public string AzureAISearchCollectionName { get; }
        
        // Azure DevOps settings
        public string AzureDevOpsOrganizationUrl { get; }
        public string AzureDevOpsProject { get; }
        public string AzureDevOpsPersonalAccessToken { get; }

        public ConfigurationManager()
        {
            // Load environment variables from .env file if it exists
            Env.Load();

            // OpenAI or Azure OpenAI settings
            OpenAIApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your_openai_api_key";
            AzureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "your_azure_openai_endpoint";
            AzureOpenAIApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "your_azure_openai_api_key";
            AzureOpenAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "your_azure_openai_deployment";

            // Ai Search 
            AzureAISearchUri = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") ?? "";
            AzureAISearchKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY") ?? "";
            AzureAISearchCollectionName = Environment.GetEnvironmentVariable("AZURE_SEARCH_COLLECTION_NAME") ?? "";
            
            // Azure DevOps settings
            AzureDevOpsOrganizationUrl = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG_URL") ?? "";
            AzureDevOpsProject = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PROJECT") ?? "";
            AzureDevOpsPersonalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? "";
        }

        /// <summary>
        /// Determines if Azure OpenAI should be used instead of OpenAI
        /// </summary>
        public bool ShouldUseAzureOpenAI()
        {
            return !string.IsNullOrEmpty(AzureOpenAIEndpoint) && !string.IsNullOrEmpty(AzureOpenAIApiKey);
        }
    }
}