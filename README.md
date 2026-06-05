# CI-CD Weather API

[![CI-CD](https://github.com/priten-sarvaiya-devit/CI-CD/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/priten-sarvaiya-devit/CI-CD/actions/workflows/ci-cd.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A small, production-shaped **ASP.NET Core Web API** built to demonstrate an
**enterprise-grade CI/CD pipeline** end to end: clean architecture, dependency
injection, automated unit testing with xUnit, a GitHub Actions pipeline, and
free deployment to Render.com.

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
      │  git push / open PR  →  master
      ▼
┌──────────────────────────────────────────────────────────┐
│                   GitHub Actions                           │
│                                                            │
│   ┌─────────┐     ┌────────┐     ┌──────────┐              │
│   │  BUILD  │ ──▶ │  TEST  │ ──▶ │  DEPLOY  │              │
│   └─────────┘     └────────┘     └──────────┘              │
│   restore +       run xUnit      trigger Render            │
│   build +         tests +        deploy hook               │
│   publish         report         (push only)               │
└──────────────────────────────────────────────────────────┘
      │  Deploy job calls Render Deploy Hook
      ▼
┌──────────────────────────────────────────────────────────┐
│                    Render.com                              │
│   Pull repo → docker build (Dockerfile) → run container    │
│   → live at https://<service>.onrender.com                 │
└──────────────────────────────────────────────────────────┘
```

The pipeline is split into **three sequential jobs** — each renders as its own
block in the Actions run graph, and each only starts if the previous one passes:

- **Build** — runs on every push and pull request to `master`.
- **Test** — runs after Build succeeds (`needs: build`).
- **Deploy** — runs after Test succeeds, **only on push to `master`** (skipped for
  pull requests). It triggers Render via a Deploy Hook secret.

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
and is split into **three jobs** that run sequentially on `ubuntu-latest`. Each
job appears as its own block in the Actions run graph, chained with `needs:`.

### Job 1 — `build`
| Step                      | What it does                                                                 |
|---------------------------|------------------------------------------------------------------------------|
| **Checkout source**       | Clones the repository into the runner.                                       |
| **Setup .NET SDK**        | Installs the .NET 10 SDK.                                                     |
| **Cache NuGet packages**  | Caches `~/.nuget/packages` keyed on the `.csproj` hashes to speed up restore.|
| **Restore dependencies**  | `dotnet restore` — pulls all NuGet packages.                                 |
| **Build solution**        | `dotnet build -c Release --no-restore`.                                      |
| **Publish API**           | `dotnet publish` produces the deployable output in `./publish`.              |
| **Upload artifact**       | Uploads `./publish` as the `ci-cd-api` artifact (downloadable from the run). |

### Job 2 — `test` (`needs: build`)
| Step                      | What it does                                                                 |
|---------------------------|------------------------------------------------------------------------------|
| **Restore + build**       | Restores packages (build is implicit in `dotnet test`).                      |
| **Run unit tests**        | `dotnet test` with a `.trx` logger and `XPlat Code Coverage` collection.     |
| **Upload test results**   | Uploads the `TestResults` folder as an artifact (`if: always()`).            |
| **Publish test report**   | Renders the `.trx` file as a readable check (non-fatal).                      |

### Job 3 — `deploy` (`needs: test`, push to `master` only)
| Step                      | What it does                                                                 |
|---------------------------|------------------------------------------------------------------------------|
| **Trigger Render deploy** | POSTs to the `RENDER_DEPLOY_HOOK` secret URL to kick off a Render deploy.     |

> **Enabling the Deploy job:** add a repository secret named `RENDER_DEPLOY_HOOK`
> (Settings → Secrets and variables → Actions → New repository secret). Paste the
> **Deploy Hook URL** from your Render service (Settings → Deploy Hook). If the
> secret is missing — or Render returns a non-success response — the Deploy job
> **fails (red)**. A green Deploy means the deployment was actually triggered.

The triggers are defined at the top of the file:

```yaml
on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]
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
git remote add origin https://github.com/priten-sarvaiya-devit/CI-CD.git
git push -u origin master
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
Deployment is **driven by GitHub Actions** — the `deploy` job hits a Render
Deploy Hook *after* Build and Test pass, so a deploy only happens on green tests.

1. **Create the service:** sign in at <https://render.com> with GitHub →
   **New → Blueprint** → select your repo → **Apply**. Render builds the
   `Dockerfile` and gives you a `https://<service>.onrender.com` URL.
2. **Grab the Deploy Hook:** in Render → service → **Settings → Deploy Hook**,
   copy the URL.
3. **Add it to GitHub:** repo **Settings → Secrets and variables → Actions →
   New repository secret** → name it `RENDER_DEPLOY_HOOK`, paste the URL.
4. Now every push to `master` runs Build → Test → **Deploy** (which triggers
   Render). Render's own `autoDeploy` is disabled in `render.yaml` so deploys
   aren't duplicated — Actions is the single source of truth.

Verify: open `https://<service>.onrender.com/api/weather`.

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
