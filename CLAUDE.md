# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TechDebt is a 2D Unity management/simulation game (roguelite) where the player is a startup CTO managing infrastructure, developers, and releases while battling technical debt, security threats, and scaling challenges. Built in Unity **6000.3.0f1** (Unity 6), targeting WebGL (primary, itch.io) and Standalone (Win/Linux/Mac, with Steam integration).

- Unity project root: `TechDebt/` (open this folder in Unity Editor / Rider via `TechDebt/TechDebt.sln`)
- All gameplay source: `TechDebt/Assets/Scripts/` (~150 C# files)
- Single scene: `TechDebt/Assets/Scenes/GameScene.unity`
- WebGL build output: `output/` (`.wasm.br`, `.framework.js.br`, `.data.br`)
- Current version: 0.0.48

There is no CLI build/test workflow — this is developed through the Unity Editor / Rider. Build and iteration happen by opening the project in Unity and using the Editor's Build Settings (WebGL/Standalone targets) or Play mode.

## Coding Conventions

- **No `var` keyword** — always use explicit types when declaring variables.
- No coroutines — game loop timing goes through `GameLoopManager`/stat-driven ticks instead.

## Working Conventions

- Before running any CLI command, briefly explain *why* you're running it, then run it.

## Unity MCP Usage Notes

Common tool calls when driving the Editor via the `unityMCP` MCP server:

| Goal | Call |
|------|------|
| Check editor/play-mode state | `ReadMcpResourceTool` on `mcpforunity://editor/state` |
| List connected Unity instances | `ReadMcpResourceTool` on `mcpforunity://instances` |
| List project-specific custom tools | `ReadMcpResourceTool` on `mcpforunity://custom-tools` |
| Check for compile errors/warnings before entering Play mode | `mcp__unityMCP__read_console` with `action: "get"`, `types: ["error","warning"]` |
| Enter/exit Play mode | `mcp__unityMCP__manage_editor` with `action: "play"` / `"pause"` / `"stop"` |
| Find all NPCs in the scene | `mcp__unityMCP__find_gameobjects` with `search_method: "by_component"`, `search_term: "NPCBase"` (searching `by_component` with `"NPC"` returns nothing — the component type name must match exactly, e.g. `NPCBase`, `NPCDevOps`) |
| Find other objects by component/name/tag/layer/path | `mcp__unityMCP__find_gameobjects` — required param is `search_term` (not `query`), with `search_method` set to `by_name`/`by_tag`/`by_layer`/`by_component`/`by_path`/`by_id` |
| Inspect a specific GameObject's transform/components | `ReadMcpResourceTool` on `mcpforunity://scene/gameobject/{instanceID}` (instance IDs come back from `find_gameobjects`, often negative for scene objects) |
| Inspect just a GameObject's components | `mcpforunity://scene/gameobject/{instanceID}/components` |

Note: `mcpforunity://editor/state` can return `"advice":{"ready_for_tools":false,"blocking_reasons":["stale_status"]}` even when the Editor is actually responsive — don't block on it, just proceed and re-check if a subsequent call fails.

## Architecture

### Core Managers (singletons/statics, top level of `Assets/Scripts/`)

| File | Role |
|------|------|
| `GameManager.cs` | Singleton god-object holding all game state: infrastructure, NPCs, tasks, stats, events, releases, network packets, stakeholders. Drives the `FixedUpdate` game loop (packet ticking, tech debt accumulation, event spawning). |
| `GameLoopManager.cs` | Controls the day/sprint cycle: `Plan` → `Play` → `WaitingForNpcsToExpire` → `Summary`. Each day has a timer (`DayDurationSeconds = 120s`). Triggers daily income, traffic growth, attack accumulation. |
| `MetaGameManager.cs` | Static class handling persistent save/load (JSON to disk/IndexedDB). Tracks prestige points, completed runs, game stage, meta-stat allocations, claimed rewards across runs. |
| `UIManager.cs` | Manages all UI panels, pause state, screen transitions. |
| `PrefabManager.cs` | Factory for spawning prefabs by string ID (NPCs, packets, effects, etc). |

### Game Loop

`GameLoopManager.CurrentState` (`GameState` enum: `Plan`, `Play`, `WaitingForNpcsToExpire`, `Summary`) in practice only ever toggles between **`Plan`** and **`Play`** — `WaitingForNpcsToExpire` and `Summary` are declared but never assigned anywhere in the current codebase (checked elsewhere, e.g. `NPCBase.cs`, but dead in `GameLoopManager` itself).

```
StartNewGame() (GameManager.cs)
  Initialize() → sets up stats, network packet types, events, stakeholders
  MetaGameManager.ApplyMetaRewards() → applies persistent bonuses
  SetupRun() → builds infrastructure grid
  Map.SetCurrentLevel(level) + GameLoopManager.BeginPlanPhase() → starts first sprint

BeginPlanPhase() — runs ONCE at the start of a sprint (not every day):
  player reviews tasks, hires NPCs, assigns priorities.
  UIPlanPhaseMenuPanel's "Start Day" button calls BeginPlayPhase().

BeginPlayPhase() → CurrentState = Play, dayTimer resets, NPCs notified.

While CurrentState == Play, FixedUpdate() ticks dayTimer; when it hits
DayDurationSeconds (120s), GameLoopManager.TriggerNextDay() runs —
still inside Play, no state change:
  - MapLevel.PostSummaryCheck() → checks victory conditions for the day;
    fails the run via EndGame() if lost, or (on the final/"launch" day)
    calls MapLevel.OnLaunchDaySummary() → MarkCompleted() on the level
  - currentDay++, MapLevel.PlanPhaseCheck() → applies day-1/launch-day
    level modifiers (NOT a state transition to Plan)
  - daily stats reset, traffic/attack-possibility increase, daily budget paid

On sprint completion the player picks the next sprint from the Product
Road Map (UIProductRoadMap "Start Sprint" button), which calls
Map.SetCurrentLevel(...) + BeginPlanPhase() again for the new sprint.
```

Meta-progression stages (unlock new stakeholders/technologies/level tiers): `Tutorial → Bootstrapped → Seed → SeriesA`.

### Product Road Map / Levels (`Scripts/ProductRoadMap/`)

- `Map.cs` — pool of all available sprint levels; tracks current level and global victory conditions.
- `MapLevel.cs` — base class for a sprint level: victory conditions, level modifiers, level rewards, sprint duration (days), dependency graph (unlocks based on completed predecessors).
- Concrete levels live alongside it (e.g. `LaunchProductRoadMapLevel`, `SslLevel`, `CheckoutCartLevel`, `SecurityAuditProductRoadMapLevel`, `TutorialProductRoadMapLevel`, etc.) — each themed around a real product/infra milestone.
- `Scripts/ProductRoadMap/VictoryConditions/` — pluggable win conditions (`InfraActiveVictoryCondition`, `UpTimeVictoryCondition`, `HasMoneyVictoryCondition`, `TechnologyResearchVictoryCondition`, `NetworkPacketLatencyVictoryCondition`, `SpecialReleaseVictoryCondition`).

### Infrastructure / World Objects (`Scripts/WorldObjects/`)

- `WorldObjectBase.cs` — base for all placeable/clickable world objects. Defines `State { Locked, Unlocked, Planned, Operational, Frozen }`, inherited by every world object (infrastructure, desks, etc.) via `CurrentState`.
- `InfrastructureInstance.cs` — core infra class built on `WorldObjectBase`: load management (packets add load, recovers over time), packet routing (receive → apply latency/cost → route to next hop), size tiers (Small/Medium/Large multiply capacity), per-second operational cost. Overload transitions `CurrentState` to `Frozen`.
- `Scripts/WorldObjects/Types/` — one `WorldObjectType` per infra kind (real-world analogs: `ApplicationServerWOType`, `ALBWOType`, `CDNWOType`, `CognitoWOType`, `CloudWatchMetricsWOType`, `CodePipelineWOType`, `DedicatedDBWOType`, `EmailServiceWOType`, `QueueWOType`, `RedisWOType`, `BinaryStorageWOType`, `SecretManagerWOType`, `SNSWOType`, `WAFWOType`, `WorkerServerWOType`, `WaterCoolerWOType`). Types define build/daily costs, unlock conditions (technologies), routing rules, and packet-load handling.
- Special objects: `Database.cs`, `Desk.cs`, `OrgChart.cs`, `WhiteBoard.cs` (tech tree display), `WaterCooler.cs` (NPC morale), `CodePipelineInstance.cs`, `ProductRoadMap.cs` (in-game roadmap board).
- `GridManager.cs` / `Pathfinding.cs` — grid placement and NPC pathfinding across the infrastructure layout (relevant to the current `feat/path-finding-fix` branch).

### NPCs (`Scripts/NPCs/`)

- `NPCBase.cs` — abstract base; states `Idle, ExecutingTask, Wandering, Exiting, Exited, Dead`; handles pathfinding, task assignment, cooldowns, HP, attack/damage. Idle NPCs poll for work every 1s.
- `NPCDevOps.cs` — hireable engineer with XP/levels, modifier slots, stats (code speed/quality, DevOps speed, research speed, security). Leveling up triggers a roguelite card-pick modifier choice.
- `NPCStakeholder.cs` — CEO/CTO/CISO/CFO/CMO stakeholders driving narrative/dialog.
- `NPCBug.cs` — enemy; severity Minor → Medium, evolves over time, attacks infrastructure.
- `NPCPhishingAttack.cs`, `NPCMimic.cs`, `BossNPC.cs` — other enemy types.

### Task System (`Scripts/NPCTask/`)

- `NPCTask.cs` — abstract base; states `Available → Queued → Executing → Completed`/`Interrupted`; has priority, role requirement, target. NPCs self-assign to the highest-priority unassigned task matching their role.
- Roles: DevOps, Boss, Dev, Intern, Enemy, SchematicalBot.
- Task types: `CodeTask`, `DeploymentTask`, `ResearchTask`, `InfrastructureTaskBase`, `BugConsumeTask`, `AttackTask`, `PhishingTask`, `RedirectTrafficTask`, `DeliverItemTask`, `TutorialMoveToTask`.

### Network Packets (`Scripts/NetworkPackets/`)

`NetworkPacket.cs` represents a request flowing through infrastructure; states `Running`/`Failed`/`Stolen`; tracks latency and visited nodes; moves visually between world objects. Packet types: `Purchase`, `Text`, `Image`, `MaliciousText` (SQLi), `BatchJob`, `PII`, `SQLInjection` — each maps to its own prefab.

### Stats System (`Scripts/Stats/`, `Scripts/Enum/StatType.cs`)

Modifier-based: stats = base value + additive/multiplicative modifier stack. Key global stats: `Money`, `TechDebt`, `Traffic`, `Difficulty`, `AttackPossibility`, `ItemDropChance`, `Global_DailyBudget`, `Global_DeploymentSpeed`/`Global_CodeSpeed`. NPC stats: MovementSpeed, HP, CoolDown, AttackDamage, Energy, CodeSpeed, CodeQuality, DevOpsSpeed, ResearchSpeed, XPSpeed, ModifierSlots. Infrastructure stats: MaxLoad, LoadRecoveryRate, BuildTime, LoadPerPacket, DailyCost, PacketCost, MaxSize, InputValidation, LatencyStartsAtLoad.

### Releases (`Scripts/Release/ReleaseBase.cs`)

Pipeline: `InDevelopment → InTesting → InReview → DeploymentReady → DeploymentInProgress → DeploymentRewardReady → DeploymentCompleted` (or `Failed`). Each release has version, quality score (affected by code quality/tech debt), attached bugs, reward modifier, rarity tier. Deployed to `ApplicationServer` instances via `DeploymentTask`.

### Events (`Scripts/Events/`)

Random Play-phase spawners whose probability scales with tech debt, low release quality, low input validation, and accumulated attack possibility: `SpawnBugEvent`, `SpawnXSSEvent`, `SpawnPhishingAttackEvent`, `SpawnSQLInjectionEvent`, `SpawnDDoSEvent`.

### Technology Tree (`Scripts/Technology.cs`)

States: `MetaLocked → Locked → Researching → Unlocked`. Researched by `NPCDevOps` at the Whiteboard; each tech has research time, dependency-based unlock conditions, and a direction in the tech-tree UI. Unlocks new infrastructure types/capabilities.

### Rewards (`Scripts/Rewards/`)

`RewardBase` abstract, with `StatBaseValueReward`, `StatModifierReward`, `StakeHolderReward`, `MetaStatBaseValueReward` (persistent), `LeveledRewardBase`, `SpecialCallbackReward`.

### Stakeholders (`Scripts/Stakeholders/Stakeholder.cs`)

C-suite (CEO/CTO/CISO/CFO/CMO) driving narrative dialog choices; leveled unlock tiers (e.g. CISO: Junior Security Officer → Security Consultant → CISO); unlocked via prestige points; all report to CEO; gate which levels/missions are available.

### Meta-Progression (`Scripts/Meta/`)

- `MetaProgressData.cs` — persistent save data (completed runs, prestige points, game stage, allocations, claimed rewards, meta-stats).
- `MetaGameManager.cs` — static save/load, prestige allocation, applies meta-rewards at run start.
- `MetaPrestigePointAllocatable.cs` — items that accept prestige-point investment.
- `MetaStat.cs` — cumulative cross-run stats.

Flow: complete a run → earn prestige points → allocate into org chart/technologies/bonuses → next run starts with bonuses applied.

### UI (`Scripts/UI/`)

Programmatic, data-driven panel framework:
- `UIPanel` — base class for all panels; manages a list of `UIPanelLine`; `AddButton`/`AddLine<T>` build content into a `scrollContent` Transform. Call `CleanUp()` then rebuild via `AddLine<T>()` (typically in `OnEnable`/`Refresh`), then `MarkUpdated()` to trigger Unity's `LayoutRebuilder`.
- `UIPanelLine` — one horizontal row; can hold multiple `UIPanelLineSection`s and nested child lines (tree views).
- `UIPanelLineSection` — individual element (Text/Image/Button/ProgressBar).
- `UIMapPanel.cs` — abstract base (already extracted from `UITechTreePanel.cs`) sharing tilemap/pan/zoom/procedural-layout logic across Tech Tree, Org Chart, Meta Unlock Map, and Product Road Map, driven by the generic `iUIMapNode` interface + `MapNodeState`/`MapNodeDirection` enums (`interfaces.cs`). `UITechTreePanel.cs`, `UI/MetaUnlock/UIMetaUnlockMapTabBase.cs` and its tab subclasses (`UIMetaUnlockTechnologyTab`, `UIMetaUnlockBonusesTab`, `UIMetaUnlockOrgChartTab`) are concrete `UIMapPanel` implementations.

Major panels include `UIMainMenu`, `UITopBarPanel` (HUD), `UIProductRoadMap`, `UIMapPanel`, `UIVictoryConditionListPanel`, `UINPCDetailPanel`, `UIWorldObjectDetailPanel`, `UIMultiSelectPanel` (roguelite card-pick), `UIOrgChartPanel`, `UIMetaUnlockMapPanel`, `UISaveSlotListPanel`/`UISaveSlotDetailPanel`, `UIRewardPanel`, `UITimeControlPanel`, `UIPauseMenu`, `UIDebugPanel`, `UISummaryPhasePanel`, `UIRunSetupPanel`, `UIChallengeSelectPanel`/`UIMetaChallengesPanel`, `UIReleaseHistoryPanel`, `UIGlobalStatsPanel`, `UITechTreePanel`.

### Other Systems

- `Scripts/Items/` — power-ups (`NukeItem`, `FreezeTimeItem`, `EnergyDrinkItem`).
- `Scripts/Effects/` — temporary timed effects on game state.
- `Scripts/Rarity.cs` — rarity tiers for rewards/modifiers.
- `Scripts/Util/Analytics/` — Unity Analytics events (DaySummary, PrestigePointAllocation, RewardInteraction).
- `Scripts/Util/SteamManager.cs` — Steamworks SDK integration, conditionally compiled (Standalone builds only).
- `Scripts/EnvGraphic/` — visual effects (level-up animation, evolve effect).
- `Scripts/Tutorial/` — `TutorialManager.cs`, `TutorialStep.cs`, `TutorialProductRoadMapLevel.cs`.

## Key Design Patterns

1. Singleton — `GameManager.Instance` accessed globally.
2. Observer/Events — C# events for state changes (`OnStatsChanged`, `OnInfrastructureStateChange`, `OnPhaseChange`, etc.).
3. Task Queue — priority-based task assignment; NPCs self-assign from a global queue.
4. Modifier Stack — stats are base value + list of named modifiers (additive).
5. Roguelite Card Pick — on NPC level-up, choose from a random reward pool.
6. Meta-Progression — prestige points invested between runs into a persistent skill tree.
7. Probability Weighting — events and packet types use weighted random selection.
