# 🛍️ ShopNext - Hyperlocal E-Commerce & Smart Delivery Platform

ShopNext ek modern, high-performance **Hyperlocal E-Commerce Platform** hai jo neighborhood stores (Groceries, Bakery, Electronics, Organic Fruits & Vegetables) ko local customers aur delivery riders se connect karta hai.

Platform me real-time geolocation-based shop discovery, instant marketplace search, saved address management, multi-mode payments (COD, UPI, Card, Net Banking), live Leaflet.js GPS delivery tracking, cancellation/return workflows, aur ratings/reviews system shamil hai.

---

## 🚀 Key Technologies & Stack

- **Backend**: ASP.NET Core MVC (Targeting `.NET 8.0`)
- **Database**: Microsoft SQL Server (`localhost\SQLEXPRESS`), Database: `ShopNext`
- **Data Access (ORM)**: Entity Framework Core 8.0 + High-performance Stored Procedures & Raw SQL
- **Front-End**: Bootstrap 5 Responsive Glassmorphic UI, Vanilla CSS, FontAwesome 6
- **Asynchronous Logic**: jQuery AJAX
- **Interactive Maps**: Leaflet.js (Open-Source Live Rider & Order Tracking)

---

## 📂 Project Structure

```
ShopNext/
│
├── Controllers/
│   ├── HomeController.cs           # Marketplace, Proximity Sorting, Product Details, Checkout, Order Tracking
│   ├── CustomerController.cs       # Register, Login, Profile, Saved Addresses, My Orders, Cancel, Return, Reviews
│   ├── VendorController.cs         # Merchant Register/Login, Product Catalog, Order Management, Rider Assignment
│   ├── AdminController.cs          # Store Auditing & Approval Dashboard
│   ├── RiderController.cs          # Rider Onboarding, Trip Management, GPS Telemetry Streaming
│   └── DatabaseSetupController.cs  # SQL Script & Stored Procedures Explorer
│
├── Models/
│   ├── User.cs                     # Customer/User entity (Email, Password, Role)
│   ├── CustomerAddress.cs          # Saved delivery addresses (Home/Work, Pincode, Landmark)
│   ├── Shop.cs                     # Registered stores (Location coords, Approval status)
│   ├── Product.cs                  # Store items (Category, Price, Stock status, Image)
│   ├── Order.cs                    # Orders (Multi-payment mode, status, cancel/return reasons)
│   ├── OrderItem.cs                # Line items inside purchase
│   ├── Rider.cs                    # Delivery partners & live GPS coordinates
│   ├── Review.cs                   # 1-5 star ratings & customer feedback
│   └── ShopNextDbContext.cs        # EF Core DbContext with relational configurations
│
├── Services/
│   ├── IShopNextService.cs         # Business & Data contracts
│   └── ShopNextService.cs          # EF Core & Stored Procedure implementations
│
├── Views/
│   ├── Home/                       # Index (Nearby Shops), Products (Marketplace), ProductDetails, Cart, OrderSuccess
│   ├── Customer/                   # Register, Login, Profile (Addresses), MyOrders (Tracking & Actions)
│   ├── Vendor/                     # Products Catalog, Orders Management, Register, Login
│   ├── Admin/                      # Store Approval Dashboard, Login
│   ├── Rider/                      # Delivery Dashboard, Location Simulator, Register, Login
│   └── Shared/                     # _Layout.cshtml (Glassmorphic responsive nav & footer)
│
├── wwwroot/
│   └── sql/
│       ├── ShopNext_Complete_DB_Setup.sql  # Master All-in-One SQL Script for any machine
│       └── database_setup.sql              # Database reference script
│
├── appsettings.json                # Database connection string configuration
└── Program.cs                      # Service registration & middleware pipeline
```

---

## 🗄️ Database Setup (1-Click Run on Any Machine)

Aap kisi ko bhi ye project dein, wo bina kisi dikkat ke SQL Server me database create kar sakte hain:

### Option A: Via PowerShell / Command Prompt (Fastest)
SQL Server Local Instance (`localhost\SQLEXPRESS`) par script run karne ke liye:

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -i "wwwroot\sql\ShopNext_Complete_DB_Setup.sql"
```

### Option B: Via SQL Server Management Studio (SSMS)
1. SSMS open karein aur connect karein: `localhost\SQLEXPRESS`
2. File menu me jakar open karein: `wwwroot/sql/ShopNext_Complete_DB_Setup.sql`
3. **Execute (F5)** dabayein.
4. Database `ShopNext`, sabhi 8 tables, stored procedures, aur rich seed data ready ho jayega.

---

## ⚙️ Connection String Configuration

File: `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ShopNext;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## 🏃‍♂️ How to Run the Application

1. **Build the project**:
   ```powershell
   dotnet build
   ```

2. **Run the server**:
   ```powershell
   dotnet run --launch-profile http
   ```

3. Browser me open karein:
   👉 **`http://localhost:5200`**

---

## 🔑 Pre-Configured Test Logins (Demo Data)

| Role | Login URL | Identifier / Phone | Password | Features Available |
| :--- | :--- | :--- | :--- | :--- |
| 👤 **Customer** | `/Customer/Login` | `9999988888` | `pass123` | Cart, Saved Addresses, Order, Track, Cancel, Return, Review |
| 🏪 **Vendor (Shop)** | `/Vendor/Login` | `9811111111` | `pass123` | Green Mart - Manage Products, Accept Orders, Assign Rider |
| 🏪 **Vendor (Bakery)**| `/Vendor/Login` | `9822222222` | `pass123` | BakeHouse - Artisanal breads, pastries, customer orders |
| 🛵 **Rider** | `/Rider/Login` | `9876543210` | - | Ajay Kumar - Accept trip, Stream GPS, Mark Delivered |
| 🛡️ **Admin** | `/Admin/Login` | Admin Key | `admin123` | Store approval & audit dashboard |

---

## 🔒 Security & Version Control Notice

> **Important**: Is project ko kisi public ya private GitHub repository par push **nahi** kiya jana hai (strict user constraint). Sabhi local copies system par hi surakshit hain.
