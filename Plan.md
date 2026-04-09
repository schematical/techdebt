# Project Plan - Reusable Map Component & Meta Progression

## Current Objective
Refactor `UITechTreePanel.cs` into a reusable `UIMapPanel` component and implement the `iMapNode` interface to support Tech Tree, Org Chart, Meta Unlock Map, and Product Road Map. Integrate `PrestigePoints` meta-currency for the Meta Unlock Map.

## Phase 1: Research & Assumptions
- **Key Files & Systems:** `UITechTreePanel.cs`, `Technology.cs`, `MetaGameManager.cs`, `UIPanel.cs`, `UIPanelLine.cs`.
- **PrestigePoints:** Need to ensure `PrestigePoints` is added to `MetaProgressData.cs` and handled in `MetaGameManager`.
- **Validation:** 
  - `iMapNode` must support: `Id`, `DisplayName`, `Description`, `Icon`, `Direction`, `DependencyIds`, `CurrentState` (mapped from specific systems), and `OnSelected(UIMapPanel panel)`.
  - `UIMapPanel` must be abstract with a `PopulateNodes()` method.

## Phase 2: Strategy
- **Approach:** 
  - **Interface:** Define `iMapNode` and generic enums for `MapNodeState` and `MapNodeDirection`.
  - **Meta Currency:** Add `PrestigePoints` to `MetaProgressData` and implement spending logic in `MetaGameManager`.
  - **Refactoring:** 
    - Create `UIMapPanel` (Abstract) inheriting from `UIPanel`. Move tilemap, panning, zooming, and procedural layout logic here.
    - `UIMapPanel` will work with a list of `iMapNode`.
    - `Technology` will implement `iMapNode`.
    - Create `UIMetaUnlockPanel` inheriting from `UIMapPanel` using `PrestigePoints`.
- **Architectural Alignment:** 
  - Use `UIPanelLine` system for the side detail pane.
  - No `var`, no Coroutines.
  - Tilemap Z-as-Y sorting.

## Phase 3: Execution Steps (Plan -> Act -> Validate)
- [ ] **Step 1:** Define `iMapNode`, `MapNodeState`, and `MapNodeDirection` in `interfaces.cs`.
- [ ] **Step 2:** Update `MetaProgressData.cs` to include `PrestigePoints`.
- [ ] **Step 3:** Update `Technology.cs` to implement `iMapNode`.
- [ ] **Step 4:** Create `UIMapPanel.cs` by refactoring `UITechTreePanel.cs`. Abstract the layout and node rendering.
- [ ] **Step 5:** Re-implement `UITechTreePanel.cs` as a concrete subclass of `UIMapPanel`.
- [ ] **Step 6:** Create `UIMetaUnlockPanel.cs` as a concrete subclass of `UIMapPanel`.
- [ ] **Step 7:** Implement `PrestigePoints` allocation logic in `MetaGameManager` and `iMapNode` implementations for meta unlocks.

## Phase 4: Validation & Testing
- **Test Strategy:** 
  - Verify Tech Tree still works exactly as before.
  - Verify Meta Unlock Map renders nodes correctly.
  - Verify `OnSelected` updates the detail pane using `UIPanelLine` system.
## Product Road Map Visual
```mermaid
graph LR
    Start{{"Start"}}:::start --> CTO_Star{{"Security CTO"}}:::boss
    Start --> CISO_Star{{"Security CISO"}}:::boss
    Start --> CFO_Star{{"Security CFO"}}:::boss
    Start --> CMO_Star{{"Marketing CMO"}}:::boss

    %% === CISO (Left Branch) ===
    CISO_Star --> SSL["SSL / Man-in-the-Middle Attack"]:::node
    SSL --> Auth["Multi-Factor Authentication"]:::node
    SSL --> Keys["Priv. Keys"]:::node
    SSL --> Private_Subnets["Private Subnets"]:::node
    SSL --> PoLA["Principals of Least Access"]:::node
    SSL --> Content["Content Curation"]:::node
    SSL --> Firewall["Firewall"]:::node

    %% === CFO (Up Branch) ===
    CFO_Star --> SaaS["Subscription Models SaaS / Internal"]:::node
    CFO_Star --> S3["S3 / Glacier storage classes"]:::node
    CFO_Star --> Savings["Savings Plans / Spot Instances"]:::node
    CFO_Star --> CostExplorer["Cost Explorer / Understand where the cost lies"]:::node
    CostExplorer --> CostOpt["Cost Optimization"]:::node
    CostOpt --> BetterChart["Cost Explorer 2 / Better Chart"]:::node
    Savings --> Insure["Cyber Security Insurance"]:::node

    %% === CTO (Down Branch) ===
    CTO_Star --> Disk["Disk Space"]:::node
    CTO_Star --> Linting["Code Linting"]:::node
    CTO_Star --> Metrics["Metrics"]:::node
    Metrics --> Alarms["Cloud Alarms / Optimize Task For Devs"]:::node
    Metrics --> AutoScale["Auto Scaling"]:::node
    AutoScale --> RAM["RAM / Memory Leak"]:::node
    CTO_Star --> CodePipeline["CodePipeline"]:::node
    CodePipeline --> TDD["Test Driven Development"]:::node
    TDD --> Tests["Automated Tests"]:::node
    Tests --> ManMinute["Mythical Man Minute"]:::node
    CTO_Star --> VC["Version Control"]:::node
    VC --> LocalDev["Local Dev Env / Speed"]:::node
    LocalDev --> VM["Virtual Machines / Containers / Docker For Devs"]:::node

    %% === CMO (Right Branch) ===
    CMO_Star --> Cart["Checkout Cart"]:::node
    Cart --> CartEmail["Abandon Cart Emails"]:::node
    CMO_Star --> SEOLatency["SEO Latency"]:::node
    SEOLatency -.-> HugOfDeath["Big Tech / Bot Crawl / Hug Of Death"]:::dotted
    CMO_Star --> PaidAds["Paid Ads"]:::node
    PaidAds --> Pixels["Tracking Pixels"]:::node
    Pixels --> SellData["Selling User Data"]:::node
    CMO_Star --> PII["PII / User Submitted Data"]:::node
    PII --> Credit["Credit Card Purchases / Safe Vault"]:::node
    PII --> Email["Email"]:::node
    PII --> SNS["SNS"]:::node
    PII --> Stream["Streaming Video"]:::node
    Stream --> Chat["Real Time Chat"]:::node
    
    %% === Bottom Right / AI Chain ===
    Chat --> Innovation_Star{{"Innovation Chief Innovation Officer"}}:::boss
    Chat --> AI_A["Slap a AI in it"]:::node
    AI_A --> ImageGen["Image Gen"]:::node
    AI_A --> ImageRec["Image Recognition"]:::node
    Chat --> MLOps["MLOps"]:::node
    MLOps --> CustomModels["Custom Models"]:::node
    
    %% === Agents Chain ===
    SEOLatency --> PersonalAgents["Customer's Personal Agents"]:::node
    PersonalAgents --> MCPServer["MCP Server"]:::node

    %% Global Node Styling
    classDef boss fill:#fff,stroke:#000,stroke-width:2px,shape:star;
    classDef start fill:#fff,stroke:#000,stroke-width:2px,shape:hexagon;
    classDef node fill:#fff,stroke:#000,stroke-width:1px,rx:5,ry:5;
    classDef dotted fill:#fff,stroke:#000,stroke-width:1px,rx:5,ry:5,stroke-dasharray: 5 5;
```
