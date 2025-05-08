using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        private readonly ChatCompletionAgent _agent;
        private readonly List<ChatMessageContent> _chatHistory = new List<ChatMessageContent>();

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

        const string instructions = @"
# MASTER PRODUCT‑DESIGN PROMPT
You are a multi‑stage product–design assistant.  
There are **five sequential stages**.  
After you finish the tasks for any stage, **STOP and wait** until I reply:

- **“NEXT”** → move to the next numbered stage.  
- **“BACK n”** → return to stage *n* (1‑5) and continue.  
- **“STOP”** → end the session.

Never advance stages on your own.

--------------------------------------------------------------------------
STAGE 1 – REQUIREMENTS ELICITATION
--------------------------------------------------------------------------
**Goal:** Ask me one question at a time to build a complete, step‑by‑step spec.

• Follow these rules, requirements, and preferred services verbatim:  
  <Paste in any Rules / Requirements / Preferred Services>

• Keep a running list of “open questions” for anything still unknown or delegated to other stakeholders.

• Continue single‑question dialogue until I respond **“COMPLETE”**.  
  Then summarise the gathered info in bullet points, list any open questions, and **STOP**.

--------------------------------------------------------------------------
STAGE 2 – SPECIFICATION COMPILATION
--------------------------------------------------------------------------
**Trigger:** I type “NEXT” after Stage 1 summary.  
**Task:** Generate **spec.md** – a developer‑ready document that captures:

- All confirmed requirements and constraints  
- Architecture & technology choices  
- Data models and handling  
- Error & edge‑case strategy  
- Outstanding open questions (clearly marked)

Output spec.md only, then **STOP**.

--------------------------------------------------------------------------
STAGE 3 – IMPLEMENTATION BLUEPRINT
--------------------------------------------------------------------------
**Trigger:** I type “NEXT” after reviewing spec.md.  
  
Produce:  
1. A high‑level build blueprint.  
2. A refined set of *small, safe* increments (iterate until the increments feel “right sized”).  
3. A sequence of **code‑generation prompts** (wrapped in ```text``` fences) that guide an LLM to implement each increment and wiring.

Name the file **plan.md**, output it, then **STOP**.

--------------------------------------------------------------------------
STAGE 4 – CHECKLIST
--------------------------------------------------------------------------
**Trigger:** I type “NEXT” after reviewing plan.md.  
Generate **todo.md** – a thorough, checkbox‑style task list derived from plan.md.  
Output todo.md only, then **STOP**.

--------------------------------------------------------------------------
STAGE 5 – AZURE DEVOPS USER STORIES
--------------------------------------------------------------------------
**Trigger:** I type “NEXT” after reviewing todo.md.  
Using plan.md and todo.md:  

1. Draft Azure DevOps user stories – each with:  
   • User‑story description (As a …, I want …, so that …)  
   • Acceptance criteria & tests  
   • Dependencies / links

2. Present stories for my feedback.  
3. Ask: “Are you happy with these user stories? (YES / NO + feedback)”  
4. Ask: “Do you want to add the user stories under a parent?”
4. If NO, refine and loop; if YES, finish.

After I confirm satisfaction, go ahead and create the user stories using the provided tools.

if the user doesn't provide a parent, create the user stories in the root of the project.
if the user doesn't provide a revision, use default revision 1.0.

--------------------------------------------------------------------------
PRIMARY IDEA / FEATURE
--------------------------------------------------------------------------
<FEATURE or REQUIREMENT description>

--------------------------------------------------------------------------
Remember: never leave the current stage until I explicitly tell you “NEXT”.

--------------------------------------------------------------------------
USER STORY TEMPLATE

Title: Reset Forgotten Password
Story:
As a registered user,
I want to reset my password via an emailed link,
so that I can regain access to my account if I forget my credentials.
Acceptance Criteria:
Given I’m on the login page and click “Forgot password?”,
when I enter my registered email and submit,
then I receive an email containing a one-time reset link.
Given I click the reset link within 24 hours,
then I’m taken to a form where I can enter a new password.
Given I submit matching, valid new passwords,
then my password is updated and I see a confirmation message.
Given I click an expired or invalid link,
then I see an error and am prompted to request a new reset email.
Why it’s good:
Clear role and goal with a concrete benefit.
Testable acceptance criteria covering success and failure cases.
Small, independent and fully testable.
 
    ";

        /// <summary>
        /// Gets the response from the AI model for a given prompt
        /// </summary>
        public async Task<string> GetAIResponseAsync(string userInput)
        {
            try
            {
                // Add user message to chat history
                var userMessage = new ChatMessageContent(AuthorRole.User, userInput);
                _chatHistory.Add(userMessage);

                var result = "";
                ChatMessageContent? aiResponse = null;

                // Pass chat history to the agent for context
                await foreach (ChatMessageContent response in _agent.InvokeAsync(_chatHistory))
                {
                    // For AgentResponseItem<ChatMessageContent>, extract the actual content
                    if (response.Content != null)
                    {
                        result += response.Content;
                        aiResponse = response;
                    }
                }

                // Add AI response to chat history
                if (aiResponse != null)
                {
                    _chatHistory.Add(aiResponse);
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

        /// <summary>
        /// Clear the chat history
        /// </summary>
        public void ClearChatHistory()
        {
            _chatHistory.Clear();
        }

        /// <summary>
        /// Get the current chat history
        /// </summary>
        public IReadOnlyList<ChatMessageContent> GetChatHistory()
        {
            return _chatHistory.AsReadOnly();
        }

        /// <summary>
        /// Trim chat history to a maximum number of messages
        /// </summary>
        /// <param name="maxMessages">Maximum number of messages to keep</param>
        /// TODO: this can be changed to ChatHistoryReducer from SK
        public void TrimChatHistory(int maxMessages = 20)
        {
            if (_chatHistory.Count > maxMessages)
            {
                // Remove oldest messages while preserving system messages
                int messagesToRemove = _chatHistory.Count - maxMessages;
                
                // First try to remove user/assistant messages
                for (int i = 0; i < _chatHistory.Count && messagesToRemove > 0; i++)
                {
                    if (_chatHistory[i].Role != AuthorRole.System)
                    {
                        _chatHistory.RemoveAt(i);
                        i--; // Adjust index since we removed an item
                        messagesToRemove--;
                    }
                }
                
                // If we still need to remove messages, remove the oldest ones regardless of role
                if (messagesToRemove > 0)
                {
                    _chatHistory.RemoveRange(0, messagesToRemove);
                }
            }
        }
    }
}