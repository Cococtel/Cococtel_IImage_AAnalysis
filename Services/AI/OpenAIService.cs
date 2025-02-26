using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using ComputerVisionAPI.Network;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ComputerVisionAPI.Services.AI
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ComputerVisionSettings _settings;

        public OpenAIService(IHttpClientFactory clientFactory,IOptions<ComputerVisionSettings> options)
        {
            _clientFactory = clientFactory;
            _settings = options.Value;
        }

        public async Task<List<string>> ExtractTextFromImage([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return new List<string>();
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var byteData = memoryStream.ToArray();

            var operationLocation = await PostImageForAnalysis(byteData);
            if (string.IsNullOrEmpty(operationLocation))
            {
                return new List<string>();
            }

            var contentString = await GetAnalysisResult(operationLocation);
            if (string.IsNullOrEmpty(contentString))
            {
                return new List<string>();
            }

            var extractedText = ParseAnalysisResult(contentString);
            return extractedText;
        }

        private async Task<string?> PostImageForAnalysis(byte[] byteData)
        {
            using var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.SubscriptionKey);

            using var content = new ByteArrayContent(byteData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync(_settings.UriBase, content);

            if (!response.IsSuccessStatusCode) return null;

            return response.Headers.GetValues("Operation-Location")?.FirstOrDefault();
        }

        private async Task<string?> GetAnalysisResult(string operationLocation)
        {
            using var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.SubscriptionKey);

            int retries = 0;
            string contentString;

            do
            {
                await Task.Delay(1000);
                var response = await client.GetAsync(operationLocation);
                contentString = await response.Content.ReadAsStringAsync();
                retries++;
            }
            while (retries < 60 && !contentString.Contains("\"status\":\"succeeded\""));

            if (retries == 60 && !contentString.Contains("\"status\":\"succeeded\""))
            {
                Console.WriteLine("\nTimeout error.\n");
                return null;
            }

            return contentString;
        }

        private List<string> ParseAnalysisResult(string contentString)
        {
            var analysis = JToken.Parse(contentString);
            var analyzeResult = analysis["analyzeResult"];
            var messages = new List<string>();

            if (analyzeResult?["readResults"] != null)
            {
                foreach (var result in analyzeResult["readResults"]!)
                {
                    foreach (var line in result["lines"]!)
                    {
                        var text = line["text"]?.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            messages.Add(text);
                        }
                    }
                }
            }

            return messages;
        }
    }
}