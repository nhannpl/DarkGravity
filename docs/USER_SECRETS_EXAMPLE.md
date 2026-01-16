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

### 2. Crawler Project (AI API Keys)
```bash
# Primary AI Key
dotnet user-secrets set "GEMINI_API_KEY" "your_key_here" --project src/Crawler

# Optional Fallbacks
dotnet user-secrets set "OPENAI_API_KEY" "your_key_here" --project src/Crawler
dotnet user-secrets set "DEEPSEEK_API_KEY" "your_key_here" --project src/Crawler
```

## 🔍 How to View Your Secrets
To verify your setup, run:
```bash
dotnet user-secrets list --project src/Api
dotnet user-secrets list --project src/Crawler
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

**Crawler (secrets.json)**
```json
{
  "GEMINI_API_KEY": "...",
  "OPENAI_API_KEY": "..."
}
```
