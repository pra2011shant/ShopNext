# 🏗️ ShopNext - Complete System Design & Database Architecture

## 📌 Executive Summary
**ShopNext** is an enterprise-grade Hyperlocal E-Commerce & Multi-Vendor Marketplace built with **ASP.NET Core (.NET 8/9)**, **Entity Framework Core**, and **Microsoft SQL Server**. It connects local neighborhood brick-and-mortar merchants with nearby customers for fast same-day deliveries, live GPS order tracking, intelligent rider dispatch, automated return/refund management, and platform governance.

---

## 🏛️ 1. High-Level System Architecture

```mermaid
graph TD
    subgraph Client_Layer ["Client & Presentation Layer (HTML5, Razor, Vanilla CSS, JS)"]
        UI_Customer["🛒 Customer Portal (Search, Cart, Checkout, Live Tracking)"]
        UI_Vendor["🏪 Seller Dashboard (Orders Pipeline, Products, Analytics)"]
        UI_Admin["👑 Master Admin Hub (Approvals, Fraud Prevention, Metrics)"]
        UI_Rider["🛵 Rider Portal (Delivery Tasks, Navigation, Proof of Delivery)"]
    end

    subgraph Security_Layer ["Security, Auth & Middleware Layer"]
        AuthFilter["🔐 Role-Based Cookie Authentication (Admin, Seller, Rider, Customer)"]
        AntiCSRF["🛡️ Anti-Forgery & Token Validation"]
        AuditMiddleware["📝 Real-Time System Audit & Request Logging"]
        LocEngine["🌐 Multi-Language Client Engine (Hindi, English, Bengali, etc.)"]
    end

    subgraph Controller_Layer ["ASP.NET Core Controller Layer"]
        C_Home["HomeController (Marketplace, Cart, Geolocation)"]
        C_Customer["CustomerController (Account, Orders, Reviews)"]
        C_Vendor["VendorController (Catalog, Order Pipeline, Sales)"]
        C_Admin["AdminController (Platform Governance, Clusters, Payouts)"]
        C_Rider["RiderController (Accept, Pickup, OTP Verification)"]
    end

    subgraph Service_Layer ["Business Logic & Application Services"]
        S_ShopNext["IShopNextService (Core Marketplace Engine)"]
        S_Advanced["IAdvancedEcommerceService (Fraud, Multi-Category, Pincodes)"]
        S_Audit["IAuditService (System Monitoring & Audit Logs)"]
        S_Loc["ILocalizationService (Server-Side Culture Management)"]
    end

    subgraph Data_Layer ["Data Access & ORM Layer"]
        EF_Context["Entity Framework Core (ShopNextDbContext)"]
        SP_Engine["Raw SQL & Stored Procedures (Complex Reports, Payouts)"]
    end

    subgraph Database_Layer ["SQL Server Database (ShopNextDb)"]
        DB_Users[("Users / Customers")]
        DB_Shops[("Shops / Merchants")]
        DB_Catalog[("Categories, Products, Variants")]
        DB_Orders[("Orders, OrderItems, Addresses")]
        DB_Logistics[("Riders, Deliveries, OTPs")]
        DB_Compliance[("Complaints, Reviews, AuditLogs")]
    end

    UI_Customer --> Security_Layer
    UI_Vendor --> Security_Layer
    UI_Admin --> Security_Layer
    UI_Rider --> Security_Layer

    Security_Layer --> Controller_Layer
    Controller_Layer --> Service_Layer
    Service_Layer --> Data_Layer
    Data_Layer --> Database_Layer
```

---

## 🗄️ 2. Complete Entity-Relationship (ER) Database Diagram

```mermaid
erDiagram
    USERS ||--o{ SHOPS : owns
    USERS ||--o{ ORDERS : places
    USERS ||--o{ CUSTOMER_ADDRESSES : saves
    USERS ||--o{ REVIEWS : writes
    USERS ||--o{ NOTIFICATIONS : receives
    USERS ||--o{ AUDIT_LOGS : performs

    SHOPS ||--o{ PRODUCTS : sells
    SHOPS ||--o{ ORDERS : receives
    SHOPS ||--o{ OFFERS : creates
    SHOPS ||--o{ COMPLAINTS : involved_in

    CATEGORIES ||--o{ PRODUCTS : categorizes
    
    PRODUCTS ||--o{ PRODUCT_VARIANTS : has
    PRODUCTS ||--o{ ORDER_ITEMS : contains
    PRODUCTS ||--o{ REVIEWS : rated_in

    ORDERS ||--o{ ORDER_ITEMS : includes
    ORDERS ||--o| RIDERS : assigned_to
    ORDERS ||--o| CUSTOMER_ADDRESSES : delivers_to
    ORDERS ||--o{ ORDER_EVIDENCES : tracks_proof
    ORDERS ||--o{ COMPLAINTS : disputed_in

    COUPONS ||--o{ ORDERS : discounts

    USERS {
        int Id PK
        string FullName
        string Email
        string PhoneNumber
        string PasswordHash
        string Role "Admin, Seller, Rider, Customer"
        bool IsActive
        string RestrictionLevel "Normal, Warning, Blocked"
        datetime CreatedAt
    }

    SHOPS {
        int Id PK
        int UserId FK
        string ShopName
        string OwnerName
        string PhoneNumber
        string Category
        string Address
        string City
        string State
        string Pincode
        decimal Latitude
        decimal Longitude
        bool IsApproved
        bool IsActive
        decimal CommissionRate
        datetime CreatedDate
    }

    CATEGORIES {
        int Id PK
        string CategoryName
        string Description
        string IconClass
        bool IsActive
    }

    PRODUCTS {
        int Id PK
        int ShopId FK
        int CategoryId FK
        string ProductName
        string Description
        decimal Price
        decimal Mrp
        int StockQuantity
        string ImageUrl
        bool IsActive
        bool IsDeleted
        datetime CreatedDate
    }

    PRODUCT_VARIANTS {
        int Id PK
        int ProductId FK
        string VariantName "e.g. 500g, 1kg, Red, XL"
        string Sku
        decimal Price
        int StockQuantity
        bool IsActive
    }

    ORDERS {
        int Id PK
        int CustomerId FK
        int ShopId FK
        int RiderId FK
        decimal TotalAmount
        decimal DiscountAmount
        decimal DeliveryFee
        string OrderStatus "Pending, Accepted, Processing, Packed, Shipped, Delivered, Cancelled, ReturnRequested, RefundCompleted"
        string PaymentMode "COD, UPI, Card, NetBanking"
        string PaymentStatus "Pending, Paid, Failed, Refunded"
        string DeliveryAddress
        string DeliveryPincode
        string CancelReason
        string ReturnReason
        string DeliveryOtp
        datetime CreatedDate
        datetime UpdatedDate
    }

    ORDER_ITEMS {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal TotalPrice
    }

    CUSTOMER_ADDRESSES {
        int Id PK
        int CustomerId FK
        string AddressType "Home, Office, Other"
        string RecipientName
        string PhoneNumber
        string AddressLine
        string City
        string State
        string Pincode
        decimal Latitude
        decimal Longitude
        bool IsDefault
    }

    RIDERS {
        int Id PK
        int UserId FK
        string RiderName
        string PhoneNumber
        string VehicleType
        string VehicleNumber
        string Status "Available, Busy, Offline"
        decimal CurrentLatitude
        decimal CurrentLongitude
        bool IsApproved
    }

    REVIEWS {
        int Id PK
        int ProductId FK
        int ShopId FK
        int CustomerId FK
        int Rating "1 to 5 Stars"
        string Comment
        bool IsVerifiedPurchase
        datetime CreatedDate
    }

    COUPONS {
        int Id PK
        string Code
        string DiscountType "Percentage, Flat"
        decimal DiscountValue
        decimal MinOrderAmount
        decimal MaxDiscount
        datetime ExpiryDate
        bool IsActive
    }

    AUDIT_LOGS {
        int Id PK
        int UserId FK
        string Action
        string EntityName
        string EntityId
        string Details
        string IpAddress
        datetime Timestamp
    }
```

---

## 📋 3. Database Schema & Tables Specification

### 3.1. Core Identity & Access Tables
1. **`Users`**: Central account table storing user credentials, hashed passwords, contact details, multi-role claims (`Customer`, `Seller`, `Rider`, `Admin`), suspension status, and risk flags.
2. **`PasswordResetTokens`**: Secure time-bound cryptographic tokens for self-service password recovery.
3. **`CustomerAddresses`**: Multiple saved shipping destinations for 1-click checkout with GPS coordinates (`Latitude`, `Longitude`).

### 3.2. Merchant & Catalog Tables
1. **`Shops`**: Merchant storefronts with geocoding (`Latitude`, `Longitude`), business categories, tax/GST compliance, approval state, and bank account settlement info.
2. **`Categories`**: Hierarchy classification (Grocery, Electronics, Dairy, Fruits & Vegetables, Bakery, Fashion, etc.).
3. **`Products`**: Multi-merchant catalog items with MRP, Selling Price, dynamic inventory stock, and image galleries.
4. **`ProductVariants`**: Specific item sizes, weights, and color configurations (e.g., 500g, 1kg, 5L, Size L).

### 3.3. Order Processing & Transaction Tables
1. **`Orders`**: Master transaction table tracking order financial breakdown, payment modes (COD, UPI, Card), delivery address, order status lifecycle, and OTPs.
2. **`OrderItems`**: Line items for each order preserving snapshot unit prices and quantities at purchase time.
3. **`Coupons`**: Promotional campaign discount codes with validity windows, minimum purchase thresholds, and max discount caps.
4. **`Offers`**: Flash sales and "Today's Deals" with discounted prices and real-time countdown timers.

### 3.4. Logistics & Fulfillment Tables
1. **`Riders`**: Hyperlocal delivery partners with live vehicle info, availability state, and live GPS coordinates.
2. **`OrderEvidences`**: Delivery confirmation photo proof, signature uploads, and delivery dispute resolution records.

### 3.5. Quality, Governance & Security Tables
1. **`Reviews`**: Customer ratings (1-5 stars) and feedback for products and shops.
2. **`Complaints`**: Order disputes, item damaged claims, and seller/customer dispute tracking.
3. **`Notifications`**: Real-time customer, merchant, and rider order alerts and system announcements.
4. **`AuditLogs`**: Platform-wide immutable audit trail of administrative actions, payouts, status updates, and security events.

---

## 🔄 4. Core Workflows & Data Pipelines

### 4.1. Order Lifecycle Pipeline (Feature 25)
```mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant App as ShopNext Web App
    participant DB as SQL Database
    actor Seller
    actor Rider

    Customer->>App: Places Order (COD/Online)
    App->>DB: Insert Order (Status: Pending) & OrderItems
    DB-->>Seller: Real-Time Notification (New Order Received)
    
    Seller->>App: Clicks [1. Accept Order]
    App->>DB: Update Status -> "Accepted"
    
    Seller->>App: Clicks [2. Start Processing]
    App->>DB: Update Status -> "Processing"
    
    Seller->>App: Clicks [3. Mark as Packed]
    App->>DB: Update Status -> "Packed"
    
    Seller->>App: Selects Rider & Clicks [4. Assign & Ship]
    App->>DB: Update Status -> "Shipped" / "Dispatched" (RiderId Assigned)
    
    Rider->>App: Pick up items from Store & En Route
    Customer->>App: Views Live Tracking Page (Leaflet GPS Map)
    
    Rider->>Customer: Delivers items at Doorstep & Verifies OTP
    Rider->>App: Submits Delivery Confirmation
    App->>DB: Update Status -> "Delivered" / "Completed"
    
    Customer->>App: Leaves Rating & Review (1-5 Stars)
    App->>DB: Insert Product/Shop Review
```

---

### 4.2. GPS Proximity & Distance Calculation (Haversine Algorithm)
When a customer browses approved shops:
$$\text{Distance} = 2 R \cdot \arcsin\left(\sqrt{\sin^2\left(\frac{\Delta\text{lat}}{2}\right) + \cos(\text{lat}_1)\cos(\text{lat}_2)\sin^2\left(\frac{\Delta\text{lon}}{2}\right)}\right)$$
- `R = 6371 km` (Earth's mean radius)
- Verified approved shops are dynamically queried from `Shops`, ordered by shortest distance to customer's detected GPS pin, and rendered on interactive maps.

---

## 🛡️ 5. Security & Data Integrity Guarantees
1. **Password Security**: Cryptographic hashing (PBKDF2 / SHA-256 with salts).
2. **SQL Injection Prevention**: 100% Parameterized EF Core queries & Dapper Stored Procedures.
3. **XSS & CSRF Protection**: Encoded Razor output and token verification.
4. **Role Isolation**: Strict Controller-level authorization policies (`[Authorize(Roles = "...")]`).
5. **Multi-Language Architecture**: 0-delay client DOM translation engine with server cookie synchronization.
