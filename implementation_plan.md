# DarkGravity: Evolutionary Implementation Plan

You requested a smaller, step-by-step approach. We will build this project **iteratively**, ensuring each piece works perfectly before moving to the next.

## Phase 1: The Crawler (proof of concept) [COMPLETED]
**Goal**: Build a simple console app that can fetch stories from Reddit and print them. No database, no complex architecture yet.
1.  Create a standard `.NET Console App` (simpler than Worker Service for now).
2.  Add the `HttpClient` logic to fetch from `r/nosleep`.
3.  **Verify**: Run it and see stories appear in the terminal.

## Phase 2: The Data Store [COMPLETED]
**Goal**: Save the fetched stories to a real database so we don't lose them.
1.  Spin up **SQL Server** using Docker (just one container).
2.  Add **Entity Framework Core** to the project.
3.  Create the database table `Stories`.
4.  **Verify**: Run the crawler, then query the database to prove data is saved.

## Phase 3: The Logic Engine ("Socrates" Lite) [COMPLETED]
**Goal**: Use AI to analyze the text we saved.
1.  Connect **OpenAI/Semantic Kernel** to the Console App.
2.  Send the story text to the AI to ask: "Is this story scary? (Yes/No)".
3.  Save the AI's response to the database.
4.  **Verify**: Check the database for the AI's analysis.

## Phase 4: The Web API [COMPLETED]
**Goal**: Make the data accessible to a website via a RESTful API.
**Prerequisite**: We need to share the `Story` model and `AppDbContext` between the Crawler and the API.

### 4.1. Refactor: The "Shared" Library [COMPLETED]
1.  **Create Library**: Create a new Class Library project named `Shared` in `src/`.
2.  **Move Code**: Move `Story.cs` (Models) and `AppDbContext.cs` (Data) from `Crawler` to `Shared`.
3.  **Dependencies**: Install EF Core packages into `Shared`.
4.  **Update Crawler**: Remove the moved files from `Crawler`, add reference to `Shared`, and fix namespaces.

### 4.2. Create the API Project [COMPLETED]
1.  **Create Project**: Create a new ASP.NET Core Web API project named `Api` in `src/`.
2.  **References**: Add reference to `Shared`.
3.  **Database Config**:
    *   Copy the Connection String to `appsettings.json`.
    *   Register `AppDbContext` in `Program.cs`.

### 4.3. Implement Endpoints [COMPLETED]
1.  **CORS**: Enable CORS to allow our future Angular app to connect.
2.  **Controller**: Create `StoriesController.cs`.
    *   `GET /api/stories`: Returns list of stories (with ID, Title, AI Analysis).
    *   `GET /api/stories/{id}`: Returns full story details.
3.  **Swagger**: Ensure Swagger UI is enabled for easy testing.

### 4.4. Verify [COMPLETED]
1.  Run the API project.
2.  Open `http://localhost:xxxx/swagger`.
3.  Execute `GET /api/stories` and confirm we see the Reddit stories fetched by the Crawler.

## Phase 5: The "Dark" Evolution (Frontend & Decoupling) [COMPLETED]
**Goal**: Create a premium web experience and separate crawling from analysis for better stability.

### 5.1. The Foundation (Design & Branding)
1.  **Initialize Angular**: Create the project in `src/Web` using Angular 17/18+.
2.  **Design System**: Define a "Dark Gravity" theme in `styles.css`.
    *   **Palette**: Deep Void (#050505), Gravity Purple (#6200ea), and Ethereal Silver.
    *   **Effects**: Glassmorphism, neon glows, and smooth transitions.
3.  **Core Layout**: Create a shell with a premium sidebar/header and a main content area.

### 5.2. Infrastructure & Services
1.  **Story Service**: Create an Angular service to interface with the .NET Web API.
2.  **DTO Interfaces**: Define TypeScript interfaces reflecting the `Shared` Story model.
3.  **Environment Config**: Setup API base URLs for local dev.

### 5.3. Components (High-End UI)
1.  **Story Card**: A glass-styled card showing title, excerpt, and the AI "Scary Score".
2.  **Scary Score Gauge**: A custom visual indicator (glowing meter) for the AI analysis result.
3.  **Story Grid**: A responsive layout using CSS Grid/Flexbox with staggered entrance animations.
4.  **Reader Mode**: A dedicated view for reading full stories with optimized typography.

### 5.4. Features & UX
1.  **Dynamic Sorting**: Sort stories by date or "Scary Score" (AI-driven).
2.  **Search/Filter**: Quick search through the abyss of stories.
3.  **Loading States**: Implement "Ghost" skeletons (shimmering placeholders) during data fetch.

### 5.6. Backend Decoupling (The Analyzer) [COMPLETED]
1.  **Project Creation**: Created `src/Analyzer` purely for AI logic.
2.  **Migration**: Moved `StoryAnalyzer` and AI dependencies from Crawler to Analyzer.
3.  **Refactor**: Updated Crawler to save stories with empty analysis fields.
4.  **Database Indexing**: Added unique index on `ExternalId` for robust deduplication.
5.  **Verify**: Ran Crawler then Analyzer sequentially to prove decoupling works.

### 5.7. Final Verification
1.  Run the .NET Api, Angular Web app, Crawler, and Analyzer.
2.  Ensure stories flow from Reddit -> Crawler -> SQL -> Analyzer -> SQL -> API -> Angular UI.

## Phase 6: Scaling Up (The "Resume" Features)
**Goal**: *Now* we add the advanced enterprise features using event-driven architecture and senior-level architectural patterns.

### 6.1. Event Bus Foundation (Reliability & Stability) [COMPLETED]
1.  **Contracts Library** [COMPLETED]: Created `src/Contracts` containing ONLY interfaces and records (Low Coupling).
2.  **MassTransit + Kafka** [COMPLETED]: Implemented the event bus in Crawler and Analyzer using the Contracts.
3.  **Outbox Pattern** [COMPLETED]: Enabled MassTransit's Transactional Outbox to ensure database and message bus are perfectly synced.
4.  **Resiliency (Polly)** [COMPLETED]: Configured Retry policies for AI API calls using MassTransit's built-in resiliency features.

### 6.2. Advanced Processing (Scalability & Observability)
1.  **Idempotent Consumers**: Ensure the Analyzer can safely receive duplicate messages without re-calling the AI.
2.  **OpenTelemetry**: Implement distributed tracing to track a story's journey from Reddit to the UI.
3.  **Vector Search**: Integrate a Vector Database (e.g., Qdrant) for semantic "vibe" matching.

### 6.3. Enterprise Monitoring
1.  **Admin Dashboard**: Integrate Power Platform for high-level monitoring and manual story curation.

---
**Current Status**: Phase 6.1 (Event Bus Foundation) is completed. 
**Next Objective**: Phase 6.2 (Advanced Processing & Observability).

[COMPLETED]: https://github.com/nhan/DarkGravity
