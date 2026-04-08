# TechDebt Project Context

This project is a Unity-based game that simulates managing cloud infrastructure while dealing with technical debt, network traffic, and software releases.

## Core Systems

### GameManager (`TechDebt/Assets/Scripts/GameManager.cs`)
The central hub of the game. It manages:
- **Game State:** Main Menu and Playing states.
- **Infrastructure:** Tracks all active infrastructure instances.
- **Packets:** Manages network packet generation and destruction.
- **Tasks:** Centralized task queue for NPCs.
- **Stats:** Global stats like Money, Traffic, and Tech Debt.
- **Events:** Random game events (e.g., bug spawns, DDoS attacks).

### GameLoopManager (`TechDebt/Assets/Scripts/GameLoopManager.cs`)
Manages the phases of the game (e.g., Play, Plan phases) and day/night cycles.

## Infrastructure System

### Base Classes
- **WorldObjectBase:** Base class for all physical objects in the game world.
- **InfrastructureInstance:** Inherits from `WorldObjectBase`. Represents functional cloud infrastructure (Servers, Databases, etc.).
- **InfrastructureData:** ScriptableObject-based data defining instance-specific settings (ID, Prefab, Grid Position).

### Infrastructure Types (`TechDebt/Assets/Scripts/WorldObjects/Types/`)
Infrastructure behavior is defined by `WorldObjectType` derivatives (e.g., `ApplicationServerWOType`, `InternetPipeWOType`). They specify:
- Costs (Build/Daily).
- Unlock conditions (Technologies).
- Network connections (Routing rules).
- Packet load handling.

### Key Infrastructure
- **InternetPipe:** Entry point for all network traffic.
- **ApplicationServer:** Processes various packet types; can be upgraded and receives releases.
- **Database/Redis:** Storage components for specific packet types.
- **ALB (Application Load Balancer):** Distributes traffic.
- **Queue/Worker:** Handles background jobs.

## Networking & Packets

### NetworkPacket (`TechDebt/Assets/Scripts/NetworkPackets/`)
Represents data moving through the system. Packets have types (Text, Image, PII, Malicious) and can be delayed or fail based on infrastructure load.

### Routing
Infrastructure instances route packets based on their `WorldObjectType` configuration. Each type defines where specific packet types should be sent next.

## NPC & Task System

### NPC Types (`TechDebt/Assets/Scripts/NPCs/`)
- **NPCBase:** Base class for all NPCs with a state machine (Idle, ExecutingTask, etc.) and pathfinding.
- **NPCDevOps:** The primary "worker" NPC. They have stats (CodeSpeed, Quality) and can level up to gain traits.
- **NPCBug / NPCXSS:** "Enemy" NPCs that represent technical debt or security threats.

### Tasks (`TechDebt/Assets/Scripts/NPCTask/`)
Work is distributed via `NPCTask` objects.
- **BuildTask:** Constructing new infrastructure.
- **CodeTask:** Developing features/releases.
- **DeploymentTask:** Deploying new versions to servers.
- **FixFrozenTask:** Repairing overloaded/failed infrastructure.

## Progression & Stats

### Stats System (`TechDebt/Assets/Scripts/Stats/`)
Uses `StatsCollection` to manage modifiable values. Common stats include `Money`, `Traffic`, `TechDebt`, and NPC-specific speeds.

### Technology (`TechDebt/Assets/Scripts/Technology.cs`)
A research-based progression system. Unlocking technologies allows building new infrastructure or improving existing ones.

## Release System (`TechDebt/Assets/Scripts/Release/`)
Manages the software lifecycle. Players develop releases via `CodeTask` and deploy them via `DeploymentTask`. Releases have quality levels and provide rewards/buffs upon completion.

## Project Structure
- `TechDebt/Assets/Scripts/WorldObjects/`: Infrastructure implementation.
- `TechDebt/Assets/Scripts/NPCs/`: NPC behavior and types.
- `TechDebt/Assets/Scripts/NPCTask/`: Task definitions.
- `TechDebt/Assets/Scripts/NetworkPackets/`: Packet logic.
- `TechDebt/Assets/Scripts/UI/`: UI panels and HUD components.
- `TechDebt/Assets/Scripts/Stats/`: Stat management system.

## Coding Conventions
- **No `var` keyword:** Always use explicit data types instead of `var` when declaring variables.
