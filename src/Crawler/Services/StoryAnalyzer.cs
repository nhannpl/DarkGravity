using Crawler.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text;

namespace Crawler.Services;

public class StoryAnalyzer
{
    private readonly HttpClient _http;
    private readonly Kernel? _openaiKernel;
    private readonly string? _geminiKey;

    public StoryAnalyzer(HttpClient http)
    {
        _http = http;
        
        // Setup Keys
        _geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        string? openaiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        // Setup OpenAI (if available)
        if (!string.IsNullOrEmpty(openaiKey))
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion("gpt-4o", openaiKey);
            _openaiKernel = builder.Build();
        }
    }

    public async Task<string> AnalyzeAsync(Story story)
    {
        // 1. Try Gemini (Direct HTTP)
        if (!string.IsNullOrEmpty(_geminiKey))
        {
            return await AnalyzeWithGeminiDirectAsync(story);
        }

        // 2. Try OpenAI (Semantic Kernel)
        if (_openaiKernel != null)
        {
            try
            {
                var prompt = $"Analyze this horror story. Is it Ghost/Slasher/Monster? Score 1-10. Title: {story.Title}. Body: {story.BodyText.Substring(0, Math.Min(300, story.BodyText.Length))}";
                var result = await _openaiKernel.InvokePromptAsync(prompt);
                return result.ToString();
            }
            catch (Exception ex)
            {
                return $"OpenAI Error: {ex.Message}";
            }
        }

        // 3. Fallback
        return "MOCK ANALYSIS: Very scary! (Score: 8/10)";
    }

    private async Task<string> AnalyzeWithGeminiDirectAsync(Story story)
    {
        try
        {
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"; // Updated model name for stability
            
            // Note: Using gemini-3-flash-preview or similar if preferred, but gemini-pro is standard.
            // Original code used: gemini-3-flash-preview
            // I will stick to what was there or standard. User had gemini-3-flash-preview. 
            // I'll revert to that to be safe, or use the variable from previous file if I recall it.
            // Step 15: "gemini-3-flash-preview".
            url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";

            var payload = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = $"Analyze this horror story. 1. Is it a Ghost, Slasher, or Monster story? 2. Give it a 'Scary Score' from 1-10.\n\nTitle: {story.Title}\nBody: {story.BodyText.Substring(0, Math.Min(300, story.BodyText.Length))}..." }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", _geminiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                 return $"Gemini Error: {response.StatusCode} - {responseString}";
            }

            using var doc = JsonDocument.Parse(responseString);
            
            // Navigate JSON: candidates[0].content.parts[0].text
            // Add safety check
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                 var text = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                 return text?.Trim() ?? "No analysis returned.";
            }
            return "No candidates returned.";
        }
        catch (Exception ex)
        {
            return $"Gemini Exception: {ex.Message}";
        }
    }
}
