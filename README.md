# Greenfield Local Hub (GFLHApp)

<p align="center">
  <strong>A modern ASP.NET Core 9.0 MVC co-operative e-commerce web platform connecting local producers and conscious consumers.</strong>
</p>

<p align="center">
  <a href="https://github.com/jamiejustcodes/GFLHApp/actions/workflows/ci.yml">
    <img src="https://github.com/jamiejustcodes/GFLHApp/actions/workflows/ci.yml/badge.svg" alt="Build & Test" />
  </a>
  <img src="https://img.shields.io/badge/Tests-46%20Passing-brightgreen?logo=xunit" alt="46 Tests Passing" />
  <img src="https://img.shields.io/badge/.NET-9.0%20LTS-512BD4?logo=dotnet" alt=".NET 9.0 LTS" />
  <img src="https://img.shields.io/badge/C%23-13.0-239120?logo=csharp" alt="C# 13.0" />
  <img src="https://img.shields.io/badge/License-MIT-blue.svg" alt="MIT License" />
</p>

---

## Overview

**Greenfield Local Hub (`GFLHApp`)** is a full-featured ASP.NET Core 9.0 MVC web application designed to connect conscious consumers with regional agricultural producers and farm shops. The platform supports multi-vendor order slicing, dynamic loyalty calculations, role-based dashboards, and regulatory compliance features.

### Key Features

- **🛒 Customer E-Commerce Experience**:
  - Filter products by category, view real-time stock levels, allergen badges (Natasha's Law compliance), and producer origins.
  - Automatic calculation of subtotals and tiered discounts based on repeat purchase history.
  - Flexible fulfillment: choice between courier delivery (with 3 speed tiers) and Click & Collect date picker (with automated +2 day minimum scheduling).
  - One-click re-ordering from previous order history and live delivery tracking progression.

- **🚜 Producer Management Hub**:
  - Vendor dashboard with visual sales summaries, pending order counts, and revenue metrics.
  - **Multi-Vendor Order Slicing**: Independent order item acceptance, partial fulfillment, and item cancellation without disrupting other vendors in the same order.
  - Inventory controls with instant toggle for product availability and stock levels.
  - Custom logo image upload validation, VAT registration status, and automated HMRC-compliant invoice generation (`INV-2026-XXXXX`).

- **🛡️ Admin Suite**:
  - Centralised management with high-level platform statistics, revenue overview, and recent activity streams.
  - Catalogue moderation over producer listings and product catalog items.
  - User and role administration, customer order audit trails, and inquiry inbox management.

- **♿ Accessibility & Compliance**:
  - Global accessibility toolbar with real-time font scaling, high-contrast theme toggling, and readable font switches.
  - Mandatory Terms & Conditions consent, allergen notices, and automated invoice number generation.

---

## Solution Structure

```text
GFLHApp/
├── GFLHApp.sln                        # .NET 9.0 Solution File
├── GFLHApp/                           # ASP.NET Core 9.0 MVC Web Application
│   ├── Areas/Identity/                # Identity Auth (Login, Register, 2FA, Manage)
│   ├── Controllers/                   # MVC Controllers (Orders, Baskets, Producers, Admin, etc.)
│   ├── Data/                          # EF Core ApplicationDbContext, Migrations, and SeedData
│   ├── Models/                        # Domain entities (Basket, Orders, Products, Producers, etc.)
│   ├── Views/                         # Razor Views & Layouts
│   └── wwwroot/                       # Static Assets, CSS Design System, and JavaScript Modules
├── tests/
│   └── GFLHApp.Tests/                 # xUnit Test Suite (46 Automated Unit & Integration Tests)
│       ├── Controllers/               # Products and Admin Controller Tests
│       ├── Data/                      # Database persistence & cascade tests
│       ├── Helpers/                   # In-memory EF Core database test helpers
│       └── Models/                    # Basket, Order, Compliance, and Security tests
└── docs/                              # Project documentation, logs, and development diary
```

---

## Automated Testing & CI/CD

- **46 Automated Tests Passing**:
  - **Basket & Calculation Tests (5 tests)**: Subtotal calculations, loyalty discount rates, multi-item baskets, and dynamic quantity modifications.
  - **Product & Inventory Validation (5 tests)**: Stock thresholds, active status, producer association, and price validation.
  - **Order & Fulfillment Tests (6 tests)**: Multi-vendor order slicing, delivery speed calculations, Click & Collect date rules, and invoice formatting.
  - **Allergen & Legal Compliance (6 tests)**: Natasha's Law allergen parsing, UK VAT registration number verification (`GB...`), and conditional invoice generation.
  - **Security & Role-Based Authorization (5 tests)**: Role claim enforcement (Admin, Producer, Standard, Developer) and producer resource ownership checks.
  - **Database & Persistence Tests (3 tests)**: In-memory Entity Framework Core relationships, cascading foreign keys, and basket item tracking.
  - **Controller Unit Tests (4 tests)**: Products catalogue actions, invalid ID handling, and view model assignments.

- **Continuous Integration**:
  - GitHub Actions runs automatically on every `push` and `pull_request` to `main`.
  - Compiles the solution in `Release` mode and executes the complete xUnit test matrix on `windows-latest`.

---

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider

### Setup & Run
1. Clone the repository:
   ```bash
   git clone https://github.com/jamiejustcodes/GFLHApp.git
   cd GFLHApp
   ```
2. Restore NuGet dependencies and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run automated tests:
   ```bash
   dotnet test
   ```
4. Start the application:
   ```bash
   dotnet run --project GFLHApp
   ```
5. Open your browser and navigate to `https://localhost:7148`.

### Seeded Credentials
| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@example.com` | `Password123!` |
| **Producer** | `producer@example.com` | `Password123!` |
| **Standard User** | `user@example.com` | `Password123!` |
| **Developer** | `developer@example.com` | `Password123!` |

---

## Documentation
Full project documentation and development history are available in the [`docs/`](./docs/) directory:
- `DevelopmentDiary.docx` — Complete chronological 36-stage development log
- `Task2_Test_Log_Final.docx` — Quality assurance and testing logs
- `Task2_AI_Log.docx` — AI assistance documentation
- `Asset Log.docx` & `Task2_Source_Log.docx` — Asset attribution and source logs
