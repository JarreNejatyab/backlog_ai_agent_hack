using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;

namespace BacklogAiAgent.Services.Plugins
{
    /// <summary>
    /// Plugin for creating work items in Azure DevOps
    /// </summary>
    public class WorkItemPlugin
    {
        private readonly HttpClient _httpClient;
        private readonly string _organizationUrl;
        private readonly string _project;
        private readonly string _personalAccessToken;

        public WorkItemPlugin(string organizationUrl, string project, string personalAccessToken)
        {
            _httpClient = new HttpClient();
            _organizationUrl = organizationUrl;
            _project = project;
            _personalAccessToken = personalAccessToken;

            // Configure HTTP client with authorization header for Azure DevOps API
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [KernelFunction("CreateWorkItem")]
        [Description("Creates a new work item in Azure DevOps")]
        public async Task<string> CreateWorkItemAsync(
            [Description("The type of work item to create (e.g., Bug, Task, User Story)")] string workItemType,
            [Description("The title of the work item")] string title,
            [Description("The description of the work item")] string description = "",
            [Description("Priority of the work item (e.g., 1, 2, 3)")] int priority = 2,
            [Description("Assigned to (email or display name)")] string assignedTo = "")
        {
            // Create the URL for the Azure DevOps API
            // Format: https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${type}?api-version=7.1
            var apiUrl = $"{_organizationUrl}/{_project}/_apis/wit/workitems/${workItemType}?api-version=7.1";

            try
            {
                // Create JSON patch document for the work item fields
                var patchDocument = new List<JsonPatchOperation>();

                // Add required fields
                patchDocument.Add(new JsonPatchOperation
                {
                    Op = "add",
                    Path = "/fields/System.Title",
                    Value = title
                });

                // Add optional fields if provided
                if (!string.IsNullOrEmpty(description))
                {
                    patchDocument.Add(new JsonPatchOperation
                    {
                        Op = "add",
                        Path = "/fields/System.Description",
                        Value = description
                    });
                }

                if (priority > 0)
                {
                    patchDocument.Add(new JsonPatchOperation
                    {
                        Op = "add",
                        Path = "/fields/Microsoft.VSTS.Common.Priority",
                        Value = priority
                    });
                }

                if (!string.IsNullOrEmpty(assignedTo))
                {
                    patchDocument.Add(new JsonPatchOperation
                    {
                        Op = "add",
                        Path = "/fields/System.AssignedTo",
                        Value = assignedTo
                    });
                }

                // Serialize the patch document
                var content = new StringContent(
                    JsonSerializer.Serialize(patchDocument),    
                    Encoding.UTF8,
                    "application/json-patch+json");

                // Send the POST request to create the work item
                var response = await _httpClient.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to get the work item details
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var workItem = JsonSerializer.Deserialize<WorkItemResponse>(jsonResponse);
                    
                    return $"Work item created: ID {workItem.Id}, URL: {workItem.Url}";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Failed to create work item. Error: {response.StatusCode}, {errorContent}";
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"An error occurred creating work item: {ex.Message}");
                return $"Error creating work item: {ex.Message}";
            }
        }

        /// <summary>
        /// Represents a JSON Patch Operation for Azure DevOps API
        /// </summary>
        private class JsonPatchOperation
        {
            [JsonPropertyName("op")]
            public string Op { get; set; }

            [JsonPropertyName("path")]
            public string Path { get; set; }

            [JsonPropertyName("value")]
            public object Value { get; set; }

            [JsonPropertyName("from")]
            public string From { get; set; }
        }

        /// <summary>
        /// Response model for Work Item creation
        /// </summary>
        private class WorkItemResponse
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("rev")]
            public int Rev { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}