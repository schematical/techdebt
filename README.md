# TechDebt - The Game

A 2D Unity management/simulation game where you play as a startup CTO managing infrastructure, developers, and releases while battling technical debt, security threats, and scaling challenges.

## Overview

TechDebt is a roguelite management sim built in Unity (WebGL + Standalone). You run a software company through sprints, managing network infrastructure, hiring DevOps engineers, researching technologies, deploying releases, and fending off attacks (bugs, phishing, DDoS, SQL injection, XSS). The game features a meta-progression system with prestige points that persist across runs.

---

## Architecture

### Core Managers

| File | Role |
|------|------|
| `GameManager.cs` | Singleton god-object. Holds all game state: infrastructure, NPCs, tasks, stats, events, releases, network packets, stakeholders. Manages the FixedUpdate game loop (packet ticking, tech debt accumulation, event spawning). |
| `GameLoopManager.cs` | Controls the day/sprint cycle. States: `Plan` → `Play` → `WaitingForNpcsToExpire` → `Summary`. Each day has a timer (`DayDurationSeconds = 120s`). Triggers daily income, traffic growth, attack accumulation. |
| `MetaGameManager.cs` | Static class. Handles persistent save/load (JSON to disk/IndexedDB). Tracks prestige points, completed runs, game stage, meta-stat allocations, and claimed rewards across runs. |
| `UIManager.cs` | Manages all UI panels, pause state, and screen transitions. |
| `PrefabManager.cs` | Factory for spawning prefabs by string ID (NPCs, packets, effects, etc). |

### Game Loop / Phases

```
StartNewGame()
  └─ Initialize() → sets up stats, network packet types, events, stakeholders
  └─ MetaGameManager.ApplyMetaRewards() → applies persistent bonuses
  └─ SetupRun() → builds infrastructure grid
  └─ Map.SetCurrentLevel(level) → starts first sprint

Sprint Cycle:
  Plan Phase → Player reviews tasks, hires NPCs, assigns priorities
  Play Phase → Real-time simulation (120s/day):
    - Network packets flow through infrastructure
    - NPCs execute tasks (code, deploy, fix bugs, research)
    - Events spawn (bugs, attacks)
    - Tech debt accumulates
    - Money earned/spent
  Day End → Stats reset, traffic increases, new day starts
  Launch Day → Victory conditions checked → Sprint complete or failed
  Summary → Rewards applied, next sprint selected from Product Road Map
```

### Game Stages (Meta-Progression)

```
Tutorial → Bootstrapped → Seed → SeriesA
```

Each stage unlocks new stakeholders, technologies, and level tiers.

---

## Key Systems

### Product Road Map / Levels (`Scripts/ProductRoadMap/`)

- **`Map.cs`** — Holds the pool of all available levels (sprints). Tracks the current level and global victory conditions.
- **`MapLevel.cs`** — Base class for sprint levels. Each level has:
  - Victory Conditions (must be met to complete the sprint)
  - Level Modifiers (random positive/negative effects)
  - Level Rewards (earned on completion)
  - Sprint Duration (number of days)
  - Dependency graph (levels unlock based on completed predecessors)

**Available Levels:**
| Level | Theme |
|-------|-------|
| `LaunchProductRoadMapLevel` | Initial product launch |
| `UserSignupProductRoadMapLevel` | User authentication (Cognito) |
| `MobileProductRoadMapLevel` | Mobile app support |
| `EmailProductRoadMapLevel` | Email service integration |
| `SocketChatProductRoadMapLevel` | Real-time chat |
| `GeoLocationProductRoadMapLevel` | Geolocation features |
| `CodePipelineLevel` | CI/CD pipeline |
| `Metrics1Level` | CloudWatch metrics |
| `SslLevel` | SSL/TLS security |
| `SaasLevel` | Multi-tenant SaaS |
| `CheckoutCartLevel` | Shopping cart |
| `OnlinePaymentsProductRoadMapLevel` | Payment processing |
| `SecurityAuditProductRoadMapLevel` | Security audit |
| `TutorialProductRoadMapLevel` | Tutorial/onboarding |

### Victory Conditions (`Scripts/ProductRoadMap/VictoryConditions/`)

- `InfraActiveVictoryCondition` — Specific infrastructure must be operational
- `SpecialReleaseVictoryCondition` — A special release must be deployed
- `UpTimeVictoryCondition` — Maintain uptime threshold
- `HasMoneyVictoryCondition` — End sprint with minimum money
- `TechnologyResearchVictoryCondition` — Research a specific technology
- `NetworkPacketLatencyVictoryCondition` — Keep latency below threshold

---

### Infrastructure / World Objects (`Scripts/WorldObjects/`)

- **`WorldObjectBase.cs`** — Base for all world objects (clickable, has state, placed on grid)
- **`InfrastructureInstance.cs`** — Core infrastructure class. Handles:
  - Load management (packets add load, load recovers over time)
  - Packet routing (receives packets, applies latency/cost, routes to next hop)
  - State: `Operational` / `Frozen` (overloaded = frozen)
  - Size tiers: Small → Medium → Large (multiplies capacity)
  - Per-second operational cost

**World Object Types (`Scripts/WorldObjects/Types/`):**
| Type | Real-World Analog |
|------|-------------------|
| `ApplicationServerWOType` | App server (deployment target) |
| `ALBWOType` | Application Load Balancer |
| `CDNWOType` | Content Delivery Network |
| `CognitoWOType` | AWS Cognito (auth) |
| `CloudWatchMetricsWOType` | CloudWatch (monitoring) |
| `CodePipelineWOType` | CI/CD Pipeline |
| `DedicatedDBWOType` | Dedicated Database |
| `EmailServiceWOType` | Email Service (SES) |
| `QueueWOType` | Message Queue (SQS) |
| `RedisWOType` | Redis Cache |
| `BinaryStorageWOType` | S3/Binary Storage |
| `SecretManagerWOType` | Secrets Manager |
| `SNSWOType` | Simple Notification Service |
| `WAFWOType` | Web Application Firewall |
| `WorkerServerWOType` | Background Worker |
| `WaterCoolerWOType` | Water Cooler (morale) |

**Special World Objects:**
- `Database.cs` — Database instance
- `Desk.cs` — Stakeholder/NPC workstation
- `OrgChart.cs` — Org chart display
- `WhiteBoard.cs` — Tech tree display
- `WaterCooler.cs` — NPC morale boost
- `CodePipelineInstance.cs` — CI/CD pipeline instance
- `ProductRoadMap.cs` — The in-game product roadmap board

---

### NPCs (`Scripts/NPCs/`)

- **`NPCBase.cs`** — Abstract base. Has states: `Idle`, `ExecutingTask`, `Wandering`, `Exiting`, `Dead`. Handles pathfinding, task assignment, cooldowns, HP, attack/damage. NPCs check for work every 1s when idle.
- **`NPCDevOps.cs`** — Hireable engineer. Has XP, levels, modifier slots, and stats for code speed/quality, DevOps speed, research speed, security, etc. Levels up by gaining XP from tasks; on level-up the player picks a random modifier (roguelite card-pick mechanic).
- **`NPCStakeholder.cs`** — CEO/CTO/CISO/CFO/CMO stakeholders that drive the narrative and provide dialog choices.
- **`NPCBug.cs`** — Enemy NPC. Has severity (Minor → Medium). Minor bugs evolve into Medium bugs over time. Bugs attack infrastructure.
- **`NPCPhishingAttack.cs`** — Phishing attack enemy
- **`NPCMimic.cs`** — Mimic enemy
- **`BossNPC.cs`** — Boss-type enemy

---

### Task System (`Scripts/NPCTask/`)

- **`NPCTask.cs`** — Abstract base. States: `Available` → `Queued` → `Executing` → `Completed`/`Interrupted`. Has priority, role requirement, and target. NPCs auto-assign to highest-priority unassigned task.
- **Task Roles:** DevOps, Boss, Dev, Intern, Enemy, SchematicalBot

**Task Types:**
| Task | Purpose |
|------|---------|
| `CodeTask` | Write code for a release |
| `DeploymentTask` | Deploy a release to application server |
| `ResearchTask` | Research a technology at the whiteboard |
| `InfrastructureTaskBase` | Build/repair infrastructure |
| `BugConsumeTask` | Fix/kill a bug |
| `AttackTask` | Attack an enemy NPC |
| `PhishingTask` | Enemy phishing attack task |
| `RedirectTrafficTask` | Redirect network traffic |
| `DeliverItemTask` | Deliver a power-up item |
| `TutorialMoveToTask` | Tutorial movement guidance |

---

### Network Packets (`Scripts/NetworkPackets/`)

- **`NetworkPacket.cs`** — Represents a request/data packet flowing through infrastructure. Has states: `Running`, `Failed`, `Stolen`. Tracks latency, past nodes visited, and routes through connected infrastructure. Packets visually move between world objects.

**Packet Types:**
| Type | Prefab | Description |
|------|--------|-------------|
| `Purchase` | FileCoin | Revenue-generating purchase |
| `Text` | NetworkPacket | Standard text request |
| `Image` | FileCat | Image upload |
| `MaliciousText` | NetworkPacketAttack | SQL injection attempt |
| `BatchJob` | BatchJobNetworkPacket | Background batch job |
| `PII` | PIINetworkPacket | Personally identifiable info |
| `SQLInjection` | SQLInjectionNetworkPacket | SQL injection attack |

---

### Stats System (`Scripts/Stats/`, `Scripts/Enum/StatType.cs`)

A modifier-based stats system. Stats have base values plus additive/multiplicative modifiers.

**Key Global Stats:**
- `Money` — Currency (starting: $230)
- `TechDebt` — Accumulates over time; increases attack probability
- `Traffic` — Packets per day (starting: 30)
- `Difficulty` — Scales over time (starting: 1.25x)
- `AttackPossibility` — Probability accumulator for security events
- `ItemDropChance` — Chance for power-up drops
- `Global_DailyBudget` — Passive income per day
- `Global_DeploymentSpeed` / `Global_CodeSpeed` — Multipliers

**NPC Stats:** MovementSpeed, HP, CoolDown, AttackDamage, Energy, CodeSpeed, CodeQuality, DevOpsSpeed, ResearchSpeed, XPSpeed, ModifierSlots, etc.

**Infrastructure Stats:** MaxLoad, LoadRecoveryRate, BuildTime, LoadPerPacket, DailyCost, PacketCost, MaxSize, InputValidation, LatencyStartsAtLoad

---

### Releases (`Scripts/Release/ReleaseBase.cs`)

Software releases follow a pipeline:
```
InDevelopment → InTesting → InReview → DeploymentReady → DeploymentInProgress → DeploymentRewardReady → DeploymentCompleted
```
Or `Failed`.

Each release has:
- Version number
- Quality score (affected by code quality, tech debt)
- Attached bugs
- Reward modifier (roguelite bonus on successful deploy)
- Rarity tier

Releases are deployed to `ApplicationServer` instances via `DeploymentTask`.

---

### Events (`Scripts/Events/`)

Random events that spawn during the Play phase based on probability (influenced by tech debt, release quality, attack possibility):

| Event | Effect |
|-------|--------|
| `SpawnBugEvent` | Spawns a bug NPC |
| `SpawnXSSEvent` | Spawns an XSS attacker |
| `SpawnPhishingAttackEvent` | Spawns a phishing attack |
| `SpawnSQLInjectionEvent` | Spawns SQL injection |
| `SpawnDDoSEvent` | Marks an internet pipe as DDoS'd (traffic spike) |

Event probability scales with tech debt, low release quality, low input validation, and accumulated attack possibility.

---

### Technology Tree (`Scripts/Technology.cs`)

Technologies have states: `MetaLocked` → `Locked` → `Researching` → `Unlocked`.

Researched by NPCDevOps at the Whiteboard. Each technology has:
- Research time
- Unlock conditions (dependencies on other technologies)
- Direction in tech tree UI

Technologies unlock new infrastructure types and capabilities.

---

### Rewards (`Scripts/Rewards/`)

- `RewardBase` — Abstract reward
- `StatBaseValueReward` — Modifies a stat's base value
- `StatModifierReward` — Adds a stat modifier
- `StakeHolderReward` — Stakeholder-related reward
- `MetaStatBaseValueReward` — Persistent meta-progression reward
- `LeveledRewardBase` — Reward with level tiers
- `SpecialCallbackReward` — Custom callback reward

---

### Stakeholders (`Scripts/Stakeholders/Stakeholder.cs`)

C-suite executives (CEO, CTO, CISO, CFO, CMO) that:
- Provide narrative dialog choices
- Have leveled unlock tiers (e.g., CISO: Junior Security Officer → Security Consultant → CISO)
- Are unlocked via prestige points in meta-progression
- Each has dependency requirements (all report to CEO)
- Drive which levels/missions are available

---

### Meta-Progression (`Scripts/Meta/`)

- **`MetaProgressData.cs`** — Persistent save data: completed runs, prestige points, game stage, allocations, claimed rewards, meta-stats.
- **`MetaGameManager.cs`** — Static manager for save/load, prestige point allocation, applying meta-rewards at run start.
- **`MetaPrestigePointAllocatable.cs`** — Items that can receive prestige point investment.
- **`MetaStat.cs`** — Cumulative stats tracked across all runs.

**Prestige Flow:**
1. Complete a run → earn prestige points
2. Allocate prestige points into org chart (stakeholders), technologies, or bonuses
3. Next run starts with those bonuses applied

---

### Tutorial (`Scripts/Tutorial/`)

- `TutorialManager.cs` — Manages tutorial step progression
- `TutorialStep.cs` — Individual tutorial step definition
- `TutorialProductRoadMapLevel.cs` — Special level for new players

---

### UI (`Scripts/UI/`)

Major panels:
- `UIMainMenu` — Title screen
- `UITopBarPanel` — HUD (money, day timer, stats)
- `UIProductRoadMap` — Sprint/level selection
- `UIMapPanel` — Tech tree / level map display
- `UIVictoryConditionListPanel` — Current sprint objectives
- `UINPCDetailPanel` — NPC info/stats
- `UIWorldObjectDetailPanel` — Infrastructure detail
- `UIMultiSelectPanel` — Roguelite card-pick (level-up rewards)
- `UIOrgChartPanel` — Organization chart
- `UIMetaUnlockMapPanel` — Meta-progression unlock tree
- `UISaveSlotListPanel` / `UISaveSlotDetailPanel` — Save management
- `UIRewardPanel` — Reward display
- `UITimeControlPanel` — Speed controls
- `UIPauseMenu` — Pause menu
- `UIDebugPanel` — Debug tools
- `UIToastHolderPanel` — Toast notifications
- `UISummaryPhasePanel` — End-of-sprint summary
- `UIRunSetupPanel` — Run configuration
- `UIChallengeSelectPanel` / `UIMetaChallengesPanel` — Challenge selection
- `UIReleaseHistoryPanel` — Past releases
- `UIGlobalStatsPanel` — Global stats display
- `UITechTreePanel` — Technology research tree

---

### Other Systems

- **Items (`Scripts/Items/`)** — Power-ups: NukeItem, FreezeTimeItem, EnergyDrinkItem
- **Effects (`Scripts/Effects/`)** — Temporary timed effects on game state
- **Rarity (`Scripts/Rarity.cs`)** — Rarity tiers for rewards/modifiers
- **Analytics (`Scripts/Util/Analytics/`)** — Unity Analytics events (DaySummary, PrestigePointAllocation, RewardInteraction)
- **Steam Integration (`Scripts/Util/SteamManager.cs`)** — Steamworks SDK integration (conditionally compiled)
- **EnvGraphic (`Scripts/EnvGraphic/`)** — Visual effects (level-up animation, evolve effect)

---

## Build Targets

- **WebGL** — Primary (deployed to itch.io, uses IndexedDB for saves)
- **Standalone** (Win/Linux/Mac) — With Steam integration
- Current version: **0.0.48**

## Project Structure

```
/opt/techdebt/
├── TechDebt/                    # Unity project root
│   ├── Assets/
│   │   ├── Scenes/             # GameScene.unity (single scene)
│   │   ├── Scripts/            # All C# source (~150 files)
│   │   │   ├── Enum/           # Enums (StatType, GameStage)
│   │   │   ├── Events/         # Random event spawners
│   │   │   ├── Meta/           # Meta-progression system
│   │   │   ├── NetworkPackets/ # Packet simulation
│   │   │   ├── NPCs/           # NPC classes
│   │   │   ├── NPCTask/        # Task system
│   │   │   ├── ProductRoadMap/ # Sprint/level system
│   │   │   │   ├── Level/      # Individual level definitions
│   │   │   │   └── VictoryConditions/
│   │   │   ├── Release/        # Software release pipeline
│   │   │   ├── Rewards/        # Reward system
│   │   │   ├── Stakeholders/   # C-suite stakeholders
│   │   │   ├── Stats/          # Stat/modifier system
│   │   │   ├── Tutorial/       # Tutorial system
│   │   │   ├── UI/             # All UI panels
│   │   │   │   ├── MetaUnlock/ # Meta-progression UI
│   │   │   │   └── UIPanel/    # Base panel components
│   │   │   ├── Util/           # Utilities, analytics, Steam
│   │   │   └── WorldObjects/   # Infrastructure & world objects
│   │   │       └── Types/      # World object type definitions
│   │   ├── Prefabs/            # Prefab assets
│   │   │   └── NPCs/           # NPC prefabs
│   │   ├── Sprites/            # Sprite assets
│   │   └── Settings/           # URP settings
│   ├── Library/                # Unity cache (auto-generated)
│   └── Temp/                   # Build temp files
└── output/                     # WebGL build output
    ├── index.html
    └── Build/                  # .wasm.br, .framework.js.br, .data.br
```

---

## Key Design Patterns

1. **Singleton** — `GameManager.Instance` accessed globally
2. **Observer/Events** — C# events for state changes (`OnStatsChanged`, `OnInfrastructureStateChange`, `OnPhaseChange`, etc.)
3. **Task Queue** — Priority-based task assignment; NPCs self-assign from global queue
4. **Modifier Stack** — Stats are base + list of named modifiers (additive)
5. **Roguelite Card Pick** — On NPC level-up, choose from random reward pool
6. **Meta-Progression** — Prestige points invested between runs into a persistent skill tree
7. **Probability Weighting** — Events and packet types use weighted random selection
