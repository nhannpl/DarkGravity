# Secrets Management Guide

This document provides instructions on how to manage sensitive API keys and configuration secrets for the **DarkGravity** project.

## 🔐 .NET User Secrets
We use the `.NET User Secrets` manager for local development. This keeps keys out of the source code and prevents accidental commits to Git.

### Required Keys
The following keys are utilized by the `StoryAnalyzer` service ([StoryAnalyzer.cs](../src/Crawler/Services/StoryAnalyzer.cs)):

| Key | Service | Purpose |
| :--- | :--- | :--- |
| `GEMINI_API_KEY` | Google Gemini | Primary AI analysis |
| `OPENAI_API_KEY` | OpenAI | GPT-4o analysis |
| `DEEPSEEK_API_KEY` | DeepSeek | Alternative LLM |
| `MISTRAL_API_KEY` | Mistral AI | Alternative LLM |
| `HUGGINGFACE_API_KEY` | Hugging Face | Local/Open-weight models |
| `OPENROUTER_API_KEY` | OpenRouter | Multi-model gateway |
| `CLOUDFLARE_API_TOKEN` | Cloudflare Workers AI | Serverless AI |
| `CLOUDFLARE_ACCOUNT_ID` | Cloudflare | Account context |

---

## 🛠️ Management Commands

Run these commands from the project root.

### 1. View Current Secrets
To see which keys are currently configured (values will be shown):
```bash
dotnet user-secrets list --project src/Crawler
```

### 2. Set or Update a Secret
Replace `your_key_here` with your actual API key:
```bash
# Example for OpenAI
dotnet user-secrets set "OPENAI_API_KEY" "your_key_here" --project src/Crawler

# Multi-provider shortcuts
dotnet user-secrets set "GEMINI_API_KEY" "..." --project src/Crawler
dotnet user-secrets set "DEEPSEEK_API_KEY" "..." --project src/Crawler
dotnet user-secrets set "MISTRAL_API_KEY" "..." --project src/Crawler
dotnet user-secrets set "HUGGINGFACE_API_KEY" "..." --project src/Crawler
dotnet user-secrets set "OPENROUTER_API_KEY" "..." --project src/Crawler
dotnet user-secrets set "CLOUDFLARE_API_TOKEN" "..." --project src/Crawler
dotnet user-secrets set "CLOUDFLARE_ACCOUNT_ID" "..." --project src/Crawler
```

### 3. Remove a Secret
```bash
dotnet user-secrets remove "KEY_NAME" --project src/Crawler
```

---

## 📂 Physical Location (macOS)
The secrets are stored in a JSON file outside of your project directory:
`~/.microsoft/usersecrets/a7205fee-b873-4d14-abcd-51d5ce9baa1d/secrets.json`

## 🔗 References
- [Official Microsoft Docs: User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [StoryAnalyzer.cs Source](../src/Crawler/Services/StoryAnalyzer.cs)
