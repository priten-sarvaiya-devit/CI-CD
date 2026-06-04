# CI-CD Weather API

[![CI-CD](https://github.com/<your-username>/<your-repo>/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/<your-username>/<your-repo>/actions/workflows/ci-cd.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A small, production-shaped **ASP.NET Core Web API** built to demonstrate an
**enterprise-grade CI/CD pipeline** end to end: clean architecture, dependency
injection, automated unit testing with xUnit, a GitHub Actions pipeline, and
free deployment to Render.com.

> Replace `<your-username>/<your-repo>` in the badges above with your actual
> GitHub repository path so the build badge renders.

---

## 📖 Project Overview

The project exposes a single, realistic endpoint — `GET /api/weather` — that
returns sample weather forecast data. While the domain is intentionally simple,
the **structure and tooling around it are the point**: it shows how a real team
would organize code, test it, and ship it automatically.

What it demonstrates:

- A layered API following **Separation of Concerns**.
- **Constructor-based dependency injection** throughout.
- **Unit tests** for the model, service, and controller (xUnit + Moq).
- A full **GitHub Actions** pipeline: restore → build → test → publish → artifact.
- **Containerized deployment** to Render.com on the free tier.

---

## 🏗️ Architecture

The solution is split into two projects with a clean, layered structure:

```
CI-CD/                          # Solution root
├── CI-CD.slnx                  # Solution file
├── .github/
│   └── workflows/
│       └── ci-cd.yml           # GitHub Actions pipeline
├── Dockerfile                  # Multi-stage build for Render deployment
├── render.yaml                 # Render Blueprint (Infrastructure-as-Code)
│
├── CI-CD/                      # ── ASP.NET Core Web API project ──
│   ├── Controllers/
│   │   └── WeatherController.cs # HTTP layer — thin, DI-driven
│   ├── Models/
│   │   └── WeatherForecast.cs   # Domain model
│   ├── Services/
│   │   ├── IWeatherService.cs   # Service abstraction (interface)
│   │   └── WeatherService.cs    # Business logic implementation
│   ├── Program.cs               # Composition root / DI registration
│   ├── appsettings.json
│   └── CI-CD.csproj
│
└── CI-CD.Tests/                # ── xUnit test project ──
    ├── Controllers/
    │   └── WeatherControllerTests.cs
    ├── Models/
    │   └── WeatherForecastTests.cs
    ├── Services/
    │   └── WeatherServiceTests.cs
    └── CI-CD.Tests.csproj
```

**Layer responsibilities**

| Layer          | Component                | Responsibility                                            |
|----------------|--------------------------|-----------------------------------------------------------|
| Presentation   | `WeatherController`      | HTTP routing, input validation, status codes. No logic.   |
| Application    | `IWeatherService` / `WeatherService` | Business rules — generates forecast data.     |
| Domain         | `WeatherForecast`        | Plain data model (POCO).                                   |
| Composition    | `Program.cs`             | Wires interfaces to implementations in the DI container.   |

The controller depends on the **`IWeatherService` abstraction**, not the
concrete class — so the implementation can be swapped or mocked without touching
the HTTP layer. This is the core of the testability story.

---

## 🛠️ Technologies Used

| Area              | Technology                                  |
|-------------------|---------------------------------------------|
| Runtime           | .NET 10 / ASP.NET Core                      |
| Language          | C# 13                                        |
| API Docs          | OpenAPI + [Scalar](https://scalar.com/)     |
| Testing           | xUnit, Moq, Coverlet (coverage)             |
| CI/CD             | GitHub Actions                              |
| Containerization  | Docker (multi-stage)                        |
| Hosting           | Render.com (free tier)                      |

---

## 🔄 CI/CD Flow

```
   Developer
      │  git push / open PR  →  main
      ▼
┌──────────────────────────────────────────────────────────┐
│                   GitHub Actions (CI)                      │
│                                                            │
│  Checkout → Setup .NET 10 → Restore → Build → Test →       │
│  Upload Test Results → Publish API → Upload Artifact       │
└──────────────────────────────────────────────────────────┘
      │  push to main (autoDeploy)
      ▼
┌──────────────────────────────────────────────────────────┐
│                    Render.com (CD)                         │
│   Pull repo → docker build (Dockerfile) → run container    │
│   → live at https://<service>.onrender.com                 │
└──────────────────────────────────────────────────────────┘
```

- **CI** runs on every push and pull request to `main`.
- **CD** is handled by Render's `autoDeploy`, triggered on push to `main`.

---

## 🚀 How to Run Locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
# 1. Restore dependencies
dotnet restore CI-CD.slnx

# 2. Run the API
dotnet run --project CI-CD/CI-CD.csproj
```

The API starts at `http://localhost:5215` (and `https://localhost:7059`).

**Try it:**

```bash
curl http://localhost:5215/api/weather
curl "http://localhost:5215/api/weather?days=7"
curl http://localhost:5215/health
```

Interactive API docs (Development only): open
`http://localhost:5215/scalar/v1` in a browser.

Sample response from `GET /api/weather`:

```json
[
  { "date": "2026-06-05", "temperatureC": 21, "temperatureF": 69, "summary": "Mild" },
  { "date": "2026-06-06", "temperatureC": -4, "temperatureF": 25, "summary": "Chilly" }
]
```

---

## 🧪 How to Run Tests

```bash
# Run all unit tests
dotnet test CI-CD.slnx

# With code coverage collection
dotnet test CI-CD.slnx --collect:"XPlat Code Coverage"
```

The suite covers the model conversion logic, the service (including edge cases
and argument validation), and the controller (verified with **Moq**).

---

## ⚙️ GitHub Actions Pipeline Explanation

The pipeline lives in [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml)
and runs on `ubuntu-latest`. Step by step:

| Step                      | What it does                                                                 |
|---------------------------|------------------------------------------------------------------------------|
| **Checkout source**       | Clones the repository into the runner.                                       |
| **Setup .NET SDK**        | Installs the .NET 10 SDK.                                                     |
| **Cache NuGet packages**  | Caches `~/.nuget/packages` keyed on the `.csproj` hashes to speed up restore.|
| **Restore dependencies**  | `dotnet restore` — pulls all NuGet packages.                                 |
| **Build solution**        | `dotnet build -c Release --no-restore`.                                      |
| **Run unit tests**        | `dotnet test` with a `.trx` logger and `XPlat Code Coverage` collection.     |
| **Upload test results**   | Uploads the `TestResults` folder as an artifact (`if: always()`).            |
| **Publish test report**   | Renders the `.trx` file as a readable check in the PR.                       |
| **Publish API**           | `dotnet publish` produces the deployable output in `./publish`.              |
| **Upload artifact**       | Uploads `./publish` as the `ci-cd-api` artifact (downloadable from the run). |

The triggers are defined at the top of the file:

```yaml
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:   # manual run button
```

---

## ☁️ Deploying to Render.com (Free)

This repo ships with a [`Dockerfile`](Dockerfile) and a
[`render.yaml`](render.yaml) Blueprint. Render's native runtimes don't include
.NET, so we deploy as a **Docker web service** — the reliable free-tier path.

### Option A — Blueprint (recommended, one click)

1. Push this repo to GitHub.
2. In Render, go to **New → Blueprint**, connect the repo. Render reads
   `render.yaml` and provisions the service automatically.

### Option B — Manual web service

Create **New → Web Service**, connect the repo, choose **Docker**, and use:

| Setting                | Value                                                            |
|------------------------|------------------------------------------------------------------|
| **Runtime**            | Docker                                                           |
| **Build Command**      | *(leave empty — handled by the Dockerfile)*                     |
| **Publish/Output**     | *(handled by `dotnet publish` inside the Dockerfile)*            |
| **Start Command**      | *(leave empty — uses the Dockerfile `ENTRYPOINT`)*               |
| **Health Check Path**  | `/health`                                                        |
| **Instance Type**      | Free                                                             |

> If you instead deploy **without Docker** (Render "Native" .NET environment, where available):
> - **Build Command:** `dotnet build CI-CD/CI-CD.csproj -c Release`
> - **Publish Command:** `dotnet publish CI-CD/CI-CD.csproj -c Release -o publish`
> - **Start Command:** `dotnet publish/CI-CD.dll`

### Environment Variables

| Variable                  | Value         | Notes                                                       |
|---------------------------|---------------|-------------------------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`  | `Production`  | Disables dev-only OpenAPI/Scalar UI.                        |
| `PORT`                    | *(auto)*      | Render injects it; the Dockerfile binds Kestrel to it.      |

The container reads `$PORT` via `ASPNETCORE_URLS=http://+:${PORT}`, so no manual
port configuration is needed.

---

## 📋 Step-by-Step: From Code to Live Demo

### 1. Create a GitHub repository
- Go to <https://github.com/new>.
- Name it (e.g. `ci-cd-weather-api`), keep it **public**, **don't** initialize
  with a README (you already have one), then **Create repository**.

### 2. Push the code
```bash
cd e:/Projects/CI-CD/CI-CD
git init
git add .
git commit -m "Initial commit: enterprise CI/CD weather API"
git branch -M main
git remote add origin https://github.com/<your-username>/<your-repo>.git
git push -u origin main
```

### 3. Enable GitHub Actions
- Open the **Actions** tab on your repo. Workflows in `.github/workflows/` are
  enabled by default; if prompted, click **"I understand my workflows, enable them"**.
- The `CI-CD` workflow runs automatically on the first push.

### 4. Verify Build and Test stages
- In the **Actions** tab, open the latest **CI-CD** run.
- Confirm every step shows a green check: Restore → Build → Run unit tests →
  Publish API.
- Open the **Unit Test Report** check to see passed test counts.
- Scroll to **Artifacts** and download `ci-cd-api` (the published app) and
  `test-results`.

### 5. Deploy to Render
- Sign in at <https://render.com> with GitHub.
- **New → Blueprint** → select your repo → **Apply**.
- Wait for the build to finish, then open the generated
  `https://<service>.onrender.com/api/weather` URL.
- Every subsequent push to `main` redeploys automatically (`autoDeploy: true`).

### 6. Demonstrating the CI/CD pipeline in a presentation
A suggested live-demo script:

1. **Show the green pipeline** — open the Actions tab; walk through each stage.
2. **Make a visible change** — edit a `Summaries` value or add a forecast field.
3. **Add/adjust a test** — show a test failing, then passing, to prove the gate works.
4. **Commit & push** — `git commit -am "demo change" && git push`.
5. **Watch CI run live** — the pipeline kicks off; narrate build → test → publish.
6. **Show the artifact** — download `ci-cd-api` from the completed run.
7. **Show CD** — Render auto-deploys; refresh the live `/api/weather` URL to show
   the change in production.
8. **Talk through the architecture** — DI, separation of concerns, why the
   controller depends on `IWeatherService`, and how that enables the Moq tests.

> Tip: open a failing PR on purpose to show the **red** pipeline blocking a merge —
> the strongest argument for CI.

---

## 📝 License

Released under the MIT License. See `LICENSE` for details.
