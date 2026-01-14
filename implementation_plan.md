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

## Phase 4: The Web API
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

### 4.4. Verify
1.  Run the API project.
2.  Open `http://localhost:xxxx/swagger`.
3.  Execute `GET /api/stories` and confirm we see the Reddit stories fetched by the Crawler.

## Phase 5: The Frontend
**Goal**: Display the stories beautifully.
1.  Create a basic **Angular** app.
2.  Fetch data from our `GET /stories` API.
3.  Display the stories in a list.

## Phase 6: Scaling Up 
**Goal**: *Now* we add the advanced enterprise features.
1.  Introduce **Kafka** to decouple the Crawler from the AI.
2.  Add **Vector Search** for "Vibe" matching.
3.  Add **Power Platform** for the admin dashboard.

---
**Current Status**: Ready to start Phase 4.
**Action**: Create the Web API project.
