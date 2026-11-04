# Green & Tasty — Restaurant Platform (IT Marathon 2026)

Monorepo containing the complete codebase, infrastructure, and test automation suite for the **Green & Tasty** restaurant application, developed during the **IT Marathon 2026**.

---

## 🏗 Architecture & Project Structure

The project is structured into standalone modules covering the complete software engineering lifecycle:

```text
├── .github/workflows/          # CI/CD Workflows (Build, Test, Quality Gates)
├── backend/                    # .NET 10 Web API & AWS Lambda microservices
│   └── restaurant-backend-app/
│       ├── dnapp/lambdas/      # API Handler, Reports Handler, Report Sender
│       └── Tests/              # Backend unit and integration tests (xUnit)
├── frontend/                   # Modern Angular 19 web application
│   └── restaurant-frontend-app/
│       └── src/app/            # Signals-based components, services, and tests
├── aqa/                        # Automated QA Framework (.NET 10 + Playwright + Reqnroll + TUnit)
│   └── restaurant-aqa-framework/
│       ├── Features/           # Gherkin BDD specifications (Direction 1 & 2)
│       ├── Screenplay/         # Actor, Tasks, Questions, and Abilities
│       ├── StepDefinitions/    # Reqnroll bindings
│       └── Api/ & Ui/          # REST API clients & Playwright page objects
├── docs/                       # Project documentation & Manual QA
│   └── qa-manual/              # Test cases, requirements matrix, bug reports (CSV & MD)
└── infra/                      # Infrastructure as Code
    └── terraform/              # AWS resources (S3, CloudFront, Cognito, API Gateway)
```

---

## 🎯 Completed Homework Milestones

### 1. Backend (`backend/restaurant-backend-app`)
* **Group Pre-Order (Direction 1):**
  * `GET /guest/groups/{groupReservationId}/sessions`: Owner overview of participants, items, and total prices.
  * `POST /guest/sessions`: Guest session initialization and cart handling.
  * `POST /group-reservations/{groupReservationId}/submit`: Pre-order consolidation, item aggregation, and order submission.
  * `DELETE /guest/groups/{groupReservationId}/sessions/{guestSessionId}`: Participant removal with dynamic total recalculation.
* Comprehensive unit test coverage across all services and request handlers.

### 2. Frontend (`frontend/restaurant-frontend-app`)
* **Signals Architecture:** Migrated state management and reactive flows to Angular Signals (`signal`, `computed`).
* **Group Order Management:**
  * Owner review dashboard (`/reservations/:id/group-order`).
  * Participant cards with real-time status indicators (`Submitted` vs `Still choosing`).
  * Modal dialogs for order submission and participant removal.
  * Background polling and manual sync.
* Full test suite passing 67 Karma/Jasmine tests.

### 3. Manual QA (`docs/qa-manual`)
* **Requirements Analysis:** Deconstruction of user stories and acceptance criteria.
* **Test Suite:** 10 end-to-end and boundary test cases (`02_test_cases.csv` and `.md`).
* **Bug Reports:** Detailed defect documentation with reproduction steps (`03_bug_reports.csv` and `.md`).

### 4. Test Automation / AQA (`aqa/restaurant-aqa-framework`)
* **Screenplay Pattern + BDD:** Clean separation of concerns with declarative Gherkin features.
* **Full Coverage of Direction 1:**
  * Level 1: Viewing participants, empty group states, authorization checks.
  * Level 2: Consolidated pre-order submission, waiter visibility, negative submission conditions.
  * Level 3: Guest removal, cart isolation across groups, end-to-end smoke flow.
* **CI/CD Quality Gates:** GitHub Actions pipelines with TUnit HTML/TRX reporting and Playwright traces.

---

## 🚀 Quick Start & Local Execution

### 1. Automated QA Tests
```bash
cd aqa/restaurant-aqa-framework
cp .env.example .env     # configure environment variables
dotnet build
dotnet run --no-build -- --treenode-filter "/*/*/*/*[Category=group-preorder]"
```

### 2. Backend
```bash
cd backend/restaurant-backend-app
dotnet test
```

### 3. Frontend
```bash
cd frontend/restaurant-frontend-app
npm install
npm run test:hw
```

---

## 🌐 CI/CD Pipelines

GitHub Actions workflows are located in `.github/workflows/`:
* `aqa-quality.yml`: Format verification, analyzers, Gherkin lint, and test discovery.
* `aqa-tests.yml`: Scheduled (nightly) and on-demand automated test execution.
* `deploy.yml` & `pr.yml`: Frontend/backend CI verification.
