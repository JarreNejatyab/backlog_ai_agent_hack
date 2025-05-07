namespace BacklogAiAgent.Models
{
    public class ChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}