# Network Management App — Development Guide

A concise overview of **how this app was built**: its architecture, the layers, and the
key flows. The app is an **ASP.NET Core MVC** application (.NET 10) with REST API
controllers, server-rendered Razor *Views*, a vanilla-JS front-end, EF Core + PostgreSQL,
and a background monitoring service.

> Note: this is **MVC** (Controllers + Views + attribute-routed APIs), not Razor Pages.

---

## 1. High-level architecture

```mermaid
flowchart TB
    subgraph Client["Browser (vanilla JS)"]
        UI["Razor Views + JS bundles<br/>admin-devices.js, admin-device-detail.js, ..."]
    end

    subgraph Server["ASP.NET Core MVC (.NET 10)"]
        MVC["MVC Controllers<br/>Home / Account / Admin"]
        API["REST API Controllers<br/>/api/devices, /api/vlans, ..."]
        SVC["Service Layer<br/>DeviceService, VlanService,<br/>DeviceOperationsService, ..."]
        BG["Background Service<br/>DeviceMonitorService<br/>(pings every 2 min)"]
        REACH["ReachabilityService<br/>(real ICMP ping)"]
    end

    subgraph Data["Persistence"]
        EF["EF Core<br/>AppDbContext"]
        DB[("PostgreSQL")]
    end

    NET["Physical network<br/>switch / router / clients"]

    UI -->|"fetch() JSON"| API
    UI -->|"page loads"| MVC
    MVC --> SVC
    API --> SVC
    SVC --> EF
    BG --> SVC
    BG --> REACH
    REACH -. "ICMP" .-> NET
    EF --> DB
```

---

## 2. Request lifecycle (typical API call)

```mermaid
sequenceDiagram
    participant JS as Browser JS
    participant API as API Controller
    participant Auth as Auth + Role Policy
    participant Svc as Service
    participant EF as EF Core
    participant Log as IAppLogger

    JS->>API: fetch('/api/devices', POST)
    API->>Auth: [Authorize(AdminOrIT)]
    Auth-->>API: allowed
    API->>Svc: CreateAsync(dto, userId)
    Svc->>EF: validate + SaveChanges
    Svc->>Log: PersistDeviceAsync(...)
    Log->>EF: insert Log row
    Svc-->>API: DeviceDto
    API-->>JS: 201 Created (JSON)
```

---

## 3. Layered structure (folders)

```mermaid
flowchart LR
    A["Controllers/<br/>HTTP endpoints"] --> B["Services/<br/>business logic"]
    B --> C["Repository/<br/>AppDbContext + Models"]
    C --> D[("PostgreSQL")]
    E["Models/DTOs/<br/>request + response shapes"] -.-> A
    E -.-> B
    F["Views/ + wwwroot/<br/>UI + JS + CSS"] --> A
```

| Layer | Responsibility | Examples |
|-------|----------------|----------|
| **Controllers** | HTTP, routing, auth, status codes | `DevicesController`, `DeviceOperationsController`, `AdminController` |
| **Services** | Business rules, logging, validation | `DeviceService`, `VlanService`, `DeviceOperationsService` |
| **Repository** | EF Core `DbContext`, entities, relationships | `AppDbContext`, `Device`, `Vlan`, `Port`, `StaticRoute` |
| **DTOs** | Decoupled request/response models | `DeviceDto`, `SaveVlanDto`, `ProbeResultDto` |
| **Views + wwwroot** | Server-rendered pages, JS, CSS | `Device.cshtml`, `admin-device-detail.js` |

---

## 4. The device lifecycle (the core domain flow)

```mermaid
stateDiagram-v2
    [*] --> Offline: Add device (registered only)
    Offline --> Online: Connect (provision ports) / probe reachable
    Online --> Offline: Probe/monitor detects no response
    Offline --> Offline: Configure (IP, VLAN, ports, routing)
    Online --> [*]: Delete
    Offline --> [*]: Delete
```

- **Add** ? device stored as `Offline` (no network action).
- **Connect** ? marks `Online`, seeds default ports. *(SSH provisioning is a future TODO.)*
- **Probe** ? real ICMP ping; updates status to reflect reality.
- **Monitor** ? background sweep every 2 min keeps all statuses in sync.

---

## 5. Automatic monitoring loop

```mermaid
flowchart LR
    T["Timer (2 min)"] --> P["Ping every device<br/>(ICMP)"]
    P --> C{"Status<br/>changed?"}
    C -->|No| T
    C -->|Yes| U["Update status in DB"]
    U --> L["Log as 'system' user<br/>(Activity Logs)"]
    L --> T
```

---

## 6. Roles & access (cross-cutting)

```mermaid
flowchart TB
    Admin["Admin"] -->|full access| All["All features + Users + delete VLANs + IP range"]
    IT["IT"] -->|manage| Dev["Devices, VLANs, ports, routing, locations"]
    Guest["Guest"] -->|read-only| View["View dashboards + status"]
```

Enforced at the API via policies (`AdminOnly`, `AdminOrIT`) and mirrored in the UI.

---

## 7. How it was built — step by step

1. **Foundation** — MVC project, EF Core + PostgreSQL, cookie auth, role policies, `DataSeeder`.
2. **Core entities** — Users, Roles, Devices, DeviceTypes, Locations, Logs.
3. **Activity logging** — `IAppLogger` persists auditable actions; categorized log viewer.
4. **Network settings** — admin-managed IP range; device IPs validated/suggested against it.
5. **Device management** — CRUD, unique IPv4 validation, connect-on-demand workflow.
6. **Device operations** — probe, status, change IP, VLAN attach/detach, move, ports, routing.
7. **Device detail page** — full-page view with a right-hand action sidebar (role-aware).
8. **Real reachability** — `ReachabilityService` (ICMP) + `DeviceMonitorService` (2-min sweeps).
9. **System audit** — reserved `system` account attributes automated status changes.
10. **Deployment** — published to IIS (LanTest environment) for local/LAN testing.

---

## 8. Tech stack

| Area | Technology |
|------|-----------|
| Framework | ASP.NET Core **MVC**, .NET 10, C# 14 |
| Data | EF Core, PostgreSQL (Npgsql) |
| Auth | Cookie authentication + role-based policies |
| Front-end | Razor Views, vanilla JS, WebOptimizer bundling |
| Background | `BackgroundService` (hosted) + ICMP ping |
| Hosting | IIS (`AspNetCoreModuleV2`, in-process) |
```
