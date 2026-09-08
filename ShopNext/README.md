# 🛍️ ShopNext - Hyperlocal Multi-Vendor E-Commerce, Logistics & System Monitoring Platform

ShopNext is an enterprise-grade, high-performance **Hyperlocal Multi-Vendor E-Commerce, Smart Logistics & Enterprise System Monitoring Platform** built with **ASP.NET Core 8.0 MVC**, **Entity Framework Core**, **Microsoft SQL Server**, and modern **Dark Glassmorphic UI aesthetics**.

The platform seamlessly connects neighborhood merchants (Grocery, Electronics, Bakery, Organic produce, Fashion) with local customers and delivery riders for ultra-fast doorstep delivery, backed by a bank-grade Admin System Monitoring and Audit Command Center.

---

## 🛡️ Enterprise System Monitoring & Audit Center (Point 174)
ShopNext includes a dedicated **Super-Admin System Monitoring Command Center** accessible at `/Admin/SystemMonitoring`:
- 👥 **Live Session Telemetry**: Real-time tracking of Active, Online, Offline, and Idle user sessions with device, IP, and browser detection.
- ⚡ **Force Session Termination**: Super-Admin can remotely terminate compromised or rogue user sessions with 1-click (`/Admin/ForceLogout`).
- 📝 **Diff Change Tracking (Audit)**: Tracks before vs after JSON values (`OldValues` vs `NewValues`) on every entity update across all tables.
- 🗑️ **Soft Delete & 1-Click Restore**: Full Recycle Bin architecture ensuring zero accidental data loss with instant rollback (`/Admin/RestoreDeletedRecord`).
- 👁️ **Seen / Unseen Notification Tracking**: Read receipts and live badge counters for alerts, orders, and customer queries.
- 🚨 **Automated Threat Detection**: Real-time security alerts for brute force logins, suspicious IP hopping, and high-velocity order fraud.
- 📊 **Audit Log CSV Export**: One-click streaming download of complete audit logs for compliance (`/Admin/ExportAuditLogsCsv`).

---

## 🌐 Point 173: Multi-Language & Localization System
ShopNext includes a native **Multi-Language & Localization Engine** supporting 10 Indian & international languages:
- 🇬🇧 **English (`en`)** (Default)
- 🇮🇳 **हिन्दी - Hindi (`hi`)**
- 🇮🇳 **Hinglish (`hi-Latn`)**
- 🇮🇳 **বাংলা - Bengali (`bn`)**
- 🇮🇳 **मराठी - Marathi (`mr`)**
- 🇮🇳 **தமிழ் - Tamil (`ta`)**
- 🇮🇳 **తెలుగు - Telugu (`te`)**
- 🇮🇳 **ગુજરાતી - Gujarati (`gu`)**
- 🇮🇳 **ಕನ್ನಡ - Kannada (`kn`)**
- 🇮🇳 **ਪੰਜਾਬੀ - Punjabi (`pa`)**

### ⚡ Localization Highlights:
1. **Header Dropdown**: Instant language switcher with native scripts and regional flags.
2. **Instant DOM Translation**: Dynamic client-side dictionary replacement without reloading the page.
3. **Persistent Culture**: Synchronized across cookies (`ShopNext_Language`), `localStorage`, and server-side responses.
4. **Server-Side ILocalizationService**: Localized flash messages, error codes, tax invoices, and notifications.

---

## 🚀 Key Technologies & Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core 8.0 MVC (C#) |
| **Database** | Microsoft SQL Server (`localhost\SQLEXPRESS` / LocalDB) |
| **ORM & Data Access** | Entity Framework Core 8.0 + LINQ + 40 Optimized Stored Procedures |
| **Styling & Design** | Vanilla CSS3, Bootstrap 5, Dark Glassmorphism, FontAwesome 6 |
| **Localization** | Custom Dual-Engine Localization (`ILocalizationService` + `localization.js`) |
| **Maps & Geolocation** | Leaflet.js (OpenStreetMap) Live GPS Tracking |
| **Security** | Claims-Based Cookie Authentication, Antiforgery Tokens, SHA256 Hashing |

---

## 👥 Platform Portals & Architecture

```mermaid
graph TD
    A[ShopNext Platform] --> B[🛒 Customer Portal]
    A --> C[🏪 Seller / Merchant Portal]
    A --> D[🛵 Delivery Partner / Rider Portal]
    A --> E[🛡️ Admin Super-Console & Monitoring Center]

    B --> B1[Marketplace, Cart, Wallet, Wishlist, Compare, Q&A, Voice/Multi-Lang]
    C --> C1[Catalog, Inventory, Flash Sales, Order Fulfillment, Payouts]
    D --> D1[Active Trips, Route Navigation, OTP Verification, POD Geotagging]
    E --> E1[System Monitoring, Live Sessions, Diff Audit, Threat Guard, Soft Deletes]
```

### 1. 🛒 Customer Experience Portal
- **Geolocation Discovery**: Proximity-based shop sorting (Haversine formula).
- **Product Comparison**: Side-by-side matrix for 2–4 products (Price, Rating, Specs).
- **Digital Wallet & Gift Cards**: 4-balance ledger (Main, Instant Refund, Promo, Reward Points).
- **Search Memory & Suggestions**: Auto-recorded queries with one-click cleanup.
- **Payment Failure Recovery**: Non-destructive checkout retry gateway.
- **Smart Recommendations**: *"Customers also bought"*, *"You may also like"*.

### 2. 🏪 Seller / Merchant Hub
- **Shop Onboarding**: KYC verification, address geolocation, document audit.
- **Product Management**: Variants, multi-angle imagery, stock threshold alerts.
- **Flash Sale Participation**: Seller campaign enrollment and price discount controls.
- **Order Dispatch**: Packing slips, barcode tagging, rider auto-assignment.

### 3. 🛵 Delivery Partner / Rider Portal
- **Trip Dispatch**: Real-time order dispatch notifications.
- **Turn-by-Turn Navigation**: Map routing from merchant doorstep to customer address.
- **Proof of Delivery (POD)**: Customer delivery OTP verification + latitude/longitude geotagging.
- **Failed Delivery Logging**: 3-attempt failover management with automatic re-scheduling.

### 4. 🛡️ Platform Admin Super-Console & Monitoring Center
- **System Monitoring Command Center**: Live online user counters, session management, telemetry.
- **Entity Diff Tracking**: Visual old-vs-new JSON property differences on record updates.
- **Recycle Bin & Soft Delete Restore**: Reversible data deletion safeguards.
- **AI Customer Risk Score Engine**: Multi-variable behavioral fraud scoring formula.
- **Payment Reconciliation**: Gateway gross vs captured vs refunded net settlement ledger.

---

## 🗄️ Database Architecture: 41 Tables & 40 Stored Procedures

### 41 Production Database Tables:
1. `dbo.Users` - Platform accounts (Admin, Customer, Vendor, Rider)
2. `dbo.PasswordResetTokens` - Secure OTP password resets
3. `dbo.Shops` - Merchant stores & geo-coordinates
4. `dbo.Products` - Catalog items & inventory
5. `dbo.Orders` - Transactions & lifecycle tracking
6. `dbo.OrderItems` - Item lines mapped inside orders
7. `dbo.ProductVariants` - SKU sizes, colors, and pricing
8. `dbo.CustomerAddresses` - Delivery addresses with geotags
9. `dbo.Categories` - Product category tree & icons
10. `dbo.Brands` - Brand catalogs & logos
11. `dbo.Coupons` - Discount vouchers & limits
12. `dbo.Offers` - Promotional banners & deals
13. `dbo.Complaints` - 3-party dispute management
14. `dbo.Reviews` - Ratings & verified buyer reviews
15. `dbo.Riders` - Delivery partners & live GPS telemetry
16. `dbo.Notifications` - System alerts & push notices
17. `dbo.Wishlists` - Saved customer items
18. `dbo.AuditLogs` - Who/What/When/Where audit trail
19. `dbo.OrderEvidences` - Unboxing photos & dispatch seals
20. `dbo.LoginHistories` - Auth logs with IP, device & browser
21. `dbo.UserSessions` - Active sessions with last-seen heartbeat
22. `dbo.UserActivities` - Granular user actions & audit telemetry
23. `dbo.EntityChangeLogs` - Before vs After JSON diff tracking
24. `dbo.SoftDeleteLogs` - Recycle Bin with 1-click restore metadata
25. `dbo.EntityViewLogs` - Seen/Unseen read receipt tracking
26. `dbo.SecurityThreatAlerts` - Automated fraud & brute force alerts
27. `dbo.SearchHistories` - Keyword search logs & suggestions
28. `dbo.ProductQuestions` - Pre-purchase Q&A inquiries
29. `dbo.ProductAnswers` - Verified answers from sellers/admin
30. `dbo.ChatMessages` - Live customer-vendor chat
31. `dbo.PriceDropAlerts` - Target price drop notifications
32. `dbo.StockAlerts` - Back-in-stock auto notifications
33. `dbo.WalletAccounts` - Customer digital wallet balances
34. `dbo.WalletTransactions` - Wallet credit/debit audit ledger
35. `dbo.GiftCards` - Digital gift vouchers & pins
36. `dbo.RewardPoints` - Loyalty rewards balance
37. `dbo.RewardPointsTransactions` - Loyalty accrual & redemption log
38. `dbo.StockReservations` - Concurrency stock hold locks
39. `dbo.PincodeServiceabilities` - Delivery serviceable zones & ETAs
40. `dbo.FailedDeliveryLogs` - 3-strike delivery failover logs
41. `dbo.DeliveryProofs` - Geotagged POD with handover OTPs

---

## 📁 Repository Structure

```
ShopNext/
│
├── Controllers/
│   ├── HomeController.cs           # Marketplace, Compare, Invoice, Language Switcher, Cart
│   ├── CustomerController.cs       # Account, Orders, Digital Wallet, Retry Payment, Returns
│   ├── VendorController.cs         # Merchant Portal, Product Catalog, Order Fulfillment
│   ├── RiderController.cs          # Delivery Dashboard, GPS Telemetry, OTP Verification
│   ├── AdminController.cs          # Audit Logs, Reconciliation, Risk Guard, System Monitoring
│   ├── AccountController.cs        # Auth, Session Tracking, Login/Logout Telemetry
│   └── DatabaseSetupController.cs  # SQL Schema Hub & Seed Console
│
├── Models/
│   ├── User.cs                     # Identity & Role Management
│   ├── Shop.cs                     # Merchant Stores & Geolocation
│   ├── Product.cs                  # Catalog Items & Stock
│   ├── Order.cs                    # Orders, Payments & Return Status
│   ├── AdvancedFeaturesModels.cs   # Comparison, Wallet, Q&A, Alerts, Reconciliation, POD
│   ├── SystemMonitoringModels.cs   # Sessions, User Activities, Diffs, Soft Deletes, Threats
│   └── ShopNextDbContext.cs        # Relational Database Context with EF Core mappings
│
├── Services/
│   ├── ILocalizationService.cs     # Multi-Language Localization Interface
│   ├── LocalizationService.cs      # 10-Language Dictionary Translation Provider
│   ├── ISystemMonitoringService.cs # Session Telemetry & Audit Interface
│   ├── SystemMonitoringService.cs  # Diffs, Soft Delete Restore, Live Monitoring
│   ├── IAdvancedEcommerceService.cs# Points 81-100 Advanced Engines
│   ├── AdvancedEcommerceService.cs # Business Logic Implementation
│   └── IShopNextService.cs         # Core E-Commerce Services
│
├── Views/
│   ├── Home/                       # Index, Marketplace, Compare, Cart, Invoice
│   ├── Customer/                   # Account, Wallet, RetryPayment, MyOrders
│   ├── Vendor/                     # Products, Orders, Store Settings
│   ├── Rider/                      # Delivery Trips, GPS Navigation
│   ├── Admin/                      # Master Control Panel & System Monitoring Command Center
│   └── Shared/_Layout.cshtml       # Header, Multi-Lang Switcher, Global Footer
│
├── Database/
│   └── ShopNext_Complete_Schema_And_Stored_Procedures.sql # Master DB SQL Script
│
└── wwwroot/
    ├── js/localization.js          # Client-Side Dynamic Translation Engine
    ├── js/site.js                  # Cart & AJAX Helpers
    ├── css/site.css                # Dark Glassmorphism Styling
    └── sql/ShopNext_Complete_DB_Setup.sql # Synced DB SQL Script
```

---

## ⚡ Quick Start & Running Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Express, or Developer Edition)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the repository**:
   ```bash
   git clone https://github.com/pra2011shant/ShopNext.git
   cd ShopNext/ShopNext
   ```

2. **Configure Database Connection**:
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

3. **Build and Run**:
   ```bash
   dotnet build
   dotnet run
   ```

4. **Access the Application**:
   Open browser at `http://localhost:5200` or `https://localhost:7147`.
   - Admin System Monitoring Center: `http://localhost:5200/Admin/SystemMonitoring`
   - Customer Marketplace: `http://localhost:5200`
   - Merchant Portal: `http://localhost:5200/Vendor/Login`
   - Rider Portal: `http://localhost:5200/Rider/Login`

---

## 📖 Detailed Workflows
For visual flowcharts of Order Lifecycles, Return Workflows, Localization Engines, AI Risk Calculations, and System Monitoring Architecture, refer to **[WORKFLOW.md](WORKFLOW.md)**.
