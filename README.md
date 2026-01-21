# 🌌 DarkGravity: The AI-Driven Abyss

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Angular](https://img.shields.io/badge/Angular-18-dd0031.svg)](https://angular.dev/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ed.svg)](https://www.docker.com/)

**DarkGravity** is a sophisticated, end-to-end horror story ecosystem. It crawls the dark corners of the web (Reddit, YouTube), uses a multi-provider AI engine to analyze the "scary score" and hidden patterns of stories, and presents them through a premium, immersive "Dark Mode" web interface.

---

## 🏗️ Project Architecture

The repository is organized into a modular, decoupled architecture following **SOLID** principles and **Clean Architecture** patterns:

*   **`src/Crawler`**: A .NET Console application that acts as the "Ingestion Engine". It fetches stories from multiple subreddits (r/nosleep, r/shortscarystories) and YouTube transcripts. It saves raw data to the database, leaving analysis for the next stage.
*   **`src/Analyzer`**: A dedicated AI processing project. It scans the database for pending stories, runs them through the multi-provider AI failover engine, and updates the "Scary Scores" and analysis.
*   **`src/Api`**: An ASP.NET Core Web API that serves the analyzed stories to the frontend via RESTful endpoints.
*   **`src/Shared`**: A Class Library containing shared Domain Models (`Story`) and the Entity Framework Core `AppDbContext`. This ensures high cohesion and low coupling across the system.
*   **`src/Web`**: A high-end Angular 18+ application featuring glassmorphism, fluid animations, and a premium "void" aesthetic.
*   **`infra/`**: Docker configuration for local infrastructure (SQL Server).

##🚀 Key Features

-   **Multi-Source Ingestion**: Automated crawling logic for Reddit JSON APIs and YouTube transcripts.
-   **"Socrates" AI Engine**: Integrated with **Google Gemini**, **OpenAI (GPT-4o)**, **DeepSeek**, and **Mistral** via a failover strategy to ensure constant analysis availability. (See `StoryAnalyzer.cs`)
-   **Automated Scoring**: AI-generated "Scary Scores" and qualitative analysis stored alongside raw story data for complex querying and sorting.
-   **Premium Viewing Experience**: Responsive "Reader Mode" with optimized typography for maximum immersion.
-   **High-Quality Text-to-Speech**: Google Cloud TTS integration with neural voices for accessible story narration. (See [TTS Setup Guide](docs/GOOGLE_CLOUD_TTS_SETUP.md))
-   **Dockerized Infrastructure**: One-command setup for the local SQL Server database.

## 🛠️ Tech Stack

-   **Backend**: .NET 8, C#, EF Core (SQL Server).
-   **Frontend**: Angular 18+, Vanilla CSS (Glassmorphism), Google Fonts (Orbitron/Inter).
-   **AI**: Semantic Kernel, Multi-LLM provider integration with automatic failover.
-   **Infra**: Docker, User Secrets Management.

## 🏁 Getting Started

### 1. Requirements
-   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
-   [Node.js / npm](https://nodejs.org/) (for the Angular frontend)

### 2. Infrastructure Setup
Spin up the SQL Server database:
```bash
docker compose -f infra/docker-compose.yml up -d
```

Ensure you have the required AI API keys and database credentials set up using .NET User Secrets. This keeps sensitive data out of the repository. See [Docs: Secrets Management](docs/SECRETS_MANAGEMENT.md) for detailed instructions.

```bash
# Set AI Keys (Move to Analyzer project)
dotnet user-secrets set "GEMINI_API_KEY" "your_key" --project src/Analyzer

# Set Database Password (Namespaced)
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'your_strong_password_here' --project src/Api
dotnet user-secrets set "DARKGRAVITY_DB_PASSWORD" 'your_strong_password_here' --project src/Analyzer

# Set Google Cloud TTS Key (API project)
dotnet user-secrets set "GoogleCloud:TtsApiKey" "your_key" --project src/Api

# View all configured secrets
dotnet user-secrets list --project src/Analyzer
dotnet user-secrets list --project src/Api
```

### 4. Run the Abyss
1.  **Populate the database**:
    ```bash
    dotnet run --project src/Crawler
    ```
2.  **Analyze the stories** (AI Processing):
    ```bash
    dotnet run --project src/Analyzer
    ```
3.  **Start the API**:
    ```bash
    dotnet run --project src/Api
    ```
3.  **Start the Web UI**:
    ```bash
    cd src/Web && npm install && npm start
    ```

## 🧪 Testing & Code Coverage

### 🔧 Prerequisites
To generate merged HTML reports for the backend, install the `ReportGenerator` tool:
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### 🖥️ Backend (.NET)
Run unit and integration tests and collect coverage:
```bash
# Run tests and collect data
dotnet test --collect:"XPlat Code Coverage"

# Generate human-readable HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# View report (MacOS)
open coveragereport/index.html
```

### 🌐 Frontend (Angular)
Run component/service tests and generate coverage:
```bash
cd src/Web

# Run tests once with coverage
npm test -- --coverage --watch=false

# View report (MacOS)
open coverage/index.html
```

### 5. Maintenance & Troubleshooting

#### 🔧 AI Analysis Migration
If new AI providers are added or if stories were processed with "Mock Analysis" (due to missing keys), you can re-process them with the Migration Tool:

```bash
# Finds any 'MOCK ANALYSIS' stories and re-runs them with currently configured AI keys
dotnet run --project src/Analyzer -- --migrate
```

## 📖 Documentation
- [Implementation Plan](implementation_plan.md) - The roadmap of the project.
- [Secrets Management Guide](docs/SECRETS_MANAGEMENT.md) - How to configure API keys.

---

*Generated by Antigravity.*
