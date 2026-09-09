# 🔄 ShopNext - Core Workflows & Architecture Guide

This document illustrates the complete end-to-end architectural workflows of the ShopNext platform using Mermaid visual diagrams.

---

## 1. 🛒 Customer Shopping & Order Lifecycle

```mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant UI as Browser / App
    participant Engine as ShopNext Server
    participant DB as SQL Server (EF Core)
    participant Rider as Delivery Partner

    Customer->>UI: Browse Products & Add to Cart
    UI->>Engine: Reserve Stock for 10 Mins (Token Issued)
    Engine->>DB: Apply Stock Hold Lock (dbo.StockReservations)
    Customer->>UI: Select Delivery Slot & Apply Coupon
    Engine->>Engine: Strict Server Coupon Validation (Expiry, Min Order, User Limit)
    Customer->>UI: Select Payment Mode (UPI / Card / NetBanking / COD)
    
    alt Payment Successful
        UI->>Engine: Confirm Order & Payment
        Engine->>DB: Deduct Permanent Inventory, Commit Order (dbo.Orders)
        Engine->>Engine: Calculate GST (CGST/SGST or IGST) & Generate Tax Invoice
        Engine->>Engine: Award Loyalty Points (1 Pt / ₹100) (dbo.RewardPoints)
        Engine->>Rider: Auto-Assign Nearest Rider via GPS
        UI-->>Customer: Order Confirmation + Live Map Tracking
    else Payment Failed
        Engine-->>UI: Non-Destructive Payment Failure Recovery
        UI-->>Customer: Prompt Instant Payment Retry without Cart Loss
    end
```

---

## 2. 🛡️ System Monitoring, Live Sessions & Diff Audit Lifecycle (Point 174)

```mermaid
graph TD
    A[User Action: Login / Browse / Edit / Delete] --> B[HTTP Request + Cookie Auth Token]
    
    B --> C{Action Type}
    
    C -->|Authentication| D[AccountController / Login / Logout]
    D --> D1[Record dbo.LoginHistories: IP, Device, Browser, Status]
    D --> D2[Upsert dbo.UserSessions: Token, IsActive=1, Heartbeat]
    
    C -->|Normal Activity / API| E[ISystemMonitoringService: LogActivityAsync]
    E --> E1[Record dbo.UserActivities: Controller, Action, Method, Payload]
    E --> E2[Update UserSession: LastSeenTime = GETDATE()]

    C -->|Record Update| F[Entity Update Interceptor]
    F --> F1[Compute JSON Diff: OldValues vs NewValues]
    F --> F2[Save to dbo.EntityChangeLogs: EntityName, RecordId, ChangedBy, JSON Diff]

    C -->|Record Delete| G[Soft Delete Engine]
    G --> G1[Set Target Entity: IsDeleted = 1]
    G --> G2[Save to dbo.SoftDeleteLogs: EntityName, RecordId, DeletedBy, OriginalJson]

    H[Admin Command Center: /Admin/SystemMonitoring] --> I1[View Real-Time Telemetry Counters]
    H --> I2[Inspect Live User Sessions & 1-Click Force Logout]
    H --> I3[Inspect Entity Diffs with Before vs After Highlighting]
    H --> I4[1-Click Restore Soft-Deleted Records from Recycle Bin]
    H --> I5[Acknowledge Threat Alerts & Export Audit CSV]
```

---

## 3. 🌐 Point 173: Multi-Language & Localization Architecture

```mermaid
graph TD
    A[User Selects Language in Navbar Dropdown] --> B{Storage Strategy}
    B -->|Cookie| C1[Cookie: ShopNext_Language]
    B -->|Local Storage| C2[localStorage: shopnext_selected_language]
    B -->|Async POST| C3[/Home/SetLanguageJson]

    C1 & C2 & C3 --> D[Client-Side Localization Engine: localization.js]
    D --> E1[Translate data-i18n Elements]
    D --> E2[Translate Search Placeholders data-i18n-placeholder]
    D --> E3[Update Active Flag & Language Badge]

    E1 & E2 --> F[MutationObserver Watcher]
    F -->|Detects dynamic AJAX HTML| G[Auto-Translates Modals, Cart & Product Cards]

    C1 --> H[Server-Side ILocalizationService]
    H --> I1[Server Rendered Razor Views]
    H --> I2[Localized Controller Validation Errors]
    H --> I3[Localized Tax Invoices & SMS/Email Alerts]
```

---

## 4. 🚚 Hyperlocal Dispatch & Proof of Delivery (POD)

```mermaid
sequenceDiagram
    autonumber
    actor Merchant
    actor Rider
    actor Customer
    participant Server as ShopNext Backend
    participant Map as Leaflet.js GPS Engine

    Merchant->>Server: Pack Order & Print QC Packing Slip
    Server->>Rider: Dispatch Order Notification
    Rider->>Merchant: Doorstep Pickup & Scan Barcode
    Rider->>Server: Start Delivery Trip
    Server->>Customer: Order Out For Delivery + 4-Digit Handover OTP
    Map-->>Customer: Real-time Rider Route Telemetry (Lat/Lng)
    Rider->>Customer: Arrives at Customer Doorstep
    Customer->>Rider: Provides 4-Digit Delivery OTP
    Rider->>Server: Submit OTP + Geotag Location (Lat/Lng) + Timestamp (dbo.DeliveryProofs)
    
    alt OTP Matched
        Server->>Server: Verify POD & Mark Order 'Delivered'
        Server->>Customer: Delivery Verified SMS & Digital Invoice
    else 3 Failed Delivery Attempts
        Server->>Server: Log Failed Attempt (dbo.FailedDeliveryLogs)
        Server->>Server: Auto-Schedule Return to Merchant (RTO)
    end
```

---

## 5. 🛡️ Return, Swap Fraud Prevention & 3-Party Grievance

```mermaid
graph TD
    A[Customer Requests Return] --> B[Customer Submits Reason & Unboxing Proof Photos]
    B --> C{Return Value & Category Check}
    
    C -->|High Value / Dispute| D[Escalate to Level 3 / 4 Admin Audit]
    C -->|Standard Return| E[Assign Rider for QC Doorstep Pickup]

    E --> F[Rider Inspects Product Serial & Physical Condition]
    F -->|Serial/Condition Mismatched| G[Flag as 'Product_Swapped_Fraud']
    F -->|Item Verified OK| H[Return Accepted & Returned to Merchant]

    G --> I[3-Party Grievance Investigation]
    I --> I1[Pillar 1: Customer Statement & Unboxing Photo]
    I --> I2[Pillar 2: Merchant Dispatch QC Video Seal]
    I --> I3[Pillar 3: Rider Handover Telemetry Log]
    I --> I4[Pillar 4: Historical Customer Risk Score]

    I1 & I2 & I3 & I4 --> J[Admin Final Decision]
    J -->|Customer Fraud| K[Reject Return + Add 30 Pts to Customer Risk Score]
    J -->|Merchant/Rider Fault| L[Approve Instant Refund to Customer Digital Wallet]
```

---

## 6. 🧠 AI Customer Risk Score & Fraud Monitoring Engine

```mermaid
graph TD
    A[Customer Behavior Telemetry] --> B[Risk Scoring Engine]
    
    B --> C1[Returns Count x 25]
    B --> C2[Cancellations Count x 15]
    B --> C3[Doorstep COD Rejections x 30]
    B --> C4[Dispute Grievances x 20]
    B --> C5[Order Velocity Anomalies x 10]

    C1 & C2 & C3 & C4 & C5 --> D{Calculated Total Risk Score}

    D -->|Score 0 - 29| E[🟢 Low Risk - Full Access, All Payment Modes]
    D -->|Score 30 - 59| F[🟡 Medium Risk - Warning Banner, Manual Return QC]
    D -->|Score 60 - 100| G[🔴 High Risk - Block COD, Require Prepayment, Account Restriction]
```

---

## 7. ⚡ High-Concurrency Flash Sale & Zero-Oversell Protection

```mermaid
sequenceDiagram
    autonumber
    actor Buyers as 10,000+ Flash Sale Buyers
    participant Gate as Flash Sale Rate Limiter
    participant Lock as Concurrency Inventory Guard
    participant DB as SQL Server Database

    Buyers->>Gate: Flash Sale Opens (Concurrent Requests)
    Gate->>Gate: Anti-Bot & Multi-Account Detection Check
    Gate->>Lock: Request Stock Hold Slot
    
    alt Stock Available
        Lock->>Lock: Decrement Memory Hold Counter (Atomically)
        Lock->>DB: Reserve Item for 5 Minutes (dbo.StockReservations)
        Lock-->>Buyers: Slot Granted $\rightarrow$ Proceed to Immediate Checkout
    else Stock Exhausted
        Lock-->>Buyers: Flash Sale Sold Out Banner
    end
```

---

## 8. 🔒 Multi-Role Session Isolation & Cross-Role Cookie Purge

```mermaid
graph TD
    A[User Enters Login Credentials] --> B[AccountController.Login / Post]
    
    B --> C{Authentication Valid?}
    C -->|No| D[Return Error: Invalid Credentials]
    C -->|Yes| E[Execute Session Isolation Engine]
    
    E --> F[Wipe Conflicting Stale Session Cookies]
    F --> F1[Delete 'AdminAuth' Cookie]
    F --> F2[Delete 'ShopId' Cookie]
    F --> F3[Delete 'RiderId' Cookie]
    F --> F4[Delete 'CustomerId' Cookie]
    
    F1 & F2 & F3 & F4 --> G[Issue Fresh Role-Specific Claims Principal]
    G --> G1[Set ClaimTypes.Role = TargetRole]
    G --> G2[Set ClaimTypes.NameIdentifier = UserId]
    
    G1 & G2 --> H[Sign In With Claims & Set Role Cookie]
    
    H --> I{Assigned Role}
    I -->|Admin| J1[Redirect -> /Admin/Dashboard]
    I -->|Vendor| J2[Redirect -> /Vendor/Dashboard]
    I -->|Rider| J3[Redirect -> /Rider/Dashboard]
    I -->|Customer| J4[Redirect -> /Home/Index]
    
    J1 & J2 & J3 & J4 --> K[_Layout.cshtml Dynamic Role Navigation & theme-dark-portal Class]
```

---

## 9. 📍 Live GPS Auto-Detection & Interactive Map Pin-Picker (Leaflet.js)

```mermaid
sequenceDiagram
    autonumber
    actor Customer as Customer / Rider
    participant UI as Leaflet Map Component (Cart / Profile)
    participant GPS as Browser Geolocation API
    participant OSM as OpenStreetMap Nominatim Engine
    participant Server as ShopNext Backend
    participant DB as SQL Server (dbo.CustomerAddresses)

    Customer->>UI: Clicks "Detect Current Location"
    UI->>GPS: Request navigator.geolocation.getCurrentPosition()
    GPS-->>UI: Returns Latitude & Longitude (WGS84)
    UI->>UI: Pan Map View & Place Draggable Pin Marker
    UI->>OSM: Reverse Geocode Query (nominatim.openstreetmap.org/reverse?lat=X&lon=Y)
    OSM-->>UI: Returns Address (Road, Suburb, City, State, Postcode)
    UI->>UI: Auto-Fill Address Inputs (Street, City, Pincode)
    
    opt Pin Drag / Fine Adjustment
        Customer->>UI: Drags Map Pin to Precise Doorstep
        UI->>OSM: Re-query Reverse Geocoding with New Coordinates
        OSM-->>UI: Updates Form Input Values in Real-Time
    end

    Customer->>UI: Submits Order / Updates Address
    UI->>Server: POST Geocoded Address + Latitude + Longitude
    Server->>DB: Upsert dbo.CustomerAddresses / Update Order Geotag
```

---

## 10. 🎨 Universal High-Contrast Dark Portal UI Flow

```mermaid
graph TD
    A[User Navigates to Portal View] --> B[_Layout.cshtml View Engine]
    
    B --> C{Is Portal Controller?}
    C -->|Vendor / Admin / Rider| D[Inject 'theme-dark-portal' class onto &lt;body&gt;]
    C -->|Customer / Home| E[Standard Clean Light/Dark Adaptive Canvas]
    
    D --> F[CSS Cascade & High-Contrast Tokens: site.css]
    F --> F1[Canvas: Deep Slate #090d16 with Radial Gradient]
    F --> F2[Headings: 100% Bright White #ffffff]
    F --> F3[Subtext & Muted: High-Contrast Light Silver #cbd5e1]
    F --> F4[Active Glyphs: Sky Blue #60a5fa, Emerald #34d399, Amber #fbbf24]
    
    F1 & F2 & F3 & F4 --> G[Unified Portal Views: Zero Dark-on-Dark or Contrast Glitches]
```

