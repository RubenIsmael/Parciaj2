using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using San_Agustin_Final.Models;
using San_Agustin_Final.Services.ChatBot;

namespace San_Agustin_Final.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly OpenAIService _openAIService;

        public ChatBotController(OpenAIService openAIService)
        {
            _openAIService = openAIService;
        }
        [HttpGet("test")]
        public ActionResult<string> TestEndpoint()
        {
            return Ok("El API del chatbot está funcionando correctamente");
        }
        [HttpPost]
        public async Task<ActionResult<ChatResponse>> PostMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest(new ChatResponse
                    {
                        Message = "El mensaje no puede estar vacío",
                        IsError = true
                    });
                }

                string response = await _openAIService.GetChatCompletionAsync(request.Message);

                return Ok(new ChatResponse
                {
                    Message = response,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // Registro detallado del error
                Console.WriteLine($"Error en ChatBotController: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new ChatResponse
                {
                    Message = $"Error: {ex.Message}",
                    IsError = true
                });
            }
        }
    }
}