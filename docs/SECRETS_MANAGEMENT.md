# Secrets Management Guide

This document provides instructions on how to manage sensitive API keys and configuration secrets for the **DarkGravity** project.

## 🔐 .NET User Secrets
We use the `.NET User Secrets` manager for local development. This keeps keys out of the source code and prevents accidental commits of sensitive data.

**See [USER_SECRETS_EXAMPLE.md](./USER_SECRETS_EXAMPLE.md) for a quick-start template.**

### 1. Project: Analyzer (`src/Analyzer`)
Used for AI Ingestion and analysis.

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
| `DARKGRAVITY_DB_PASSWORD` | SQL Server | Database authentication |


### 2. Project: API (`src/Api`)
Used for serving the stories to the frontend.

| Key | Purpose | Default (Local) |
| :--- | :--- | :--- |
| `DARKGRAVITY_DB_PASSWORD` | SQL Server `sa` password | *Mandatory (via .env)* |

---

## 🌍 Environment Variables & Namespacing (Production)
For production environments or CI/CD pipelines, use namespaced environment variables. This prevents conflicts when multiple projects are hosted on the same infrastructure.

**Namespaced Keys:** 
- `DARKGRAVITY_DB_PASSWORD`: SQL Server `sa` password.
- `AllowedOrigins`: (JSON Array) List of domains permitted to access the API (CORS).

In your CI/CD (e.g., GitHub Actions), add these as secrets and map them in your deployment manifest.

## 🛠️ Management Commands

Run these commands from the project root.

### 1. View Current Secrets
To see all configured keys for both projects:

**Analyzer (AI Keys):**
```bash
dotnet user-secrets list --project src/Analyzer
```

**API (Infrastructure Keys):**
```bash
dotnet user-secrets list --project src/Api
```

### 2. Set or Update a Secret
Replace `your_value` with your actual secret. Use **single quotes** if the value contains special characters like `!`.

**For AI Keys:**
```bash
dotnet user-secrets set "OPENAI_API_KEY" "your_openai_key" --project src/Analyzer
```

**For Database Password:**
```bash
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'your_strong_password_here' --project src/Analyzer
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'your_strong_password_here' --project src/Api
```

# Multi-provider shortcuts
dotnet user-secrets set "GEMINI_API_KEY" "..." --project src/Analyzer
dotnet user-secrets set "DEEPSEEK_API_KEY" "..." --project src/Analyzer
dotnet user-secrets set "MISTRAL_API_KEY" "..." --project src/Analyzer
dotnet user-secrets set "HUGGINGFACE_API_KEY" "..." --project src/Analyzer
dotnet user-secrets set "OPENROUTER_API_KEY" "..." --project src/Analyzer
dotnet user-secrets set "CLOUDFLARE_API_TOKEN" "..." --project src/Analyzer
dotnet user-secrets set "CLOUDFLARE_ACCOUNT_ID" "..." --project src/Analyzer
```

### 3. Remove a Secret
```bash
dotnet user-secrets remove "KEY_NAME" --project src/Analyzer
```

---

## 📂 Physical Location (macOS)
The secrets are stored in a JSON file outside of your project directory:
`~/.microsoft/usersecrets/a7205fee-b873-4d14-abcd-51d5ce9baa1d/secrets.json`

## 🔗 References
- [Official Microsoft Docs: User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [StoryAnalyzer.cs Source](../src/Analyzer/Services/StoryAnalyzer.cs)
