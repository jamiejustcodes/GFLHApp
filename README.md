# Greenfield Local Hub (GFLHApp)

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

**Greenfield Local Hub (`GFLHApp`)** is a modern ASP.NET Core 9.0 MVC web application designed to connect conscious consumers with regional agricultural producers and farm shops. The platform supports multi-vendor order slicing, dynamic loyalty calculations, role-based dashboards, and regulatory compliance features.

---

## Key Features

### 🛒 Customer E-Commerce Experience
- **Interactive Catalogue**: Filter products by category, view real-time stock levels, allergen badges, and producer origins.
- **Dynamic Basket & Loyalty Engine**: Automatic calculation of subtotals and tiered discounts based on repeat purchase history.
- **Flexible Fulfillment**: Choice between local delivery (with 3 speed tiers) and click-and-collect date picker (with automated +2 day minimum scheduling).
- **One-Click Re-Ordering & Order Tracking**: Instant re-population of previous orders into the active basket with automated delivery tracking progress.

### 🚜 Producer Management Hub
- **Vendor Dashboard**: Visual sales summaries, pending order count, and revenue metrics.
- **Multi-Vendor Order Slicing**: Independent order item acceptance, partial fulfilment, and item cancellation without disrupting other vendors in the same order.
- **Inventory Controls**: Instant toggle for product availability and stock levels.
- **Branding & Compliance**: Custom logo image upload validation, VAT registration status, and automated HMRC-compliant invoice generation.

### 🛡️ Admin Suite
- **Centralised Management**: High-level platform statistics, revenue overview, and recent activity streams.
- **Catalogue Moderation**: Full administrative control over producer listings and product catalog items.
- **User & Role Administration**: Role assignment, customer order audit trails, and inquiry inbox management.

### ♿ Accessibility & Compliance
- **Accessibility Toolbar**: Real-time font scaling, high-contrast theme toggling, and readable font switches.
- **Legal Compliance**: Mandatory Terms & Conditions consent, allergen notices, and automated invoice number generation.

---

## Technology Stack

- **Framework**: ASP.NET Core 9.0 MVC
- **Data & ORM**: Entity Framework Core 9.0 with SQLite / SQL Server
- **Security & Identity**: ASP.NET Core Identity (Role-Based Access Control) + Google OAuth 2.0
- **Frontend & Styling**: Vanilla CSS design tokens with custom typography, responsive CSS grid, and JavaScript micro-interactions

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
3. Apply database migrations and seed default data:
   ```bash
   dotnet run --project GFLHApp
   ```
4. Open the browser and navigate to `https://localhost:7148` (or port specified in `launchSettings.json`).

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
