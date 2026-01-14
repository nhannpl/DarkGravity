using Shared.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;

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

    public async Task<(string Analysis, double? Score)> AnalyzeAsync(Story story)
    {
        string analysis = "No analysis";
        
        // 1. Try Gemini (Direct HTTP)
        if (!string.IsNullOrEmpty(_geminiKey))
        {
            analysis = await AnalyzeWithGeminiDirectAsync(story);
        }
        // 2. Try OpenAI (Semantic Kernel)
        else if (_openaiKernel != null)
        {
            try
            {
                var prompt = $"Analyze this horror story. 1. Is it a Ghost, Slasher, or Monster story? 2. Give it a 'Scary Score' from 1-10.\n\nTitle: {story.Title}\nBody: {story.BodyText.Substring(0, Math.Min(300, story.BodyText.Length))}...";
                var result = await _openaiKernel.InvokePromptAsync(prompt);
                analysis = result.ToString();
            }
            catch (Exception ex)
            {
                analysis = $"OpenAI Error: {ex.Message}";
            }
        }
        else
        {
            // 3. Fallback
            analysis = "MOCK ANALYSIS: Very scary! (Score: 8.0/10)";
        }

        // Parse Score
        var score = ParseScore(analysis);
        return (analysis, score);
    }

    private async Task<string> AnalyzeWithGeminiDirectAsync(Story story)
    {
        try
        {
            // Using gemini-3-flash-preview as before
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";

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

    private double? ParseScore(string text)
    {
        // Look for "Score: 7.5/10" or "Score: 7.5" or "7.5/10"
        var match = Regex.Match(text, @"Score[:\s]*(\d+(\.\d+)?)", RegexOptions.IgnoreCase);
        if (match.Success && double.TryParse(match.Groups[1].Value, out double score))
        {
            return score;
        }
        return null;
    }
}
