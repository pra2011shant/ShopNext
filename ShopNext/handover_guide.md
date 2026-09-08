# ShopNext - E-Commerce, Logistics & System Monitoring Handover Guide (Hindi)

**ShopNext** handover guide me aapka swagat hai. Yeh complete reference manual developers, administrators, aur technical team ke liye banaya gaya hai jisme application ki architecture, full 41 database tables, 40 stored procedures, enterprise system monitoring command center, aur end-to-end data workflows ko step-by-step Hindi/Hinglish me explain kiya gaya hai.

---

## 🚀 1. Tech Stack & Key Technologies (Tekniki Dhaancha)

- **Backend Framework**: **ASP.NET Core 8.0 MVC** (C# 12, .NET 8.0 LTS)
- **Database Engine**: **Microsoft SQL Server** (`localhost\SQLEXPRESS` / LocalDB / Azure SQL)
- **ORM & Data Layer**: **Entity Framework Core 8.0** with dual query capability (LINQ for CRUD + Stored Procedures for high-performance telemetry & reporting).
- **Frontend & Styling**: **Vanilla CSS3**, **Bootstrap 5**, **Dark Glassmorphic UI**, **FontAwesome 6 Icons**.
- **Multi-Language Engine**: Custom Dual-Engine Localization (`ILocalizationService` + `localization.js`) jo 10 Indian & global languages ko support karta hai.
- **Maps & Live Telemetry**: **Leaflet.js** (OpenStreetMap engine) dark-theme live rider tracking and route visualization ke liye.
- **System Monitoring & Auditing**: Enterprise-grade telemetry engine jo active sessions, user activities, JSON diff changes, soft-deletes, aur threat alerts ko real-time track karta hai.
- **Security**: Claims-Based Cookie Authentication, Anti-Forgery Tokens, SHA256 Password Hashing, Rate Limiting, aur Automated Threat Detection.

---

## 📊 2. Database Architecture (41 Tables Catalog)

ShopNext database me total **41 production tables** hain jo alag-alag modules ko handle karti hain:

### A. Core Platform & Identity
1. **`dbo.Users`**: Customer, Admin, Seller, aur Rider user profiles, passwords, phone numbers, aur risk scores store karta hai.
2. **`dbo.PasswordResetTokens`**: OTP aur password reset verification tokens ko expiry ke sath manage karta hai.
3. **`dbo.CustomerAddresses`**: Customers ke multiple delivery addresses, pincodes, landmarks, aur GPS coordinates store karta hai.

### B. Merchants & Catalog
4. **`dbo.Shops`**: Merchant stores ka onboarding data, KYC details, bank IFSC, address, aur approval flag (`IsApproved`) store karta hai.
5. **`dbo.Categories`**: E-commerce taxonomy, display order, aur icon classes.
6. **`dbo.Brands`**: Verified brands, logos, categories, aur rating.
7. **`dbo.Products`**: Store catalog items, pricing, MRP, stock levels, SKUs, aur approval status.
8. **`dbo.ProductVariants`**: Size, color, SKU variants aur variant-specific stock/pricing.

### C. Orders, Checkout & Logistics
9. **`dbo.Orders`**: Complete order lifecycle transactions (`Pending`, `Accepted`, `Packed`, `Dispatched`, `OutForDelivery`, `Delivered`, `Cancelled`, `Returned`).
10. **`dbo.OrderItems`**: Har order ke individual line items, quantity, aur unit prices.
11. **`dbo.Coupons`**: Promotional discount codes, minimum order value, aur usage limits.
12. **`dbo.Offers`**: Hero banner deals, flash sale discounts, aur validity dates.
13. **`dbo.StockReservations`**: Checkout ke dauran 10-minute concurrency stock hold locks taaki overselling na ho.
14. **`dbo.PincodeServiceabilities`**: Pincode-wise serviceable areas, delivery charges, aur ETA timeframes.
15. **`dbo.Riders`**: Delivery partners ki live availability aur GPS coordinates (`CurrentLatitude`/`CurrentLongitude`).
16. **`dbo.DeliveryProofs`**: Customer delivery ke waqt 4-digit handover OTP verification aur geotagged coordinates (Lat/Lng).
17. **`dbo.FailedDeliveryLogs`**: 3-strike delivery failover audit logs aur auto-RTO trigger history.

### D. Customer Engagement & Trust
18. **`dbo.Complaints`**: 3-party dispute and grievance investigation records.
19. **`dbo.Reviews`**: Verified buyer product ratings, reviews, aur feedback.
20. **`dbo.OrderEvidences`**: Unboxing photos, merchant packing slips, aur dispatch seal videos.
21. **`dbo.Wishlists`**: Customers ke saved-for-later items.
22. **`dbo.SearchHistories`**: Recent search keywords aur suggestion auto-complete logs.
23. **`dbo.ProductQuestions`**: Pre-purchase customer inquiries.
24. **`dbo.ProductAnswers`**: Sellers aur Admin dwara verified answers.
25. **`dbo.ChatMessages`**: Live buyer-seller instant messaging logs.
26. **`dbo.PriceDropAlerts`**: Customer target price alert triggers.
27. **`dbo.StockAlerts`**: Back-in-stock auto notifications.

### E. Wallet, Gift Cards & Loyalty
28. **`dbo.WalletAccounts`**: 4-balance digital wallet ledger (Main, Instant Refund, Promotional, Cashback).
29. **`dbo.WalletTransactions`**: Complete credit/debit audit transactions with reference IDs.
30. **`dbo.GiftCards`**: Prepaid digital vouchers, PINs, balance, aur expiry.
31. **`dbo.RewardPoints`**: Customer loyalty rewards points summary.
32. **`dbo.RewardPointsTransactions`**: Points earned (1 pt per ₹100) and redeemed history.

### F. Enterprise System Monitoring & Auditing (Point 174)
33. **`dbo.AuditLogs`**: Super-admin action audit trail (*Who $\rightarrow$ What $\rightarrow$ When $\rightarrow$ Where*).
34. **`dbo.LoginHistories`**: Har login attempt ka IP address, device type, OS, browser, aur success/failure status.
35. **`dbo.UserSessions`**: Live active sessions, session tokens, login time, last-seen heartbeat, aur active status.
36. **`dbo.UserActivities`**: Har user dwara perform kiya gaya controller action, HTTP method, page URL, aur request payload.
37. **`dbo.EntityChangeLogs`**: Record update hone par JSON format me Old Values vs New Values diff tracking.
38. **`dbo.SoftDeleteLogs`**: Deleted records ka JSON snapshot taaki Recycle Bin se 1-click me restore kiya ja sake.
39. **`dbo.EntityViewLogs`**: Seen/Unseen notifications aur order read receipts ka tracking.
40. **`dbo.SecurityThreatAlerts`**: Automated brute force detection, IP hopping, aur high-velocity fraud alerts.
41. **`dbo.Notifications`**: In-app notifications aur system alert messages.

---

## ⚙️ 3. Stored Procedures Catalog (40 Stored Procedures)

High performance ke liye system me 40 specialized Stored Procedures configured hain:

| # | Stored Procedure Name | Description & Functionality |
|---|:---|:---|
| 1 | `sp_GetActiveShops` | Active and approved shops ki list proximity sorting ke liye fetch karta hai. |
| 2 | `sp_GetProductsByShop` | Kisi specific shop ke active products, reviews count aur ratings calculate karta hai. |
| 3 | `sp_GetCustomerOrderHistory` | Customer ke recent 20 orders ki complete timeline aur status fetch karta hai. |
| 4 | `sp_GetAdminDashboardKPIs` | Super-admin dashboard ke main metrics (Users, Orders, Revenue, Pending Sellers) return karta hai. |
| 5 | `sp_GetHighReturnAreas` | Pincode-wise return rate analysis calculate karke fraud hotspots detect karta hai. |
| 6 | `sp_GetMonitoringOverviewStats` | Live System Monitoring center ke real-time counters (Online users, Logins, Orders) provide karta hai. |
| 7 | `sp_GetAllUsers` | Active platform users list fetch karta hai. |
| 8 | `sp_GetUserById` | Single user profile details retrieve karta hai. |
| 9 | `sp_CreateUser` | New user record insert karta hai. |
| 10 | `sp_UpdateUser` | User profile updates save karta hai. |
| 11 | `sp_DeleteUser` | Soft delete flag set karta hai. |
| 12 | `sp_GetAllShops` | Sabhi registered shops ki list return karta hai. |
| 13 | `sp_GetShopById` | Specific merchant shop details fetch karta hai. |
| 14 | `sp_CreateShop` | New shop registration record insert karta hai. |
| 15 | `sp_UpdateShop` | Shop details aur coordinates update karta hai. |
| 16 | `sp_DeleteShop` | Shop ko soft-delete mark karta hai. |
| 17 | `sp_CreateProduct` | New catalog item insert karta hai. |
| 18 | `sp_UpdateProduct` | Product price, stock, aur description update karta hai. |
| 19 | `sp_DeleteProduct` | Product ko soft-delete mark karta hai. |
| 20 | `sp_GetProductById` | Specific product detail fetch karta hai. |
| 21 | `sp_CreateOrder` | New order transaction insert karta hai. |
| 22 | `sp_CreateOrderItem` | Order line items insert karta hai. |
| 23 | `sp_GetOrderById` | Order full details retrieve karta hai. |
| 24 | `sp_UpdateOrderStatus` | Order stage update karta hai (`Packed`, `Dispatched`, etc.). |
| 25 | `sp_AssignRiderToOrder` | Order ke sath available rider map karta hai. |
| 26 | `sp_GetRiderActiveOrders` | Rider ke assigned undelivered orders fetch karta hai. |
| 27 | `sp_UpdateRiderLocation` | Rider ke real-time GPS coordinates (`Latitude`/`Longitude`) update karta hai. |
| 28 | `sp_VerifyDeliveryOtp` | Delivery handover ke 4-digit OTP ko verify karta hai. |
| 29 | `sp_GetWalletBalance` | Customer digital wallet ke 4 balances return karta hai. |
| 30 | `sp_CreditWallet` | Customer wallet me amount credit karta hai with transaction log. |
| 31 | `sp_DebitWallet` | Customer wallet se amount debit karta hai with transaction log. |
| 32 | `sp_LogUserActivity` | User activities table me action log record karta hai. |
| 33 | `sp_CreateUserSession` | New session token aur client telemetry register karta hai. |
| 34 | `sp_TerminateUserSession` | Session ko deactivate karta hai (Force Logout). |
| 35 | `sp_LogEntityChange` | Entity update hone par Old vs New values JSON record karta hai. |
| 36 | `sp_LogSoftDelete` | Entity delete hone par Recycle Bin me snapshot save karta hai. |
| 37 | `sp_RestoreSoftDeletedRecord` | Deleted record ko active state me restore karta hai. |
| 38 | `sp_GetActiveUserSessions` | Sabhi active logged-in sessions ki list return karta hai. |
| 39 | `sp_GetSecurityThreatAlerts` | Unresolved security alerts fetch karta hai. |
| 40 | `sp_ResolveSecurityThreat` | Threat alert ko resolved mark karta hai. |

---

## 🛡️ 4. System Monitoring Command Center Guide (`/Admin/SystemMonitoring`)

Super-Admin `/Admin/SystemMonitoring` par jaakar niche diye gaye features use kar sakta hai:

1. **Live Counters**:
   - Total Users, Online Users (Last 30 mins active), Today's Logins, Failed Logins, Today's Orders, Today's Activities.
2. **Active Sessions Management**:
   - Sabhi logged-in users ka IP, Device, OS, Browser, Login Time aur Last Seen time live dikhta hai.
   - Admin kisi bhi suspicious session par **"Force Logout"** click karke turant terminate kar sakta hai (`/Admin/ForceLogout`).
3. **Audit Trails & User Activities**:
   - Kaun user kis URL/Controller par kab gaya, kaunsa HTTP method use kiya, sab kuch timeline me recorded hai.
4. **Entity Diff Tracking (Old vs New)**:
   - Jab koi product, user, ya order change hota hai, toh kya value pehle thi aur ab kya hai, dono JSON format me dikhti hain.
5. **Recycle Bin & 1-Click Restore**:
   - Agar galti se koi product ya shop delete ho jaye, toh Admin Recycle Bin tab se **"Restore"** click karke bina database restore kiye record ko wapas la sakta hai (`/Admin/RestoreDeletedRecord`).
6. **CSV Audit Export**:
   - **"Export CSV"** button par click karke complete audit logs download kiye ja sakte hain compliance reporting ke liye.

---

## 💻 5. How to Compile, Setup & Run ShopNext

### Step 1: Database Setup
1. SQL Server Management Studio (SSMS) open karein.
2. Script open karein: `ShopNext/Database/ShopNext_Complete_Schema_And_Stored_Procedures.sql`
3. Execute karein. Isme 41 tables, 40 SPs, aur sample seed data automatically load ho jayega.

### Step 2: Connection String Configuration
`appsettings.json` file me apna SQL Server connection string check karein:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### Step 3: Build & Run Application
PowerShell terminal me project root par run karein:
```powershell
# 1. Compile solution (0 Warnings, 0 Errors)
dotnet build --nologo

# 2. Run the application
dotnet run
```

Application local URLs par active ho jayegi:
- **Customer Marketplace**: `http://localhost:5200`
- **Admin System Monitoring**: `http://localhost:5200/Admin/SystemMonitoring`
- **Admin Master Dashboard**: `http://localhost:5200/Admin/Dashboard`
- **Merchant Hub**: `http://localhost:5200/Vendor/Login`
- **Rider Portal**: `http://localhost:5200/Rider/Login`

### Default Login Credentials:
- **Super Admin**: `admin@shopnext.com` / `Admin@123` (Phone: `9999999999`)
- **Sample Customer**: `pooja@example.com` / `pass123` (Phone: `9999988888`)
- **Sample Merchant**: `abc.electronics@shopnext.com` / `123456` (Phone: `9876511223`)
- **Sample Rider**: `rider.amit@shopnext.com` / `123456` (Phone: `9876599881`)
