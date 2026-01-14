# DarkGravity: Evolutionary Implementation Plan

You requested a smaller, step-by-step approach. We will build this project **iteratively**, ensuring each piece works perfectly before moving to the next.

## Phase 1: The Crawler (proof of concept)
**Goal**: Build a simple console app that can fetch stories from Reddit and print them. No database, no complex architecture yet.
1.  Create a standard `.NET Console App` (simpler than Worker Service for now).
2.  Add the `HttpClient` logic to fetch from `r/nosleep`.
3.  **Verify**: Run it and see stories appear in the terminal.

## Phase 2: The Data Store
**Goal**: Save the fetched stories to a real database so we don't lose them.
1.  Spin up **SQL Server** using Docker (just one container).
2.  Add **Entity Framework Core** to the project.
3.  Create the database table `Stories`.
4.  **Verify**: Run the crawler, then query the database to prove data is saved.

## Phase 3: The Logic Engine ("Socrates" Lite)
**Goal**: Use AI to analyze the text we saved.
1.  Connect **OpenAI/Semantic Kernel** to the Console App.
2.  Send the story text to the AI to ask: "Is this story scary? (Yes/No)".
3.  Save the AI's response to the database.
4.  **Verify**: Check the database for the AI's analysis.

## Phase 4: The Web API
**Goal**: Make the data accessible to a website.
1.  Create a separate **.NET Web API** project.
2.  Build an endpoint `GET /stories` that reads from our SQL Database.
3.  **Verify**: Open the browser and see the JSON data.

## Phase 5: The Frontend
**Goal**: Display the stories beautifully.
1.  Create a basic **Angular** app.
2.  Fetch data from our `GET /stories` API.
3.  Display the stories in a list.

## Phase 6: Scaling Up (The "Resume" Features)
**Goal**: *Now* we add the advanced enterprise features.
1.  Introduce **Kafka** to decouple the Crawler from the AI.
2.  Add **Vector Search** for "Vibe" matching.
3.  Add **Power Platform** for the admin dashboard.

---
**Current Status**: Ready to start Phase 1.
**Action**: Shall I create the basic Console App for Phase 1?
