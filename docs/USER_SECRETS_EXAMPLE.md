# 🔐 .NET User Secrets Example Guide

Since .NET User Secrets are stored outside the project folder in a machine-specific location, we cannot provide a physical file example. Instead, use these templates to set up your local development environment.

## 🚀 Setup Commands

Run these from the project root after cloning:

### 1. API Project (Database & CORS)
```bash
# Set the namespaced DB password
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'REMOVED_PASSWORD' --project src/Api

# Set the Allowed Origins (Local Development)
dotnet user-secrets set "AllowedOrigins:0" "http://localhost:4200" --project src/Api
```

### 2. Analyzer Project (Database & AI API Keys)
```bash
# Set the namespaced DB password
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'REMOVED_PASSWORD' --project src/Analyzer

# AI API Keys (at least one required, multiple for failover)
# Primary (Free tier available)
dotnet user-secrets set "GEMINI_API_KEY" "your_key_here" --project src/Analyzer

# Optional Fallbacks (in order of priority)
dotnet user-secrets set "DEEPSEEK_API_KEY" "your_key_here" --project src/Analyzer
dotnet user-secrets set "MISTRAL_API_KEY" "your_key_here" --project src/Analyzer
dotnet user-secrets set "CLOUDF_API_TOKEN" "your_token_here" --project src/Analyzer
dotnet user-secrets set "CLOUDFLARE_ACCOUNT_ID" "your_account_id" --project src/Analyzer
dotnet user-secrets set "HUGGINGFACE_API_KEY" "your_key_here" --project src/Analyzer
dotnet user-secrets set "OPENROUTER_API_KEY" "your_key_here" --project src/Analyzer
dotnet user-secrets set "OPENAI_API_KEY" "your_key_here" --project src/Analyzer
```

## 🔍 How to View Your Secrets
To verify your setup, run:
```bash
dotnet user-secrets list --project src/Api
dotnet user-secrets list --project src/Analyzer
```

## 📝 Configuration Mapping
These values are automatically mapped to the following JSON structure in memory at runtime:

**Api (secrets.json)**
```json
{
  "DARKGRAVITY_DB_PASSWORD": "...",
  "AllowedOrigins": [
    "http://localhost:4200"
  ]
}
```

**Analyzer (secrets.json)**
```json
{
  "DARKGRAVITY_DB_PASSWORD": "...",
  "GEMINI_API_KEY": "...",
  "DEEPSEEK_API_KEY": "...",
  "MISTRAL_API_KEY": "...",
  "CLOUDF_API_TOKEN": "...",
  "CLOUDFLARE_ACCOUNT_ID": "...",
  "HUGGINGFACE_API_KEY": "...",
  "OPENROUTER_API_KEY": "...",
  "OPENAI_API_KEY": "..."
}
```

## 🤖 AI Provider Notes

- **Gemini**: Recommended for free tier. Get key at [Google AI Studio](https://makersuite.google.com/app/apikey)
- **DeepSeek**: Fast and affordable. Get key at [DeepSeek Platform](https://platform.deepseek.com/)
- **Mistral**: European AI provider. Get key at [Mistral AI](https://console.mistral.ai/)
- **Cloudflare**: Requires both token and account ID. Get at [Cloudflare Dashboard](https://dash.cloudflare.com/)
- **HuggingFace**: Open-source models. Get key at [HuggingFace](https://huggingface.co/settings/tokens)
- **OpenRouter**: Aggregator with free models. Get key at [OpenRouter](https://openrouter.ai/keys)
- **OpenAI**: Premium option. Get key at [OpenAI Platform](https://platform.openai.com/api-keys)

**Failover Strategy**: The Analyzer tries providers in the order listed above. If one fails (quota exceeded, error), it automatically tries the next. At least one key is required to avoid mock analysis fallback.
