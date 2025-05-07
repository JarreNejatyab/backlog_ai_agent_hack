using BacklogAiAgent.Models;
using BacklogAiAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace BacklogAiAgent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AIService _aiService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(AIService aiService, ILogger<ChatController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> PostAsync(ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponse
                {
                    Success = false,
                    ErrorMessage = "Message cannot be empty"
                });
            }

            try
            {
                var response = await _aiService.GetAIResponseAsync(request.Message);
                return Ok(new ChatResponse
                {
                    Message = response,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new ChatResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while processing your request"
                });
            }
        }
    }
}