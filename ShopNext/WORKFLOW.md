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
    Engine->>DB: Apply Stock Hold Lock
    Customer->>UI: Select Delivery Slot & Apply Coupon
    Engine->>Engine: Strict Server Coupon Validation (Expiry, Min Order, User Limit)
    Customer->>UI: Select Payment Mode (UPI / Card / NetBanking / COD)
    
    alt Payment Successful
        UI->>Engine: Confirm Order & Payment
        Engine->>DB: Deduct Permanent Inventory, Commit Order
        Engine->>Engine: Calculate GST (CGST/SGST or IGST) & Generate Tax Invoice
        Engine->>Engine: Award Loyalty Points (1 Pt / ₹100)
        Engine->>Rider: Auto-Assign Nearest Rider via GPS
        UI-->>Customer: Order Confirmation + Live Map Tracking
    else Payment Failed
        Engine-->>UI: Non-Destructive Payment Failure Recovery
        UI-->>Customer: Prompt Instant Payment Retry without Cart Loss
    end
```

---

## 2. 🌐 Point 173: Multi-Language & Localization Architecture

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

## 3. 🚚 Hyperlocal Dispatch & Proof of Delivery (POD)

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
    Rider->>Server: Submit OTP + Geotag Location (Lat/Lng) + Timestamp
    
    alt OTP Matched
        Server->>Server: Verify POD & Mark Order 'Delivered'
        Server->>Customer: Delivery Verified SMS & Digital Invoice
    else 3 Failed Delivery Attempts
        Server->>Server: Auto-Schedule Return to Merchant (RTO)
    end
```

---

## 4. 🛡️ Return, Swap Fraud Prevention & 3-Party Grievance

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

## 5. 🧠 AI Customer Risk Score & Fraud Monitoring Engine

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

## 6. ⚡ High-Concurrency Flash Sale & Zero-Oversell Protection

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
        Lock->>DB: Reserve Item for 5 Minutes
        Lock-->>Buyers: Slot Granted $\rightarrow$ Proceed to Immediate Checkout
    else Stock Exhausted
        Lock-->>Buyers: Flash Sale Sold Out Banner
    end
```
