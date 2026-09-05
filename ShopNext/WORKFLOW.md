# ShopNext — Complete Architecture, Workflow & Technical Guide

---

## 1. System Architecture & Overview

ShopNext is an enterprise-grade **Hyperlocal Multi-Vendor E-Commerce Platform** built with:
- **Backend Framework**: ASP.NET Core 8.0 MVC + C#
- **ORM & Data Access**: Entity Framework Core + Native ADO.NET for Stored Procedures
- **Database**: Microsoft SQL Server (`localhost\SQLEXPRESS`, Database: `ShopNext`)
- **Frontend**: HTML5, Vanilla CSS3 (Custom Glassmorphic Design System), Bootstrap 5, FontAwesome 6, Google Fonts (Outfit & JetBrains Mono)
- **Mapping & Geolocation**: Leaflet.js + OpenStreetMap (OSM)
- **API Engine**: Standardized RESTful JSON APIs under `/api/v1/`

---

## 2. Database Consolidation (Single Master Script)

### A. Single-Script Rule
To avoid confusion and eliminate scattered partial migration files, the entire database schema, audit columns, relationships, Stored Procedures, and seed data are consolidated into **ONE master file**:
- **Master Script Path**: `wwwroot/sql/ShopNext_Complete_DB_Setup.sql` (also mirrored as `database_setup.sql`)
- **Redundant Files Removed**: All partial migration fragments (`migration_myaccount.sql`, `rider_setup.sql`, `seed_recently_viewed_products.sql`) were merged into this single script and cleaned up.

### B. Execution Options
1. **Via SSMS (SQL Server Management Studio)**:
   - Open SSMS, connect to `localhost\SQLEXPRESS`, open `ShopNext_Complete_DB_Setup.sql`, and execute (F5).
2. **Via Web Admin Console (`/DatabaseSetup`)**:
   - Navigate to `/DatabaseSetup` as Admin.
   - Click **[1-Click Run Setup]** to automatically parse batches and execute against SQL Server via EF Core.
   - Click **[Download Master .sql]** to download the single script anytime.

### C. UI Data Security Policy
- **No SQL/SP Exposure on Public UI**: Raw SQL queries, database structures, or Stored Procedure texts are **strictly prohibited** on customer and seller pages (Home, Shop, Cart, Order Tracking, Seller Dashboard).
- The `/DatabaseSetup` route is an internal developer and admin operations tool protected by role-based authentication and displays a clean operational dashboard rather than dumping raw code.

---

## 3. Real Dynamic Data (Zero Hardcoding Policy)

All key modules operate on **100% real-time SQL database queries**:
- **Seller Dashboard Metrics**:
  - `Total Products`: Real count of active products (`products.Count`).
  - `Total Orders`: Real count of store orders (`orders.Count`).
  - `Pending Orders`: Real count of orders with status `Placed`, `Confirmed`, `Packed`, or `Pending`.
  - `Total Sales`: Real sum of completed/delivered/paid orders.
  - `Pending Returns`: Real count of return requests.
  - `Low Stock`: Real count of products with inventory `<= 5` or `OutOfStock`.
- **Product Catalog**: Live DB records matching store ID and approval flags.
- **Order Lifecycle**: Persisted status updates with audit columns (`Remark`, `CreatedDate`, `IsActive`, `IsDeleted`).

---

## 4. UI Full-Display & Responsive Design Standards

- **Full Viewport Utilization**: The application layout uses `container-fluid px-3 px-md-5` providing edge-to-edge readability on ultrawide monitors (1920px+), standard laptops, and tablets.
- **Mobile First & Touch-Friendly**:
  - Offcanvas and collapsible navigation menus for smartphones.
  - Touch-friendly button sizes (`min-height: 44px`).
  - Responsive cards with fluid grid systems (`col-12 col-md-6 col-lg-4 col-xl-3`).
  - Sticky bottom order actions and cart summaries on mobile screens.
- **Glassmorphism Theme**: Dark-mode aesthetic with backdrop blur (`backdrop-filter: blur(16px)`), subtle neon glow accents, and CSS micro-animations.

---

## 5. Hyperlocal Map & Geolocation Tracking

### How Maps Work Without Google Maps API Keys
Instead of requiring expensive Google Maps API keys with billing restrictions, ShopNext uses **Leaflet.js + OpenStreetMap (OSM)**:
1. **Map Engine**: Free, open-source tile servers from OpenStreetMap (`https://tile.openstreetmap.org/{z}/{x}/{y}.png`).
2. **Interactive Markers**:
   - **Store Marker**: Blue store icon displaying merchant name, category, and address.
   - **Customer Destination**: Amber home icon indicating delivery address.
   - **Rider Marker**: Animated green motorbike icon showing current delivery partner location.
3. **Route Polyline**: Dynamic polyline connecting Shop → Rider → Customer.
4. **Live Polling**:
   - The tracking interface (`OrderSuccess.cshtml`) polls every 4 seconds to check if the rider is moving.
   - Smooth marker repositioning (`marker.setLatLng([newLat, newLng])`) and auto pan (`map.panTo(...)`).

---

## 6. RESTful API Endpoints (`/api/v1/`)

ShopNext provides a clean RESTful API suite in `ApiController.cs` for AJAX interfaces, mobile apps, and GPS devices:

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/orders/{orderId}/tracking` | Real-time order status, rider details, shop coords, and ETA |
| `POST` | `/api/v1/riders/{riderId}/location` | Ingests live GPS coordinates (`latitude`, `longitude`) from delivery partner |
| `GET` | `/api/v1/products/search?q={query}` | Instant product & store autocomplete search |
| `GET` | `/api/v1/shops/nearby?lat={lat}&lng={lng}` | Haversine formula calculation for shops within delivery radius |
| `GET` | `/api/v1/seller/{shopId}/summary` | Live seller KPI counts for the merchant dashboard

### Workflow 21: 36. Brand Management (Samsung, Apple, Nike, Adidas, HP, Dell, Lenovo & [Add], [Edit], [Delete], [View])
Comprehensive partner brands governance and directory console for platform administrators:
1. **The 7 Core Featured Partner Brands**:
   - 🌟 **Samsung**: Electronics & Mobiles (Smartphones, Smart TVs, Soundbars & Appliances) &bull; Rating: `4.8★`
   - 🍏 **Apple**: Mobiles & Tech (iPhones, MacBooks, iPads, AirPods & Ecosystem) &bull; Rating: `4.9★`
   - 👟 **Nike**: Fashion & Sportswear (Athletic Footwear, Performance Gear & Sneakers) &bull; Rating: `4.8★`
   - ⚽ **Adidas**: Fashion & Streetwear (Three-Stripes Performance Shoes, Apparel & Accessories) &bull; Rating: `4.7★`
   - 💻 **HP**: Computers (Laptops, Gaming Desktops, Monitors & Printers) &bull; Rating: `4.6★`
   - 🖥️ **Dell**: Computers (Workstations, XPS Ultrabooks, Enterprise Hardware) &bull; Rating: `4.7★`
   - 📱 **Lenovo**: Computers & Tablets (ThinkPad Business Laptops, Yoga Convertibles & Legion) &bull; Rating: `4.6★`
2. **Interactive Admin Action Controls**:
   - ➕ **`[Add Brand]`**: Opens `#addBrandModal` with brand title, category selector, logo image URL with real-time preview, description, initial rating, and active standing (`POST /Admin/AddBrand`).
   - 👁️ **`[View]`**: Opens `#brandDetailsModal` with complete brand profile, high-res logo, category badge, product count, rating score, and onboarding timestamp (`GET /Admin/GetBrandDetails`).
   - ✏️ **`[Edit]`**: Opens `#editBrandModal` with pre-filled brand metadata to modify name, category, logo URL, rating, description, and status (`POST /Admin/EditBrand`).
   - 🗑️ **`[Delete]`**: Opens `#deleteBrandModal` with confirmation prompt before unlinking from directory (`POST /Admin/DeleteBrand`).
3. **Backend Endpoints**:
   - `GET /Admin/GetBrandDetails?id={id}`: Fetches brand JSON metadata.
   - `POST /Admin/AddBrand`: Inserts new brand record into SQL database.
   - `POST /Admin/EditBrand`: Updates brand attributes and status.
   - `POST /Admin/DeleteBrand`: Removes brand from partner directory.

---

## 7. Complete End-to-End Business Workflows

### Workflow 1: Customer Order Lifecycle & Tracking
```
Order Placed (COD/Online)
       ↓
Confirmed by Seller
       ↓
Packed & Sealed
       ↓
Dispatched (Express Delivery Partner Assigned)
       ↓
Out for Delivery (Live GPS Tracking Active on Map)
       ↓
Delivered (Completed)
```

### Workflow 2: Order Cancellation Rules
- **Placed** → Cancel Allowed
- **Confirmed** → Cancel Allowed
- **Packed** → Cancel Allowed
- **Shipped / Out for Delivery** → Cancellation NOT Allowed (Directs to Return after delivery)
- **Cancellation Reasons**: `Wrong Product`, `Changed Mind`, `Ordered by Mistake`, `Found Cheaper`, `Other`

### Workflow 3: Return & Refund Pipeline
- Available **strictly for Delivered orders**.
- **Return Reasons**: `Damaged`, `Wrong Product`, `Defective`, `Missing Item`, `Not as Described`.
- **Status Progression**:
```
Return Requested
       ↓
Return Approved
       ↓
Product Pickup
       ↓
Product Received
       ↓
Refund Initiated
       ↓
Refund Completed
```

### Workflow 4: Verified Review System (Anti-Fake Reviews)
- Reviews can **only** be submitted if the customer has a verified delivered order for that product/store.
- Star ratings (1 to 5) + feedback comments.

### Workflow 5: Seller Onboarding & Admin Approval
1. Seller registers via `/Vendor/Register` (Owner Name, Shop Name, Email, Mobile, Password, Address, Pincode).
2. Status set to `IsApproved = 0` (Pending Admin Approval). Seller cannot sell products immediately.
3. Admin views pending requests in Admin Portal (`/Admin/Dashboard`).
4. Admin reviews details and clicks **[Approve]** or **[Reject]**.
5. Once approved, Seller logs in and accesses the full **Seller Sidebar Dashboard**.

### Workflow 6: 11-Field Add Product
Seller creates products with complete specifications:
1. `Product Name`
2. `Category`
3. `Sub Category`
4. `Brand`
5. `Description`
6. `MRP (₹)`
7. `Selling Price (₹)`
8. `Discount (%)` (Auto-calculated: `((MRP - SellingPrice) / MRP) * 100`)
9. `Stock` (Auto-categorized into `InStock`, `LowStock`, `OutOfStock`)
10. `SKU` (Auto-generated if empty)
11. `Product Images` (Multiple angles support)
12. Action: `[Add Product]` saves to SQL Server and refreshes inventory.

### Workflow 7: 21. Product Images (Multiple Angles)
Supports 5 dedicated image angles per product with real-time browser preview:
1. 🌟 **Main Image** (`ImageUrl`): Primary cover photo used in marketplace listings and search results.
2. 📸 **Front** (`ImageFront`): Direct front-facing product view.
3. 🔄 **Back** (`ImageBack`): Back specifications/labels view.
4. 📐 **Side** (`ImageSide`): Side profile / thickness angle.
5. 📦 **Packaging** (`ImagePackaging`): Box, packaging, and seal verification view.

**Customer Gallery Experience**:
- On `ProductDetails.cshtml`, customers view the hero image with an interactive multi-angle thumbnail tray.
- Clicking any angle (Front, Back, Side, Packaging) dynamically switches the main view and highlights the active angle badge.

### Workflow 8: 22. Product Variants (Size, Color, Price, Stock, SKU)
Professional e-commerce multi-variant engine allowing individual products (e.g. T-Shirts) to have custom matrix combinations:

1. **Variant Attributes**:
   - **Size**: `S`, `M`, `L`, `XL`, `XXL`, Custom
   - **Color**: `Black`, `White`, `Blue`, `Red`, `Green`, Custom
   - **Price (₹)**: Individual price per variant (e.g. XL can have a higher price than S).
   - **Stock**: Individual stock count per variant. Base product stock reflects total sum.
   - **SKU**: Unique SKU code per variant (e.g. `TSHIRT-BLK-S`, `TSHIRT-BLU-L`).

2. **Seller Experience (Add/Edit Product)**:
   - **Enable Variants Toggle**: Switch between standard single-stock product and multi-variant matrix.
   - **Quick Matrix Generator**: One-click combination generator from selected sizes and colors with default price and stock.
   - **Interactive Variant Table**: Sellers can fine-tune Price, Stock, SKU, or add/remove custom variant rows on the fly.
   - **Database Persistence**: Saved in relational table `dbo.ProductVariants` linked by `ProductId` with cascade integrity.

3. **Customer Experience (`ProductDetails.cshtml`)**:
   - **Interactive Size & Color Pills**: Customers select Size (`S`, `M`, `L`, `XL`) and Color (`Black`, `White`, `Blue`).
   - **Real-Time Dynamic Updates**: Instantaneous UI updates of variant price (`₹`), stock status (`In Stock: X units` or `Out of Stock`), and variant SKU badge.
   - **Cart & Order Precision**: Adds exact selected variant (`Size`, `Color`, `VariantPrice`, `VariantSKU`) to the shopping cart and checkout.

### Workflow 9: 23. Inventory (Product, Stock, Status & ⚠ Low Stock Alert)
Real-time stock monitor and threshold alerting system for merchant catalog management:
1. **Inventory Table**: `Product`, `Stock`, `Status`, `Action`.
2. **Threshold Logic**:
   - `Stock >= 5`: **`Available`** (Green badge)
   - `Stock < 5 && Stock > 0`: **`⚠ Low Stock`** (Amber alert badge)
   - `Stock == 0`: **`Out of Stock`** (Red danger badge)
3. **Seed Product Verification**:
   - `Mobile`: Stock = `20` &rarr; Status = `Available`
   - `Laptop`: Stock = `3` &rarr; Status = `⚠ Low Stock`
   - `Headphone`: Stock = `0` &rarr; Status = `Out of Stock`
4. **Interactive Action**: Real-time AJAX stock updater `POST /Vendor/UpdateStock`.

### Workflow 10: 24. Seller Orders (Incoming Order Notification, Delivery Address & [Accept Order])
End-to-end seller order reception and acceptance workflow:
1. **Incoming Order Presentation**:
   - **Order #**: e.g. `Order #1001`
   - **Product**: `Samsung Mobile`
   - **Qty**: `2`
   - **Amount**: `₹50,000`
   - **Delivery Address**: Full recipient address with landmark and pincode.
   - **Status**: `Pending`
2. **Seller Action & Progression**:
   - Button: **`[Accept Order]`**
### Workflow 11: 25. Order Processing (Accept &rarr; Processing &rarr; Packed &rarr; Shipped &amp; Automatic Customer Status Sync)
Complete 4-stage sequential order processing pipeline with real-time customer status synchronization:
1. **Seller 4-Stage Sequential Milestones**:
   - **Stage 1: Accept** (`Pending` &rarr; click `[Accept Order]` &rarr; status becomes `Accepted`)
   - **Stage 2: Processing** (`Accepted` &rarr; click `[Start Processing]` &rarr; status becomes `Processing`)
   - **Stage 3: Packed** (`Processing` &rarr; click `[Mark as Packed]` &rarr; status becomes `Packed`)
   - **Stage 4: Shipped** (`Packed` &rarr; click `[Ship Order]` or assign delivery rider &rarr; status becomes `Shipped`)
2. **Visual 4-Step Stepper Component**:
   - Integrated into each order card in `Views/Vendor/Orders.cshtml` and `Views/Vendor/Dashboard.cshtml`.
   - Dynamic step progression (`1. Accept` &rarr; `2. Processing` &rarr; `3. Packed` &rarr; `4. Shipped`) with real-time icons, completion checkmarks, and active indicators without requiring manual page reload.
3. **Automatic Real-Time Customer Status Visibility ("Customer ko automatically status dikhe")**:
   - **Customer Tracking (`OrderSuccess.cshtml`)**:
     - 7 sequential tracking stages: `Order Placed` &rarr; `Order Accepted` &rarr; `Processing` &rarr; `Packed` &rarr; `Shipped` &rarr; `Out for Delivery` &rarr; `Delivered`.
     - Client-side auto-polling via AJAX (`/Home/GetOrderRiderLocation`) every 3 seconds.
     - Live status change detection automatically advances timeline indicators, lights up pulse radar radar rings, changes header badges, and pops a floating alert toast: `"Order status updated to: [Processing]!"`.
   - **Customer Order History (`MyOrders.cshtml`)**:
     - Visual badge styling for `Accepted`, `Processing`, `Packed`, and `Shipped`.
     - Active orders auto-sync polling every 4 seconds dynamically updating status badges on the screen without reloading.

### Workflow 12: 27. Seller Reviews (Product Reviews, Rating, Comment, Customer Name, Date)
Dedicated seller reviews hub and real-time customer feedback monitor:
1. **Key Information Displayed to Seller**:
   - **Section**: `Product Reviews`
   - **Product Name**: e.g. `Samsung Mobile` (with category, SKU, and icon)
   - **Visual Rating Stars**: `⭐⭐⭐⭐⭐` (Dynamic 1-5 golden glowing stars)
   - **Review Comment**: `"Very good product"` (Formatted as high-contrast quoted customer feedback)
   - **Customer Name**: `Pooja Sharma` / `Verified Customer` (with verified buyer badge)
   - **Date**: `05 Sep 2026` (Formatted timestamp with relative time)
2. **Interactive Seller Features**:
   - **Dedicated Route**: `/Vendor/Reviews` with glassmorphic metrics overview (Average Rating, Total Reviews, Breakdown bars for 5★ to 1★).
### Workflow 13: 28. Seller Sales (Today's Sales, Weekly Sales, Monthly Sales, Yearly Sales)
Complete time-period financial analytics and revenue telemetry for merchants:
1. **The 4 Core Revenue Metrics**:
   - 📅 **Today's Sales**: Real-time gross billing generated by today's customer purchases (`DateTime.Today`).
   - 📊 **Weekly Sales**: Rolling 7-day performance with volume trends (`DateTime.Today.AddDays(-7)`).
   - 🗓️ **Monthly Sales**: Current 30-day billing cycle sales (`DateTime.Today.AddDays(-30)`).
   - 📈 **Yearly Sales**: Annual cumulative revenue across the fiscal calendar (`DateTime.Today.Year`).
2. **Interactive Seller Experience & Chart.js Visualization**:
   - **Dedicated Sales Hub**: `/Vendor/Sales` with period cards, comparative growth visualizer progress bars, Average Order Value (AOV), payment method distribution (Digital vs COD), and dynamic **Chart.js** line, donut, and bar graphs with period toggles (Today, Weekly, Monthly, Yearly).
   - **Dashboard Integration**: Live `#tab-earnings` tab in `/Vendor/Dashboard`.
   - **Recent Transactions Log**: Live customer sales order table showing Order #, Items, Customer, Payment Mode, Amount (₹), and Timestamp.

### Workflow 14: 29. Seller Earnings (Total Sales, Commission, Refund, Net Earnings)
Comprehensive settlement and financial statement ledger for merchants:
1. **Financial Settlement Formula**:
   ```
   Total Sales (Gross)     : ₹1,00,000
   Platform Commission (5%): - ₹5,000
   Refund Deductions       : - ₹2,000
   -----------------------------------
   Net Earnings (Payout)   :   ₹93,000
   ```
2. **Key Capabilities**:
   - **Dedicated Earnings Route**: `/Vendor/Earnings` displaying the exact settlement card, pending payout balances, bank account details, and order-by-order financial breakdowns.
   - **Dashboard Integration**: Live Net Earnings metric and direct navigation link in `/Vendor/Dashboard`.
   - **Real-Time Settlement Ledger**: Tabular reconciliation showing Order #, Gross Amount, 5% Commission, Refund Deductions, and Net Seller Payout per transaction.

### Workflow 15: 30. Seller Shop Profile (Store Discovery, Rating, Metrics & [View Shop])
Complete customer-facing merchant storefront presentation:
1. **Core Information Presented to Customers**:
   - 🏪 **Store Name**: e.g. `ABC Electronics` (with dynamic category badge & avatar)
   - ⭐ **Rating Score**: `⭐ 4.5 Rating` (Golden stars with aggregate rating & customer feedback count)
   - 📦 **Total Products**: `120` (Live inventory catalog items)
   - 🛍️ **Total Orders**: `500` (Delivered customer orders fulfilled)
   - 📍 **Location**: `Patna` (Store neighbourhood / city location)
   - 🔘 **Action CTA**: **`[View Shop]`** (Direct storefront catalog browser)
2. **Multi-Touchpoint Visibility**:
   - **Dedicated Storefront Route**: `/Home/ShopProfile?id={id}` with full store hero banner, assurance badges, live products grid, and verified customer feedback.
   - **Store Discovery Grid**: `/Home/Index` shop cards displaying rating, products, orders, location, and `[View Shop]`.
   - **Product Details Page**: `/Home/ProductDetails/{id}` seller profile box showing full store metrics and `[View Shop]` link.
   - **Shop Catalog Page**: `/Home/ShopCatalog?shopId={id}` header card with full profile summary.

### Workflow 16: 31. Admin Dashboard (Customers, Sellers, Products, Orders, Today's Sales, Pending Metrics)
Unified enterprise command center for platform administrators:
1. **Core Platform KPIs (Live & Scaled Telemetry)**:
   - 👥 **Customers**: `10,500` (Registered consumers across all delivery zones)
   - 🏪 **Sellers**: `250` (Verified & active merchant store partners)
   - 📦 **Products**: `15,000` (Catalog inventory items available)
   - 📋 **Orders**: `45,000` (Cumulative orders processed by platform)
2. **Daily GMV Highlight**:
   - 💰 **Today's Sales**: `₹4,50,000` (Real-time platform sales volume with +18.4% daily growth indicator)
3. **Pending Attention Backlog**:
   - 📝 **Pending Seller Requests**: `10` (New merchant KYC onboarding applications)
   - ↩️ **Pending Returns**: `8` (Customer return pickup and refund audits)
   - ⚠️ **Pending Complaints**: `5` (Support tickets and escalation desk)
### Workflow 17: 32. Admin Sidebar Dashboard (The Complete 17-Section Command Hierarchy)
Comprehensive administrative control hierarchy with modular responsive sidebar panels:
1. **Sidebar Navigation Structure & Modules**:
   - **Users & Merchants**:
     - 👥 **Customers**: Directory of 10,500 consumers, contact details, total orders & lifetime value.
     - 🏪 **Sellers**: 250 active partners & merchant onboarding management.
   - **Catalog & Inventory**:
     - 📦 **Products**: Global catalog of 15,000 items with SKU & stock tracking.
     - 🗂️ **Categories**: Department classification (Electronics, Grocery, Dairy, Pharmacy, Produce).
     - 🏷️ **Brands**: Partner brands directory (Samsung, Apple, Amul, Nestle, Sony, Cipla).
   - **Sales & Settlements**:
     - 🛍️ **Orders**: Real-time 45,000 order telemetry and delivery milestone tracker.
     - 💳 **Payments**: Transaction settlement ledger with 5% platform commission audit.
   - **Post-Purchase Operations**:
     - ↩️ **Returns**: 8 pending return requests desk with resolution approval.
     - 💸 **Refunds**: Instant gateway refund audit ledger (UPI, Cards, Wallet).
   - **Marketing & Promotions**:
     - 🎟️ **Coupons**: Active discount codes (`WELCOME50`, `HYPER100`, `FREEDEL`, `MEGA20`).
     - 📢 **Offers**: Flash sales and seasonal campaign banners.
   - **Feedback & Quality Desk**:
     - ⭐ **Reviews**: Verified customer feedback audit (4.8★ aggregate score).
     - 🎧 **Complaints**: 5 open customer support tickets with priority escalation.
   - **Operations & Security**:
     - 📈 **Reports**: Downloadable GMV, Commission, and Rider SLA performance reports.
     - 🔔 **Notifications**: Real-time platform alerts and KYC updates.
     - ⚙️ **Settings**: Platform commission rate (5.0%) and hyperlocal delivery radius (10 KM).
     - 🚪 **Logout**: Secure cookie termination.

### Workflow 18: 33. Customer Management (Name, Email, Mobile, Status, Registration Date & [View], [Block], [Unblock])
Complete customer governance and account moderation panel for administrators:
1. **Customer Record Table Columns**:
   - 👤 **Name**: Customer full name (e.g. `Pooja Sharma`, `Rahul Verma`, `Priya Singh`, `Amit Kumar`).
   - ✉️ **Email**: Registered email address (e.g. `pooja@example.com`).
   - 📱 **Mobile**: Primary contact phone number (e.g. `+91 98765 43210`).
   - 🟢 **Status**: Live account standing badge:
     - `Active`: (Green badge) Normal purchasing and checkout privileges.
     - `Blocked`: (Red badge with reason) Account restricted from placing new orders.
   - 📅 **Registration Date**: Formatted date of onboarding (e.g. `12 Jan 2026`).
2. **Interactive Action Controls**:
   - 👁️ **`[View]`**: Opens an instant customer profile modal with contact details, delivery address, lifetime orders count, total spend (₹), and account standing.
   - 🚫 **`[Block]`**: Opens a moderation modal with reason selection (e.g. fraudulent activity, policy violation) and updates status to `Blocked` in real-time.
   - 🔓 **`[Unblock]`**: One-click reactivation restoring the customer's account to `Active` with instant live badge refresh and alert notification.
3. **Backend & Controller Integration**:
   - `GET /Admin/GetCustomerDetails?customerId={id}`: Returns complete customer metadata JSON.
   - `POST /Admin/BlockCustomer`: Sets customer `Status = "Blocked"` and persists optional restriction reason.
   - `POST /Admin/UnblockCustomer`: Restores customer `Status = "Active"`.

### Workflow 19: 34. Seller Management (Shop Name, Owner, Email, Mobile, Status [Approved, Pending, Rejected, Blocked] & [Approve], [Reject], [Block], [Unblock], [View])
Complete merchant onboarding governance, verification, and lifecycle management for administrators:
1. **Seller Directory Table Columns**:
   - 🏪 **Shop Name**: Registered store title & category (e.g. `ABC Electronics`, `Ramesh Kirana`, `Patna Fresh Hypermart`).
   - 👤 **Owner**: Merchant proprietor name with rating score (e.g. `Rajesh Kumar`, `Ramesh Gupta`, `Vikram Singhania`).
   - ✉️ **Email**: Business email address (e.g. `abc.electronics@shopnext.com`).
   - 📱 **Mobile**: Store contact phone number (e.g. `+91 98765 11223`).
   - 🏷️ **Status**: Multi-state badge with status filtering pills (`All`, `Approved`, `Pending`, `Rejected`, `Blocked`):
     - `Approved`: (Green badge) Verified merchant authorized to list products and fulfill customer orders.
     - `Pending`: (Amber badge) New applicant awaiting KYC review and document verification.
     - `Rejected`: (Red badge) Application declined with custom reason.
     - `Blocked`: (Crimson badge) Store restricted from public catalog due to policy violation.
2. **Interactive Admin Action Controls**:
   - 👁️ **`[View]`**: Opens `#sellerDetailsModal` with full store profile, owner details, email, mobile, category, address, catalog product count, lifetime orders, rating, and remarks.
   - ✅ **`[Approve]`**: One-click verification (`POST /Admin/ApproveSeller`) authorizing merchant to sell.
   - ❌ **`[Reject]`**: Opens rejection modal (`POST /Admin/RejectSeller`) prompting for decline reason.
   - 🚫 **`[Block]`**: Opens restriction modal (`POST /Admin/BlockSeller`) restricting the store with reason.
   - 🔓 **`[Unblock]`**: Instantly restores merchant account (`POST /Admin/UnblockSeller`) to `Approved` standing.
3. **Backend Endpoints**:
   - `GET /Admin/GetSellerDetails?shopId={id}`: Returns complete merchant metadata JSON.
   - `POST /Admin/ApproveSeller`: Approves and activates store.
   - `POST /Admin/RejectSeller`: Sets application to rejected state.
   - `POST /Admin/BlockSeller`: Sets store active state to false.
   - `POST /Admin/UnblockSeller`: Restores store active and approved state.

### Workflow 20: 35. Category Management (Electronics, Fashion, Grocery, Mobiles, Computers & [Add], [Edit], [Delete])
Comprehensive taxonomy and product classification console for administrators:
1. **The 5 Core Featured Categories**:
   - 📺 **Electronics**: Smart TVs, Home Audio, Cameras & Appliances (`fa-solid fa-tv`).
   - 👕 **Fashion**: Men, Women, Kids Apparel, Footwear & Accessories (`fa-solid fa-shirt`).
   - 🛒 **Grocery**: Daily Staples, Dal, Rice, Spices, Dairy & Snacks (`fa-solid fa-basket-shopping`).
   - 📱 **Mobiles**: 5G Smartphones, Tablets, Earphones & Covers (`fa-solid fa-mobile-screen-button`).
   - 💻 **Computers**: Laptops, Desktops, Monitors, Keyboards & Gaming (`fa-solid fa-laptop`).
2. **Interactive Action Controls**:
   - ➕ **`[Add]`**: Opens `#addCategoryModal` with category title, quick FontAwesome icon picker, scope description, and status selection (`POST /Admin/AddCategory`).
   - ✏️ **`[Edit]`**: Opens `#editCategoryModal` with pre-filled category metadata to modify title, icon, and active standing (`POST /Admin/EditCategory`).
   - 🗑️ **`[Delete]`**: Opens `#deleteCategoryModal` with warning confirmation before removal from marketplace navigation (`POST /Admin/DeleteCategory`).
3. **Backend Endpoints**:
   - `GET /Admin/GetCategoryDetails?id={id}`: Fetches category JSON object.
   - `POST /Admin/AddCategory`: Inserts new taxonomy classification.
   - `POST /Admin/EditCategory`: Updates category properties.
   - `POST /Admin/DeleteCategory`: Deletes category.

### Workflow 21: 36. Brand Management (Samsung, Apple, Nike, Adidas, HP, Dell, Lenovo & [Add], [Edit], [Delete], [View])
Comprehensive partner brands governance and directory console for platform administrators:
1. **The 7 Core Featured Partner Brands**:
   - 🌟 **Samsung**: Electronics & Mobiles (Smartphones, Smart TVs, Soundbars & Appliances) &bull; Rating: `4.8★`
   - 🍏 **Apple**: Mobiles & Tech (iPhones, MacBooks, iPads, AirPods & Ecosystem) &bull; Rating: `4.9★`
   - 👟 **Nike**: Fashion & Sportswear (Athletic Footwear, Performance Gear & Sneakers) &bull; Rating: `4.8★`
   - ⚽ **Adidas**: Fashion & Streetwear (Three-Stripes Performance Shoes, Apparel & Accessories) &bull; Rating: `4.7★`
   - 💻 **HP**: Computers (Laptops, Gaming Desktops, Monitors & Printers) &bull; Rating: `4.6★`
   - 🖥️ **Dell**: Computers (Workstations, XPS Ultrabooks, Enterprise Hardware) &bull; Rating: `4.7★`
   - 📱 **Lenovo**: Computers & Tablets (ThinkPad Business Laptops, Yoga Convertibles & Legion) &bull; Rating: `4.6★`
2. **Interactive Admin Action Controls**:
   - ➕ **`[Add Brand]`**: Opens `#addBrandModal` with brand title, category selector, logo image URL with real-time preview, description, initial rating, and active standing (`POST /Admin/AddBrand`).
   - 👁️ **`[View]`**: Opens `#brandDetailsModal` with complete brand profile, high-res logo, category badge, product count, rating score, and onboarding timestamp (`GET /Admin/GetBrandDetails`).
   - ✏️ **`[Edit]`**: Opens `#editBrandModal` with pre-filled brand metadata to modify name, category, logo URL, rating, description, and status (`POST /Admin/EditBrand`).
   - 🗑️ **`[Delete]`**: Opens `#deleteBrandModal` with confirmation prompt before unlinking from directory (`POST /Admin/DeleteBrand`).
3. **Backend Endpoints**:
   - `GET /Admin/GetBrandDetails?id={id}`: Fetches brand JSON metadata.
   - `POST /Admin/AddBrand`: Inserts new brand record into SQL database.
   - `POST /Admin/EditBrand`: Updates brand attributes and status.
   - `POST /Admin/DeleteBrand`: Removes brand from partner directory.

### Workflow 22: 37. Product Approval (Seller -> Product Added -> Admin Review -> Approved -> Customer ko Visible)
Comprehensive multi-vendor product moderation and quality assurance pipeline:
1. **Multi-Vendor Marketplace Lifecycle**:
   - **Seller Adds Product**: Merchant submits a new item with specifications, pricing, multiple images (Point 21), and variant matrix (Point 22). Newly created products default to `IsApproved = false` and `ApprovalStatus = "Pending"`.
   - **Product Added**: Saved to database but strictly segregated from customer-facing catalog endpoints.
   - **Admin Review Queue**: Listed in Admin Dashboard (`#panel-products`) with live counter pill, real-time status filter (`All`, `Pending Review`, `Approved`, `Rejected`), and inspection modal (`#productDetailsApprovalModal`).
   - **Admin Approval / Quality Audit**:
     - ✅ **`[Approve]`**: Sets `IsApproved = true`, `ApprovalStatus = "Approved"`, sets `ApprovedDate`, notifies seller, and makes product immediately live.
     - ❌ **`[Reject]`**: Opens `#rejectProductModal` with quick reason templates (Blurry Images, Incomplete Specs, Brand Risk, Misleading Price) and sets `ApprovalStatus = "Rejected"`, storing `RejectionReason` and alerting seller.
   - **Customer ko Visible**: Customer catalog (`GetAllProductsAsync`, `SearchProductsAsync`, `/Api/products`, `/Home/Index`, `/Shop/Index`, `/Product/Details`) strictly enforces `p.IsApproved == true`, ensuring unapproved or rejected seller products are never shown to buyers.
2. **Interactive Action Controls**:
   - 👁️ **`[View]`**: Opens `#productDetailsApprovalModal` with specifications, pricing vs MRP, merchant credentials, multi-angle photos tray, and variants breakdown (`GET /Admin/GetProductApprovalDetails?id={id}`).
   - ✅ **`[Approve]`**: One-click approval granting customer catalog visibility (`POST /Admin/ApproveProduct`).
   - ❌ **`[Reject]`**: Modal-driven rejection with required explanation logged into product audit trail (`POST /Admin/RejectProduct`).
3. **Backend Endpoints**:
   - `GET /Admin/GetProductApprovalDetails?id={id}`: Retrieves complete product specifications, images, variants, and merchant info.
   - `POST /Admin/ApproveProduct`: Sets `IsApproved = true`, `ApprovalStatus = "Approved"`.
   - `POST /Admin/RejectProduct`: Sets `IsApproved = false`, `ApprovalStatus = "Rejected"`, and records `RejectionReason`.

### Workflow 23: 38. Admin Order Management (All Orders, Filters: Today / Yesterday / This Month / Status / Seller / Customer, Modal Telemetry & Status Progression)
Enterprise multi-vendor order oversight and fulfillment command:
1. **Order Columns & Data Points**:
   - **Order ID**: Primary identifier formatted as `#ORD-{ID}` with live creation timestamp.
   - **Customer**: Buyer name, email address, contact phone number, and physical delivery address.
   - **Seller / Merchant**: Store name, proprietor name, merchant contact number, and fulfillment origin.
   - **Amount & Breakdown**: Subtotal, 5% platform fee (`₹{Amount * 0.05}`), free delivery indicator, and gross total.
   - **Payment**: Payment gateway mode (UPI, COD, NetBanking, Card) and transaction settlement status (`Paid`, `Pending`, `Refunded`).
   - **Status**: Lifecycle state (`Pending`, `Accepted`, `Preparing`, `OutForDelivery`, `Delivered`, `Cancelled`, `Returned`).
   - **Assigned Rider**: Delivery partner name and assigned rider contact.
   - **Date**: Formatted order placement timestamp with date grouping category (`Today`, `Yesterday`, `ThisMonth`, `Older`).
2. **Interactive Filters**:
   - 📅 **Date Filter Tabs**: Instant single-click switching between `All Time`, `Today`, `Yesterday`, and `This Month`.
   - 🔄 **Status Dropdown**: Filter by fulfillment stage (`All Statuses`, `Pending`, `Accepted`, `Preparing`, `OutForDelivery`, `Delivered`, `Cancelled`).
   - 🏪 **Seller Dropdown**: Filter orders by specific registered merchant store.
   - 🔍 **Customer & Order Search**: Real-time debounce search matching Customer Name, Phone, Email, or `#ORD-` reference.
3. **Interactive Actions & Modals**:
   - 👁️ **`[View]`**: Opens `#adminOrderDetailsModal` showing full order telemetry, customer card, seller card, ordered line items table with thumbnail, unit price, quantity, and total price, fee calculations, and return/cancellation remarks.
   - ✏️ **`[Update Status]`**: Opens `#adminUpdateOrderStatusModal` allowing administrators to advance order fulfillment status (`Pending` -> `Accepted` -> `Preparing` -> `OutForDelivery` -> `Delivered` -> `Cancelled`) with audit reason logging (`POST /Admin/UpdateOrderStatus`).
4. **Backend Endpoints**:
   - `GET /Admin/GetOrderDetails?id={id}`: Retrieves complete order details, line items, customer, seller, and rider telemetry.
   - `POST /Admin/UpdateOrderStatus`: Updates order status in SQL Server and records audit trail.

### Workflow 24: 39. Payment Management (Payment ID, Order ID, Customer, Amount, Payment Method, Transaction ID, Status: Pending / Success / Failed / Refunded, Date & Audit)
Centralized platform financial transactions, payment gateway monitoring, and settlement audits:
1. **Payment Columns & Attributes**:
   - **Payment ID**: Formatted transaction reference `PAY-{ID:D5}`.
   - **Order ID**: Linked order reference `#ORD-{ID}`.
   - **Customer**: Buyer identity and contact number.
   - **Amount**: Total transaction value in INR.
   - **Payment Method**: Gateway rails (`Razorpay UPI`, `COD Cash on Delivery`, `HDFC Credit Card`, `ICICI NetBanking`, `Paytm Wallet`).
   - **Transaction ID**: Unique bank/gateway reference (`TXN-260905-{ID}`).
   - **Status**: Payment reconciliation status (`Pending`, `Success`, `Failed`, `Refunded`).
   - **Date**: Settlement timestamp (`dd MMM yyyy, hh:mm tt`).
   - **Platform Commission (5%)**: Automatic platform commission fee calculation.
   - **Net Merchant Payout**: Payout settled to seller bank account (`Total - 5%`).
2. **Interactive Filter & Search**:
   - 🎯 **Status Pills**: Instant filtering by `All Transactions`, `Success / Paid`, `Pending`, `Failed`, and `Refunded`.
   - 🔍 **Transaction Search**: Real-time filtering by Customer Name, Transaction ID, Payment ID, or Order number.
3. **Interactive Actions & Modals**:
   - 👁️ **`[Audit / View]`**: Opens `#adminPaymentDetailsModal` displaying financial breakdown, gateway reference logs, 5% platform cut, and merchant net disbursement (`GET /Admin/GetPaymentDetails?id={id}`).
   - 🔄 **`[Process Refund]`**: Backend endpoint (`POST /Admin/ProcessRefund`) to reverse payments for cancelled/returned orders.
4. **Backend Endpoints**:
   - `GET /Admin/GetPaymentDetails?id={id}`: Fetches transaction audit logs, commission cut, and net merchant payout.
   - `POST /Admin/ProcessRefund`: Reconciles order refund back to customer payment source.

### Workflow 25: 40. Coupon Management (Coupon Code, Discount, Min Order, Max Discount, Start/End Date, Usage Limit, Status & Example: WELCOME100, ₹100 OFF, Min Order ₹999)
Comprehensive promotional campaigns engine and coupon code governance:
1. **Coupon Columns & Data Specification**:
   - **Coupon Code**: Unique coupon alphanumeric code (e.g. `WELCOME100`, `MEGA20`, `FREEDEL`, `FESTIVE500`).
   - **Discount**: Discount representation (`₹100 OFF`, `20% OFF`, `Free Delivery`).
   - **Discount Type**: `Flat` (₹ amount deduction), `Percentage` (% rate with cap), `FreeDelivery` (zero shipping).
   - **Minimum Order**: Threshold cart subtotal required to unlock discount (e.g. `₹999`).
   - **Maximum Discount**: Upper limit cap for percentage-based coupons (e.g. `₹100`, `₹500`).
   - **Validity Dates**: `Start Date` to `End Date` campaign validity window.
   - **Usage Limit & Redemptions**: Max redemption cap (e.g. `500`) vs actual redemptions counter (`Used: 42`).
   - **Status**: Dynamic lifecycle state (`Active`, `Inactive`, `Expired`).
2. **Featured Seed Campaigns**:
   - 🏷️ **`WELCOME100`**: `₹100 OFF` on Minimum Order `₹999` (Max Discount: `₹100`, Limit: `500`).
   - 🏷️ **`MEGA20`**: `20% OFF` on Minimum Order `₹1,499` (Max Discount: `₹300`, Limit: `1,000`).
   - 🏷️ **`FREEDEL`**: `Free Delivery` on Minimum Order `₹499` (Limit: `2,000`).
   - 🏷️ **`FESTIVE500`**: `₹500 OFF` on Minimum Order `₹2,999` (Max Discount: `₹500`, Limit: `250`).
3. **Interactive Action Controls & Modals**:
   - ➕ **`[Add Coupon]`**: Opens `#addCouponModal` with Code, Discount Type, Value, Min Order, Max Cap, Start/End Dates, Usage Limit, and Description (`POST /Admin/AddCoupon`).
   - ✏️ **`[Edit]`**: Opens `#editCouponModal` with pre-filled campaign settings to modify discount, minimums, validity period, and status (`POST /Admin/EditCoupon`).
   - ⚡ **`[Toggle Status]`**: Single-click activation/deactivation toggling coupon between `Active` and `Inactive` (`POST /Admin/ToggleCouponStatus`).
   - 🗑️ **`[Delete]`**: Opens `#deleteCouponModal` with confirmation modal before removing promo code (`POST /Admin/DeleteCoupon`).
4. **Backend Endpoints**:
   - `GET /Admin/GetCouponDetails?id={id}`: Retrieves coupon specifications for viewing and editing.
   - `POST /Admin/AddCoupon`: Creates new promo coupon campaign in SQL Server database.
   - `POST /Admin/EditCoupon`: Modifies coupon terms, limits, dates, and status.
   - `POST /Admin/ToggleCouponStatus`: Toggles coupon active status.
   - `POST /Admin/DeleteCoupon`: Soft-deletes / deletes coupon from system.

### Workflow 26: 41. Offers Management & Today's Deals (Product, MRP, Selling Price, Discount, Start Date, End Date & Home Page 🔥 Today's Deals)
Flash deals, daily discounts, and promotional offers system:
1. **Offer Columns & Data Specification**:
   - **Product**: Target product link with image, title, category, and seller attribution.
   - **MRP**: Maximum Retail Price (original list price).
   - **Selling Price**: Special discounted promotional deal price.
   - **Discount**: Calculated percentage discount (`% OFF` = `((MRP - SellingPrice) / MRP) * 100`).
   - **Start Date**: Deal activation timestamp.
   - **End Date**: Deal expiry deadline with real-time countdown tracking.
   - **Status**: Dynamic lifecycle state (`Active`, `Inactive`, `Expired`).
2. **Interactive Action Controls & Modals**:
   - ➕ **`[Add Offer]`**: Opens `#addOfferModal` with product dropdown (auto-fills MRP & Selling Price), auto-calculated discount %, start/end datetime picker, and custom promotional tagline (`POST /Admin/AddOffer`).
   - ✏️ **`[Edit]`**: Opens `#editOfferModal` with pre-filled deal settings to modify prices, discount %, campaign window, and active status (`POST /Admin/EditOffer`).
   - ⚡ **`[Toggle Status]`**: Single-click activation/deactivation toggle (`POST /Admin/ToggleOfferStatus`).
   - 🗑️ **`[Delete]`**: Opens `#deleteOfferModal` with confirmation modal before removing deal (`POST /Admin/DeleteOffer`).
3. **Customer Home Page Integration (🔥 Today's Deals)**:
   - Dynamic banner and grid rendered on `Views/Home/Index.cshtml` driven by `ViewBag.TodaysDeals`.
   - Real-time countdown timer displaying remaining hours, minutes, and seconds (`HH:MM:SS`).
   - Discount ribbon (e.g. `22% OFF`), crossed-out original MRP, bold deal selling price, and instant "Add to Cart" button.
4. **Backend Endpoints**:
   - `GET /Admin/GetOfferDetails?id={id}`: Retrieves offer parameters for editing.
   - `POST /Admin/AddOffer`: Creates new promotional offer in database.
   - `POST /Admin/EditOffer`: Updates offer prices, dates, and status.
   - `POST /Admin/ToggleOfferStatus`: Toggles offer deal active state.
   - `POST /Admin/DeleteOffer`: Removes offer deal from system.

### Workflow 27: 42. Review Management (Inappropriate Review Moderation: Product, Customer, Rating, Review, Date, [Hide], [Delete])
Admin moderation desk for managing customer product reviews and purging inappropriate or abusive feedback:
1. **Review Moderation Columns & Data Specification**:
   - **Product**: Product name and thumbnail.
   - **Customer**: Customer reviewer name and mobile contact.
   - **Rating**: Star rating score (`⭐ 1` to `⭐ 5`).
   - **Review**: Customer's full feedback commentary.
   - **Date**: Formatted submission timestamp.
   - **Status**: Audit state (`Visible` [Active], `Hidden` [Inappropriate with Reason]).
2. **Interactive Moderation Action Controls**:
   - 👁️‍🗨️ **`[Hide]`**: Opens `#hideReviewModal` prompting the admin to select or type a moderation reason (e.g. *"Violates Community Guidelines / Inappropriate Language"*, *"Spam / Promotional Content"*, *"Abusive / Harassing Speech"*, *"Fake / Misleading Review"*). Hiding sets `IsHidden = true` and `IsActive = false` (`POST /Admin/HideReview`), keeping the audit log while removing the review from customer visibility.
   - 👁️ **`[Unhide]`**: Single-click restoration restoring review visibility across catalog (`POST /Admin/UnhideReview`).
   - 🗑️ **`[Delete]`**: Opens `#deleteReviewModal` for permanent deletion of abusive reviews (`POST /Admin/DeleteReview`).
3. **Filter Pills & Real-Time Counters**:
   - Filter pills: `All Reviews`, `Visible Reviews`, and `Hidden (Flagged) Reviews`.
   - Dynamic badge counters synchronizing counts as reviews are moderated.
4. **Impact on Customer Catalog**:
   - `GetReviewsByProductIdAsync` and `GetReviewsByShopIdAsync` strictly filter by `!r.IsHidden && r.IsActive`, ensuring hidden or deleted reviews are never visible to shoppers on `ProductDetails` or shop profiles.
5. **Backend Endpoints**:
   - `GET /Admin/GetReviewDetails?id={id}`: Fetches review data for audit.
   - `POST /Admin/HideReview`: Flags review as hidden with moderation reason.
   - `POST /Admin/UnhideReview`: Restores review visibility.
   - `POST /Admin/DeleteReview`: Permanently deletes review.

### Workflow 28: 43. Complaint / Support (Customer: My Orders -> Need Help -> Order ID, Issue, Description, Attachment & Admin: Open, In Progress, Resolved, Closed)
Full-lifecycle customer support ticketing and escalation management system:
1. **Customer Workflow & Data Specification**:
   - **Entry Point**: Customer navigates to `My Orders` (`/Customer/MyOrders`).
   - **Action**: Every order card features a prominent **`[Need Help]`** button.
   - **Complaint Submission Form (`#needHelpModal`)**:
     - **Order ID**: Automatically pre-filled and displayed with a badge `#ORD{ID}`.
     - **Issue**: Categorized dropdown (`Item Damaged / Defective`, `Wrong Item Delivered`, `Missing Item in Package`, `Delivery Delayed / Rider Issue`, `Payment / Refund Status`, `Product Quality / Not as Described`, `Other`).
     - **Description**: Detailed multi-line explanation of the issue (required).
     - **Attachment**: Optional image URL or photo proof link (e.g. damaged unboxing photo, package label).
   - **Ticket Generation**: Generates unique ticket reference (e.g. `TKT-260905-001`) with initial status `Open`.
   - **Customer Ticket Status**: Instant confirmation modal (`#complaintConfirmationModal`) and order tracking.
2. **Admin Support Helpdesk (`#panel-complaints`)**:
   - **Columns**: `Ticket #`, `Order ID`, `Customer (Name, Phone, Email)`, `Issue / Category`, `Description`, `Attachment (Proof)`, `Priority (High/Medium/Low/Urgent)`, `Status (Open, In Progress, Resolved, Closed)`, `Date`, `Actions`.
   - **Status Filtering**: Filter pills for `All Tickets`, `Open` (Warning), `In Progress` (Info), `Resolved` (Success), `Closed` (Secondary).
   - **Search Filter**: Real-time debounce search matching Ticket #, Order ID, Customer Name, Phone, or Issue.
   - **Interactive Modals**:
     - 👁️ **`[View]`** (`#adminComplaintDetailsModal`): Complete ticket audit displaying customer card, order telemetry, issue commentary, full-size attachment preview, and resolution history.
     - ✏️ **`[Update Status]`** (`#adminUpdateComplaintStatusModal`): Transition ticket between `Open`, `In Progress`, `Resolved`, and `Closed`, with audit resolution remarks.
3. **Backend Endpoints**:
   - `POST /Customer/RaiseComplaint`: Receives complaint, creates record in SQL Server, and triggers notification to Admin.
   - `GET /Customer/GetOrderComplaints?orderId={id}`: Retrieves customer ticket history for an order.
   - `GET /Admin/GetComplaintDetails?id={id}`: Retrieves full ticket audit data for admin review.
   - `POST /Admin/UpdateComplaintStatus`: Updates ticket status and logs resolution notes.
   - `POST /Admin/DeleteComplaint`: Deletes complaint record.

### Workflow 29: 44. Platform Notifications (Customer, Seller, Admin Notification Events & Triggers)
Real-time tri-party notification engine delivering targeted operational alerts:
1. **Customer Notifications**:
   - 📦 **Order Confirmed**: Triggered when merchant confirms/accepts the order (`Type = Order`).
   - 🚚 **Order Shipped**: Triggered when rider picks up or order is out for delivery (`Type = Delivery`).
   - ✅ **Order Delivered**: Triggered upon successful handover to customer (`Type = Delivery`).
   - 💰 **Refund Processed**: Triggered when return is approved or refund credited (`Type = Payment`).
2. **Seller / Vendor Notifications**:
   - 🛍️ **New Order**: Triggered when customer places an order for the seller's store (`Type = Order`).
   - 🔄 **Return Request**: Triggered when buyer submits a return request on delivered items (`Type = Return`).
   - 🟢 **Product Approved**: Triggered when admin approves merchant's catalog listing (`Type = Product`).
   - 🔴 **Product Rejected**: Triggered when admin rejects catalog listing with feedback (`Type = Product`).
3. **Admin Notifications**:
   - 🏪 **New Seller**: Triggered when a new merchant completes seller onboarding registration (`Type = Seller`).
   - 🎧 **New Complaint**: Triggered when a customer logs a support ticket via *Need Help* (`Type = Complaint`).
### Workflow 31: 46. Security & Role-Based Authorization (Admin, Seller, Customer, Rider Roles, Strict Access Boundaries & 403 Forbidden Shield)
Enterprise-grade role-based access control (RBAC) and identity isolation for the multi-vendor ecosystem:
1. **Core Roles & Access Boundaries**:
   - 🛡️ **`Admin` Role**:
     - Unrestricted governance access across `/Admin/*` (Dashboard, Product Approvals, Seller Approvals, Categories, Brands, Orders, Payments, Coupons, Offers, Reviews Moderation, Support Helpdesk, Reports, Settings).
     - Full operational visibility and cross-role audit powers.
   - 🏪 **`Seller` Role**:
     - Strictly restricted to merchant catalog, store settings, and vendor order fulfillment (`/Vendor/*`).
     - **Strict Security Barrier**: Cannot access `/Admin/*` (redirected to `/Account/AccessDenied` or `/Admin/Login`).
     - **Strict Privacy Barrier**: Cannot access other merchants' catalogs or customer personal accounts (`/Customer/*`).
   - 👤 **`Customer` Role**:
     - Restricted to personal shopping, checkout, private profile, order history, address book, wishlist, and help tickets (`/Customer/*`).
     - **Strict Security Barrier**: Cannot access `/Admin/*` or `/Vendor/*` management consoles.
   - 🛵 **`Rider` Role**:
     - Restricted to rider GPS telemetry, availability toggle, and assigned delivery runs (`/Rider/*`).
2. **Authentication & Authorization Pipeline (`Program.cs`)**:
   - `builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)` with sliding expiration, HTTP-only cookies, and encrypted ClaimsPrincipal.
   - `builder.Services.AddAuthorization()` configuring declarative policies (`AdminOnly`, `SellerOnly`, `CustomerOnly`, `RiderOnly`).
   - Declarative Controller attributes: `[Authorize(Roles = "Admin")]`, `[Authorize(Roles = "Seller,Admin")]`, `[Authorize(Roles = "Customer,Admin")]`, `[Authorize(Roles = "Rider,Admin")]`.
3. **Unified Security Hub & 403 Access Denied UI (`/Account/AccessDenied`)**:
   - Renders glassmorphic 403 Security Screen explaining identity roles and providing immediate action buttons to Customer Portal, Seller Portal, and Admin Console.
4. **Backend Endpoints**:
   - `GET /Account/AccessDenied`: Renders 403 security warning.
   - `GET /Account/Login`: Role-aware login router redirecting to appropriate portal.
   - `GET /Account/Logout`: Universal sign-out clearing claims and cookies.

---

## 10. 47. Complete 26-Step Master Development Roadmap & Status Matrix

| Step # | Development Module | Status | Core Implemented Features & Endpoints |
|---|---|---|---|
| **1** | 👤 **Customer Dashboard** | ✅ Complete | Profile, addresses, password update, customer stats (`/Customer/Account`) |
| **2** | ❤️ **Wishlist** | ✅ Complete | Add/remove items, wishlist view, move to cart (`/Customer/Wishlist`) |
| **3** | 👁️ **Recently Viewed** | ✅ Complete | Dynamic tracking, recently viewed carousel on catalog & detail pages |
| **4** | 🛒 **Improved Cart** | ✅ Complete | Slide-out drawer, quantity counters, coupon code discounts, bill breakdown |
| **5** | 💳 **Checkout** | ✅ Complete | Multi-step checkout, saved addresses, delivery slots, UPI/Card/COD options |
| **6** | 📦 **Order Management** | ✅ Complete | Order tracking, timeline tracker, OTP delivery handover, receipt download |
| **7** | 🔄 **Return/Refund** | ✅ Complete | Order cancellation, item return requests, return reasons, refund tracking |
| **8** | 🔔 **Notifications** | ✅ Complete | Point 44: Cross-role targeted alerts (Customer, Seller, Admin) |
| **9** | 📝 **Seller Registration** | ✅ Complete | Merchant onboarding form, bank details, shop category, city (`/Vendor/Register`) |
| **10** | ⏳ **Seller Approval** | ✅ Complete | Admin review desk, approval/rejection workflows, pending approval notices |
| **11** | 📊 **Seller Dashboard** | ✅ Complete | Merchant sales KPIs, recent store orders, product catalog status (`/Vendor/Dashboard`) |
| **12** | 🛍️ **Seller Product Management** | ✅ Complete | Add product modal, SKU, category, brand, pricing, image uploads (`/Vendor/Products`) |
| **13** | 📦 **Seller Inventory** | ✅ Complete | Stock quantity management, in-stock, low-stock, out-of-stock indicators |
| **14** | 🚚 **Seller Order Management** | ✅ Complete | 4-stage order lifecycle: Accept, Prepare, Mark Out for Delivery, Deliver |
| **15** | 💰 **Seller Earnings** | ✅ Complete | Gross sales calculation, 10% marketplace commission deduction, net payout ledger |
| **16** | 🛡️ **Admin Dashboard** | ✅ Complete | High-level platform KPIs, pending queues, dynamic navigation (`/Admin/Dashboard`) |
| **17** | 👥 **Customer Management** | ✅ Complete | Customer directory, LTV spend, order history, active/blocked states (`#panel-customers`) |
| **18** | 🏪 **Seller Management** | ✅ Complete | Merchant directory, KYC inspection, approve, reject, block/unblock (`#panel-sellers`) |
| **19** | 🏷️ **Product/Category Management** | ✅ Complete | Point 35: Categories, Point 36: Brands, Point 37: Product review & approval |
| **20** | 💳 **Payment Management** | ✅ Complete | Point 39: Transaction ledger, UPI/Gateway references, payment status audit |
| **21** | 🎟️ **Coupon/Offers** | ✅ Complete | Point 40: Coupon codes with min order, Point 41: Deal of the Day & Offers |
| **22** | ↩️ **Returns/Refunds** | ✅ Complete | Admin return requests desk, refund processing and gateway reconciliation |
| **23** | 🎧 **Complaints** | ✅ Complete | Point 43: Customer Need Help tickets, Admin support desk (`Open`, `In Progress`, `Resolved`, `Closed`) |
| **24** | 📈 **Reports** | ✅ Complete | Point 45: 9 specialized reports (Sales, Order, Customer, Seller, Product, Payment, Return, Refund, Commission) with Date Filters & CSV Export |
| **25** | 🔒 **Security + Validation** | ✅ Complete | Point 46: ASP.NET Core Role Authorization (`Admin`, `Seller`, `Customer`, `Rider`), Claims Identity, 403 Forbidden Shield |
| **26** | 🎨 **Final UI/Responsive Design** | ✅ Complete | Mobile-first responsiveness, glassmorphic dark theme, micro-animations |

---

## 9. OOP Concepts, SOLID Principles & Clean Architecture Implementation

### A. Object-Oriented Programming (OOP) Principles Applied
1. **Encapsulation**:
   - Entity models (`Product`, `Order`, `Shop`, `ProductVariant`, `ProductImage`, `Complaint`, `Offer`, etc.) encapsulate domain properties and validation data annotations (`[Required]`, `[StringLength]`).
   - Business services (`ShopNextService`) encapsulate data-access logic and EF Core context interactions, exposing clean asynchronous methods without leaking internal query details.
2. **Inheritance**:
   - `BaseModel.cs`: Abstract base class providing common audit fields (`CreatedDate`, `CreatedById`, `UpdatedDate`, `UpdatedById`, `IsActive`, `IsDeleted`, `Remark`) inherited by domain entities (`Product`, `Shop`, `Rider`, `Complaint`, etc.).
   - MVC Controllers inherit from `Microsoft.AspNetCore.Mvc.Controller`.
3. **Polymorphism**:
   - Interface polymorphism enables dependency injection of `ICustomerService`, `IProductService`, `IOrderService`, `IVendorService`, `IRiderService`, `IAdminReportService`, and `IShopNextService`.
   - Concrete service implementations can be substituted or mocked for automated testing without altering controller code.
4. **Abstraction**:
   - High-level controllers interact with abstract contracts (`IShopNextService`, `IAdminReportService`, etc.) rather than concrete database contexts or raw SQL connections.

### B. SOLID Principles Applied
1. **Single Responsibility Principle (SRP)**:
   - **Controllers**: Responsible exclusively for handling HTTP requests, input validation, and view/JSON response generation.
   - **Services**: Responsible strictly for business logic, status progression transitions, and persistence queries.
   - **Models**: Responsible strictly for representing state and domain contracts.
2. **Open/Closed Principle (OCP)**:
   - Base models and interfaces are open for extension (e.g. adding new delivery metrics or variant types) without modifying existing stable service contracts or database schemas.
3. **Liskov Substitution Principle (LSP)**:
   - Subtypes and implementations honor base contracts completely. Any class implementing `IOrderService` or `IAdminReportService` can seamlessly execute operations without unexpected side-effects.
4. **Interface Segregation Principle (ISP)**:
   - The service interface is decomposed into focused domain-specific interfaces:
     - `ICustomerService`: Identity, customer profile, addresses, wishlist, notifications, coupons.
     - `IProductService`: Catalog, variants, multi-images, search, category navigation, inventory, reviews, offers.
     - `IOrderService`: Order lifecycle, line items, 4-stage transitions, cancellations, returns.
     - `IVendorService`: Merchant profile, store catalog management, vendor orders.
     - `IRiderService`: Delivery partner onboarding, GPS telemetry, assignment.
     - `IAdminReportService`: Platform-wide 9-module financial and operational analytics with dynamic date filtering and CSV export.
   - `IShopNextService` inherits all six interfaces to maintain 100% backward compatibility for existing controllers while allowing future components to depend only on the specific interface they require.
5. **Dependency Inversion Principle (DIP)**:
   - Controllers depend on abstractions (`IShopNextService`, `IAdminReportService`, etc.) injected via ASP.NET Core's built-in IoC container (`Program.cs`), decoupling the presentation layer completely from EF Core or database technology details.

---

## 10. Master 26-Step Development Sequence & Integration Matrix

| Step | Module / Requirement | Core Architecture & Controller | View / UI Endpoint | Database & SP Integration |
|---|---|---|---|---|
| **1** | **Customer Dashboard** | `CustomerController.Account` | `Views/Customer/Account.cshtml` | Profile, addresses, active orders, saved items |
| **2** | **Wishlist** | `CustomerController.ToggleWishlist`, `CustomerController.GetWishlist` | `Views/Customer/Account.cshtml#wishlist` | `Wishlists` table with 1-click add to cart |
| **3** | **Recently Viewed** | `ShopNextRecentlyViewed` (site.js) | `Views/Home/Index.cshtml`, `ProductDetails.cshtml` | Client-side persistent cache + auto-sync |
| **4** | **Improved Cart** | LocalStorage + `Views/Home/Cart.cshtml` | `Views/Home/Cart.cshtml` | Multi-item checkout, single-shop conflict alert |
| **5** | **Checkout & Address** | `CustomerController.Checkout`, `PlaceOrder` | `Views/Customer/Checkout.cshtml` | Multi-address picker, COD / Online switch |
| **6** | **Order Management** | `CustomerController.MyOrders`, `OrderDetails` | `Views/Customer/MyOrders.cshtml` | 4-Stage visual timeline & live status |
| **7** | **Return / Refund** | `CustomerController.RequestReturn` | `Views/Customer/OrderDetails.cshtml` | `OrderReturns` table with refund calculation |
| **8** | **Notifications** | `CustomerController.GetNotifications` | Notification popover in top navigation | `Notifications` table with role targeting |
| **9** | **Seller Registration** | `VendorController.Register` | `Views/Vendor/Register.cshtml` | Auto-geocoding, license & GST verification |
| **10** | **Seller Approval** | `AdminController.ApproveShop`, `RejectShop` | `Views/Admin/Dashboard.cshtml#panel-shops` | Real-time status update in `Shops` table |
| **11** | **Seller Dashboard** | `VendorController.Dashboard` | `Views/Vendor/Dashboard.cshtml` | Revenue, order counters, store performance |
| **12** | **Seller Product Mgmt** | `VendorController.Products`, `SaveProduct` | `Views/Vendor/Products.cshtml` | Variants, multi-images, discount auto-calculation |
| **13** | **Seller Inventory** | `VendorController.Inventory`, `UpdateStock` | `Views/Vendor/Inventory.cshtml` | Low-stock and out-of-stock indicators |
| **14** | **Seller Order Mgmt** | `VendorController.Orders`, `UpdateOrderStatus` | `Views/Vendor/Orders.cshtml` | Placed -> Confirmed -> Packed -> Shipped workflow |
| **15** | **Seller Earnings** | `VendorController.Earnings` | `Views/Vendor/Earnings.cshtml` | Payout history, commission deduction calculations |
| **16** | **Admin Dashboard** | `AdminController.Dashboard` | `Views/Admin/Dashboard.cshtml` | Master KPIs, store approvals, system monitoring |
| **17** | **Customer Management** | `AdminController.Customers`, `ToggleCustomer` | `Views/Admin/Dashboard.cshtml#panel-customers` | Account status toggles, order history preview |
| **18** | **Seller Management** | `AdminController.Shops`, `ToggleShopStatus` | `Views/Admin/Dashboard.cshtml#panel-shops` | Store blocking, commission rate overrides |
| **19** | **Product & Category** | `AdminController.Products`, `Categories` | `Views/Admin/Dashboard.cshtml#panel-products` | Platform-wide catalog curation |
| **20** | **Payment Management** | `AdminController.Payments` | `Views/Admin/Dashboard.cshtml#panel-payments` | COD & Online transaction ledger |
| **21** | **Coupon & Offers** | `AdminController.Coupons`, `Offers` | `Views/Admin/Dashboard.cshtml#panel-coupons` | Percentage and flat discount campaign manager |
| **22** | **Admin Returns/Refunds**| `AdminController.Returns`, `ProcessRefund` | `Views/Admin/Dashboard.cshtml#panel-returns` | Approve/reject returns, direct refund dispatch |
| **23** | **Reviews Management** | `AdminController.Reviews`, `DeleteReview` | `Views/Admin/Dashboard.cshtml#panel-reviews` | Customer ratings moderation & review visibility |
| **24** | **Complaints & Support**| `AdminController.Complaints`, `UpdateTicket` | `Views/Admin/Dashboard.cshtml#panel-complaints` | Customer support ticketing system |
| **25** | **Reports & Analytics** | `AdminController.GetFilteredReportData`, `ExportReportCsv` | `Views/Admin/Dashboard.cshtml#panel-reports` | 9-Module dynamic analytics + CSV / Print export |
| **26** | **Security & Roles** | ASP.NET Core Claims + `[Authorize(Roles = "...")]` | `/Account/AccessDenied` | 403 Forbidden protection across Admin, Seller, Customer |
| **27** | **UI/UX & Design System**| Master Design Tokens, Mobile Sticky Bottom Nav | `site.css`, `site.js`, `_Layout.cshtml` | Blue + White + Light Gray theme, Empty States, Toasts |

---

## 11. Point 27 — Master UI/UX & Responsive Design Specification

### A. Curated Color Palette
- **Primary Brand Color**: Royal Blue (`#2563eb`, hover `#1d4ed8`, light `#eff6ff`)
- **Background Canvas**: Pure White / Crisp Light Gray (`#f8fafc` / `#ffffff`)
- **Success State**: Emerald Green (`#10b981`, hover `#059669`, light `#ecfdf5`)
- **Warning State**: Amber Orange (`#f59e0b`, hover `#d97706`, light `#fffbeb`)
- **Danger State**: Crimson Red (`#ef4444`, hover `#dc2626`, light `#fef2f2`)
- **Dark Text & High Contrast**: Slate Black (`#0f172a`, secondary `#334155`, muted `#64748b`)

### B. Core UI Components & Micro-Interactions
1. **Responsive Dual Navigation**:
   - **Desktop / Tablet**: Dark slate floating navbar (`.main-navbar`) with role-tailored dropdowns and active state indicators.
   - **Mobile Devices (< 768px)**: Sticky bottom bar (`.mobile-bottom-nav`) with immediate 1-tap access to Home, Shop, Cart (with live badge counter), Orders, and Profile.
2. **Product Cards**:
   - Clean white elevation with subtle border transition.
   - Smooth image zoom on hover (`transform: scale(1.06)`).
   - Stock status badge overlay (`In Stock`, `Low Stock`, `Out of Stock`) and discount pill overlay.
3. **Interactive Buttons**:
   - Hover lift and active micro-scale transitions on all `.btn` elements.
4. **Universal Confirmation Modal (`#globalConfirmationModal`)**:
   - Reusable modal hook `confirmAction({...})` for Delete, Cancel, and critical state modifications with customizable action buttons.
5. **Toast Notification Engine (`#globalToastContainer`)**:
   - Non-intrusive floating toast notifications with slide-in animations for cart actions, wishlist toggles, and server notifications.
6. **Global Loading Spinner (`#globalSpinnerOverlay`)**:
   - High-contrast circular spinner with backdrop blur for smooth asynchronous operations.
7. **Accessible Empty States (`.empty-state-card`)**:
   - High-clarity illustrated empty states for empty cart (*Your Cart is Empty*), no orders (*No Orders Found*), no search results (*No Products Found*), and wishlist.

---

## 12. Points 28 - 32: Enterprise Hardening & Production Operations Guide

### Point 28: Validation & Global Exception Handling
- **Global Error Handling**: Integrated `app.UseStatusCodePagesWithReExecute("/Error/{0}")` and `app.UseExceptionHandler("/Error/500")` via `Controllers/ErrorController.cs`.
- **Custom UI Pages**: High-readability glassmorphic views `Views/Error/404.cshtml` (*Page Not Found*) and `Views/Error/500.cshtml` (*Internal Server Error*) providing immediate return-to-home recovery.
- **Model Validation**: Strict `System.ComponentModel.DataAnnotations` validations (`[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`) with client-side feedback and server-side ModelState verification.

### Point 29: Security Architecture & Protective Layers
- **Cross-Site Request Forgery (CSRF)**: Antiforgery cookie token validation configured in `Program.cs` (`X-CSRF-TOKEN`, `SameSiteMode.Lax`, `HttpOnly`).
- **File Upload Protection (`Helpers/SecurityHelper.cs`)**:
  - Whitelist: `.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`.
  - MIME type verification + 5MB size limit.
  - Sanitized unique filename generator preventing path traversal attacks.
- **Password Security**: Cryptographic salted hashing via `SecurityHelper.HashPassword` & `VerifyPassword`.
- **SQL Injection Prevention**: 100% Parameterized queries via EF Core LINQ and `SqlParameter` collections.

### Point 30: Performance & Database Optimization
- **Entity Framework Core**: Applied `.AsNoTracking()` to read-heavy reporting, catalog browsing, and search queries.
- **In-Memory Caching (`IMemoryCache`)**: Fast cache hits for high-frequency queries (Categories, Deals, Featured Stores).
- **SQL Server Indexes Added**:
  - `IX_Products_Shop_Category` on `Products(ShopId, Category, IsActive, IsDeleted)`
  - `IX_Orders_Customer_Status` on `Orders(CustomerId, OrderStatus, OrderDate)`
  - `IX_Orders_Shop_Status` on `Orders(ShopId, OrderStatus, OrderDate)`
  - `IX_OrderItems_OrderId` on `OrderItems(OrderId)`
  - `IX_AuditLogs_User_Date` on `AuditLogs(UserId, CreatedDate)`

### Point 31: Logging & Audit Trail
- **Audit Table & Model (`dbo.AuditLogs`)**: Tracks **Who** (`UserId`, `UserName`, `UserRole`), **What** (`Action`, `EntityName`, `EntityId`, `Details`), **When** (`CreatedDate`), and `IpAddress`.
- **Audit Service (`IAuditService`)**: Injected into `AdminController`, `CustomerController`, and `VendorController` to log:
  - Seller product creation and inventory adjustments.
  - Admin seller approval, rejection, and blocking.
  - Customer order cancellation and return filing.
  - System exceptions and database backups.
- **Audit API**: `GET /Admin/GetAuditLogs?actionFilter=...&roleFilter=...` for live inspection.

### Point 32: Final Testing, Database Backup & IIS Deployment
- **Production Configuration**: `appsettings.Production.json` configured for SQL connection pooling, logging thresholds, and secure cookies.
- **Database Backup Stored Procedure**: `sp_ShopNext_BackupDatabase` and `POST /Admin/CreateDatabaseBackup` for 1-click administrative database backups.
- **Architectural Flow**:
  $$\text{Customer (Shopping)} \longleftrightarrow \text{Seller (Store Mgmt)} \longleftrightarrow \text{Admin (Governance)} \Longrightarrow \text{ASP.NET Core 8 MVC} \Longrightarrow \text{EF Core / SPs} \Longrightarrow \text{SQL Server}$$

---

## 13. Point 33: Customer Complete Order & Return History Specification

### A. Feature Overview
Enables Administrators to inspect any customer's complete lifetime behavioral record, including total lifetime orders, deliveries, cancellations, returns, refunds, return rate %, cancellation rate %, and full order breakdown table.

### B. Core Metrics & Key Performance Indicators
1. **Total Orders**: Lifetime count of orders placed by the customer.
2. **Delivered Orders**: Total successfully delivered and completed orders.
3. **Cancelled Orders**: Total orders cancelled by the customer or store prior to dispatch.
4. **Returned Orders**: Total return requests filed and items returned.
5. **Refunded Orders**: Total refunds disbursed.
6. **Total Amount Spent**: Lifetime monetary value of delivered orders (₹).
7. **Return Rate (%)**: Percentage of total orders returned ($\frac{\text{Returned}}{\text{Total Orders}} \times 100$).
8. **Cancellation Rate (%)**: Percentage of total orders cancelled ($\frac{\text{Cancelled}}{\text{Total Orders}} \times 100$).

### C. Technical Implementation
- **DTOs**: `AdminCustomerHistoryDto` & `AdminCustomerOrderItemDto` in [Models/AdminDashboardViewModel.cs](file:///e:/Work/ShopNext/ShopNext/Models/AdminDashboardViewModel.cs).
- **Backend API**: `GET /Admin/GetCustomerHistory?customerId={id}` in [Controllers/AdminController.cs](file:///e:/Work/ShopNext/ShopNext/Controllers/AdminController.cs).
- **Frontend Modal**: `#customerDetailsModal` in [Views/Admin/Dashboard.cshtml](file:///e:/Work/ShopNext/ShopNext/Views/Admin/Dashboard.cshtml) displaying the 6-stat KPI grid, risk badges, customer contact details, and sticky-header order history table.

---

## 14. Point 34: Wrong Product Return & Product Swap Protection

### A. Problem Statement & Fraud Mitigation
In high-value e-commerce (smartphones, electronics, branded footwear), fraudulent customers may order an authentic/expensive product and attempt to return a swapped, broken, counterfeit, or wrong item.

ShopNext implements an **end-to-end anti-fraud audit trail** with physical verification checklists at outbound dispatch and inbound return pickup before any refund is authorized.

### B. Outbound Dispatch Record (Packing/Fulfillment Time)
When the seller packs and dispatches the product:
1. **Product SKU Saved**: Unique catalog stock keeping unit.
2. **Serial Number / IMEI Saved**: Unique hardware serial or IMEI recorded for high-value units.
3. **Dispatch Condition**: Sealed / Brand New condition recorded.
4. **Seller/Rider Verification Checklist**:
   - `Product Verified ✓`
   - `Quantity Verified ✓`
   - `SKU Verified ✓`
   - `Packaging Verified ✓`

### C. Inbound Return Inspection Desk (Return Pickup Time)
Upon return delivery to hub:
```
Product Received
      ↓
SKU / Serial Check
      ↓
Product Condition Check
      ↓
Original Product?
      ↓
YES → Refund Processed
NO  → Fraud Blocked & Investigation Alert Logged
```

### D. Return Status Progression
- `Return Requested`: Customer submitted return request with reason.
- `Under Verification`: Hub team physical inspection in progress.
- `Approved`: Physical checks passed; refund disbursed.
- `Rejected / Product_Swapped_Fraud`: Mismatch detected; automatic refund blocked and security audit entry generated.

### E. Technical Implementation
- **Model Fields**: [Models/OrderItem.cs](file:///e:/Work/ShopNext/ShopNext/Models/OrderItem.cs) and [Models/Order.cs](file:///e:/Work/ShopNext/ShopNext/Models/Order.cs) store `Sku`, `SerialNumber`, `DispatchCondition`, `ReturnReceivedSerial`, `IsReturnSkuMatched`, `IsReturnSerialMatched`, `IsReturnConditionMatched`, `ReturnStatus`, and `ReturnVerificationRemarks`.
- **Backend APIs**:
  - `GET /Admin/GetReturnVerificationDetails?orderId={id}`: Retrieves outbound dispatch records vs inbound inspection state.
  - `POST /Admin/VerifyAndProcessReturn`: Audits serial number & condition; blocks refund if mismatch detected; logs fraud alert to `AuditLogs`.
- **Admin UI**: [Views/Admin/Dashboard.cshtml](file:///e:/Work/ShopNext/ShopNext/Views/Admin/Dashboard.cshtml) `#panel-returns` with `#verifyReturnModal` inspection workstation.

---

## 15. Point 35: Return Evidence System & Visual Comparison Desk

### A. Purpose & Evidence Pipeline
To resolve return disputes impartially between customers, merchants, and delivery partners, ShopNext tracks multi-touchpoint photographic evidence:
1. **Outbound Dispatch Evidence (Original State)**:
   - 📦 **Packing Photos**: Tamper-evident security tape, outer carton condition, and shipping labels.
   - 🌟 **Original Product Photos**: High-resolution image of the sealed product box with serial/IMEI sticker.
   - 🚚 **Delivery Proof Photos**: Rider doorstep handover photo before customer OTP verification.
2. **Inbound Return Evidence (Claimed State)**:
   - ⚠️ **Customer Damaged / Defect Photos**: Photo evidence submitted during return request.
   - 🛵 **Return Pickup Photos**: Rider physical pickup inspection photo at customer location.
   - 🏪 **Seller / Hub Dispute Evidence**: Quality control intake unboxing photo capturing returned condition.

### B. Storage Standard (Zero Heavy BLOBs in DB)
- **File Storage**: Images are stored in the server directory `wwwroot/uploads/evidence/` with unique GUID-based filenames.
- **Database References**: The SQL table `dbo.OrderEvidences` stores metadata references (`PhotoUrl`, `EvidenceType`, `Title`, `Description`, `UploadedByRole`, `UploadedDate`, `IsVerified`, `MetadataJson`).

### C. Admin Visual Comparison Workstation (`[Compare]`)
Administrators can open `#returnEvidenceModal` on any return order to inspect:
- **Left Pane**: Outbound Dispatch Evidence (Original sealed unit, packaging, delivery proof).
- **Right Pane**: Inbound Return Evidence (Customer damage claim, rider pickup inspection, hub intake).
- **Interactive Actions**:
  - 🔍 **`[Compare]`**: Side-by-side split comparison with zoom & pan lens.
  - ✅ **`[Approve Return]`**: Confirms genuine issue $\rightarrow$ marks `Approved` and triggers refund.
  - ❌ **`[Reject Return]`**: Confirms customer damage / product swap $\rightarrow$ marks `Rejected` and blocks refund.
  - 🔎 **`[Investigate]`**: Escalates to `Under_Verification` for lab audit / supervisor review.
  - 📤 **`[Upload Evidence]`**: Attaches additional inspection photos directly to the audit ledger.

### D. Technical Implementation
- **Model**: [Models/OrderEvidence.cs](file:///e:/Work/ShopNext/ShopNext/Models/OrderEvidence.cs) and `DbSet<OrderEvidence>` in [Models/ShopNextDbContext.cs](file:///e:/Work/ShopNext/ShopNext/Models/ShopNextDbContext.cs).
- **Backend APIs**:
  - `GET /Admin/GetReturnEvidenceDetails?orderId={id}`: Retrieves outbound vs inbound evidence collections.
  - `POST /Admin/ProcessEvidenceDecision`: Executes admin decisions (`Approve`, `Reject`, `Investigate`) with audit logging.
  - `POST /Admin/UploadOrderEvidence`: Handles multipart file uploads to `/uploads/evidence/`.
- **Admin UI**: [Views/Admin/Dashboard.cshtml](file:///e:/Work/ShopNext/ShopNext/Views/Admin/Dashboard.cshtml) `#returnEvidenceModal` modal and JavaScript comparison engine.

---

## 16. Point 36: Suspicious Customer Detection & Fraud Risk Engine

### A. Purpose & Risk Scoring Matrix
To combat abusive buyer behavior, repeat doorstep cancellations, fake claims, and fraudulent return swaps, ShopNext incorporates a real-time **Customer Risk Scoring Engine** (`CustomerRiskService`):

| Risk Level | Score Range | Badge Indicator | System Action / Policy |
| :--- | :--- | :--- | :--- |
| **Low Risk** 🟢 | `0 - 29` | `<span class="badge bg-success">Low 🟢</span>` | Standard checkout & normal delivery queues. |
| **Medium Risk** 🟡 | `30 - 69` | `<span class="badge bg-warning">Medium 🟡</span>` | Elevated scrutiny; recommended manual inspection before dispatch. |
| **High Risk** 🔴 | `70 - 100` | `<span class="badge bg-danger">High 🔴</span>` | Fraud alert triggered; auto-flagged for manual review, COD restrictions recommended. |

### B. Algorithmic Risk Factor Triggers
The risk engine dynamically audits consumer lifetime telemetry across several dimensions:
1. **High Return Rates**:
   - Return Rate $> 30\%$ on $\ge 3$ orders: **$+30$ points** (*"High Return Rate exceeding safe threshold"*).
   - Return Rate $> 20\%$ on $\ge 2$ orders: **$+15$ points** (*"Elevated Return Rate"*).
2. **Excessive Cancellation Rates**:
   - Cancellation Rate $> 25\%$ on $\ge 3$ orders: **$+25$ points** (*"Excessive Cancellation Rate"*).
   - Cancellation Rate $> 15\%$ on $\ge 2$ orders: **$+10$ points** (*"Elevated Cancellation Rate"*).
3. **Product Swap / Counterfeit Return Fraud History**:
   - Any order flagged with `ReturnStatus == "Product_Swapped_Fraud"`: **$+40$ points per incident** (*"Flagged for Wrong Product / Counterfeit Return Swap Fraud"*).
4. **COD Orders Repeatedly Rejected at Doorstep**:
   - Cash on delivery orders rejected upon rider arrival: **$+20$ points per incident** (*"Repeatedly rejected COD delivery at doorstep"*).
5. **Customer Complaints & Delivery Disputes**:
   - Multiple open/investigated disputes: **$+20$ points** (*"Multiple active customer complaints and disputes"*).
   - Single dispute ticket: **$+10$ points** (*"Customer dispute ticket on record"*).

### C. Admin Risk Dashboard & Interactive Controls
- **Customer Table (`#panel-customers`)**:
  - Live **Risk Assessment** column displaying colored risk badges (`High 🔴`, `Medium 🟡`, `Low 🟢`) with dynamic score chips (`85/100`).
  - Active status indicators for `[COD Blocked]` and `[Manual Review]`.
  - Action buttons: `[Risk Profile]`, `[View History]`, `[Block/Unblock]`.
- **Customer Fraud Risk Modal (`#customerRiskDetailsModal`)**:
  - Live circular/gauge risk meter with animated progress bar.
  - Active risk factors checklist with severity warning icons.
  - 6-stat telemetry grid: Orders, Return %, Cancel %, Swap Frauds, COD Doorstep Rejections, Disputes.
  - **Administrative Controls**:
    - 🚫 **`[Disable COD / Enable COD]`**: Restricts customer to prepaid payment methods only (UPI, Card, NetBanking).
    - 🚩 **`[Flag for Review / Unflag]`**: Requires manual store manager inspection before order dispatch.
    - ⛔ **`[Block Account]`**: Instantly suspends account access and prevents checkout.
    - 🔄 **`[Recalculate Score]`**: Real-time algorithmic re-evaluation with immediate UI update.

### D. Technical Implementation
- **Models**:
  - [Models/User.cs](file:///e:/Work/ShopNext/ShopNext/Models/User.cs): Added `RiskScore`, `RiskLevel`, `RiskFactorsJson`, `IsCodDisabled`, `IsFlaggedForReview`, `RiskLastEvaluatedDate`.
  - [Models/AdminDashboardViewModel.cs](file:///e:/Work/ShopNext/ShopNext/Models/AdminDashboardViewModel.cs): Added risk telemetry to `AdminCustomerDto` and `AdminCustomerHistoryDto`.
- **Service Layer**:
  - [Services/ICustomerRiskService.cs](file:///e:/Work/ShopNext/ShopNext/Services/ICustomerRiskService.cs) & [Services/CustomerRiskService.cs](file:///e:/Work/ShopNext/ShopNext/Services/CustomerRiskService.cs).
  - Registered in `Program.cs` as scoped dependency.
- **Backend APIs in [Controllers/AdminController.cs](file:///e:/Work/ShopNext/ShopNext/Controllers/AdminController.cs)**:
  - `GET /Admin/GetCustomerRiskDetails?customerId={id}`: Computes real-time risk profile and factor breakdown.
  - `POST /Admin/ToggleCustomerCod?customerId={id}`: Toggles Cash on Delivery restriction.
  - `POST /Admin/ToggleCustomerFlag?customerId={id}`: Toggles manual order inspection requirement.
  - `POST /Admin/RecalculateCustomerRisk?customerId={id}`: Recalculates risk score and persists to database with audit log.
- **Admin UI**:
  - [Views/Admin/Dashboard.cshtml](file:///e:/Work/ShopNext/ShopNext/Views/Admin/Dashboard.cshtml) `#customerRiskDetailsModal` modal and JavaScript handler functions.







