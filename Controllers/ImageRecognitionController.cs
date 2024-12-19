using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComputerVisionAPI.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace ComputerVisionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageRecognitionController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;

        public ImageRecognitionController(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        [HttpPost]
        [Route("ExtractTextFromImage")]
        public async Task<IActionResult> ExtraxtTextFromIamge(string url)
        {
            var result = await _openAIService.ExtractTextFromImage(url);
            return Ok(result);
        }
    }
}