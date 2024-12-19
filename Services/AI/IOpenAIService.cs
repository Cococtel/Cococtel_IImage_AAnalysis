using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerVisionAPI.Services.AI
{
    public interface IOpenAIService
    {
        Task<List<string>> ExtractTextFromImage(string url);
    }
}