Tech Debt: The Video Game - MVP Scope Document (V1.0)

Project Goal: To rapidly prototype the core loop of a real-time management/defense game where the player manages a DevOps shop, hiring engineers (NPCDevOps) to maintain server infrastructure and battle "Tech Debt."

Target Platform: Unity (2D Isometric Tilemap)
Genre: Real-Time Management / Tower Defense-lite

1. Core Concept: Elevator Pitch (MVP)

The player manages a small server farm on an isometric grid. They must allocate worker NPCs ("NPCDevOps") to repair active server failures ("Fires") and maintain existing infrastructure (Tech Debt) while ensuring overall uptime to generate income.

2. Core Game Loop (The Day)

The game will run on a simple daily timer (e.g., 5 minutes = 1 Day).

Phase

Description

Key Action

A. Planning Phase (Paused)

Player views the map, purchases new servers, hires new Infrastructure NPCDevOps (S5), allocates NPCDevOps to permanent tasks, and hits "Start Day."

Buy/Sell Servers, Hire NPCDevOps, Assign NPCDevOps, Start Day button.

B. Day Phase (Real-Time)

Servers generate money. Random failure events ("Fires") spawn. The player manually redirects NPCDevOps to put out fires or complete tasks. NPCDevOps expend Engineering Hours (EH) to complete tasks.

Real-time observation, Click-to-Move orders.

C. Night Phase (Paused)

Day concludes. All resource gains (Money, Research) are tallied, and Engineering Hours used are summarized. Infrastructure NPCDevOps are automatically paid (Salary). Player engages with the Research Panel (placeholder).

End-of-Day report, Research Panel, Reset to Planning Phase.

3. Minimum Viable Systems (MVP V1.0)

These systems are non-negotiable for the prototype.

S0: Technical Foundation

Engine: Unity 2022+ (2D Isometric Project Settings).

Assets: Free Isometric Pixel Art Tiles (DevilsWork.shop, Kenney).

Map: Single, small, pre-built isometric Tilemap (Isometric Z as Y) representing the server room floor and pathways.

Sorting: Correct Z-as-Y sorting must be implemented for sprites to overlap correctly.

S1: NPC Management (The NPCDevOps - Infrastructure Team)

Entity: A single "NPCDevOps" NPC prefab using the 8-directional pixel art character asset.

Team Size Cap: The maximum number of controllable Infrastructure NPCDevOps is tied to the number of days survived (e.g., Max NPCDevOps = 1 + floor(Days Survived / 7)).

Movement: Simple A* pathfinding (using Unity NavMesh or a simple grid script) that allows the NPCDevOps to pathfind from their current location to a clicked destination (a Server or a Task).

Tasks: Debs have an internal state machine (Idle, Moving, Working). Tasks are managed by priority:

High Priority: Putting out a Fire / Toggling Server State (Manual Player assignment).

Low Priority: Repairing Tech Debt / Research (Automatic or Planned assignment).

Task Switching Penalty: If the player interrupts an NPCDevOps currently working on a Low Priority task to assign a High Priority task (manual override), the NPCDevOps incurs a fixed time delay (e.g., 1 second) before moving, simulating context switching/re-prioritization overhead. The penalty must be visible via UI text over the NPCDevOps.

Interactions: When an NPCDevOps reaches a target Server:

Fire: Expend EH over a fixed duration (e.g., 5 seconds of work), then destroy the Fire object.

Toggle: Expend EH over a short duration, then flip the server state (ON to OFF or vice-versa).

Tech Debt Maintenance: Continuously expend EH per second to prevent Tech Debt accumulation.

S2: Economy and Resources

Currency & Resources: The game tracks three critical resources:

Money (M): Earned from active servers. Used to purchase new infrastructure and hire Debs.

Engineering Hours (EH): The primary resource used to complete all tasks (Fire resolution, Tech Debt maintenance, Research, Toggling). Each Deb starts the day with a fixed pool of EH.

Research Points (RP): Generated when a Deb is assigned to a Research task and is not interrupted by higher-priority work. Used to unlock upgrades (V2.0+).

Income (M): Servers automatically generate M per second while in the ON state (simulating e-commerce sales).

Hours Usage (EH): EH is consumed only when a Deb is actively working on a task (not moving or idle). Debs stop working if EH runs out before the end of the day.

Expenses: Debs automatically deduct M at the end of the day (Salary).

Failure Cost: A server on fire stops generating income until the Fire is resolved.

S3: Infrastructure (Servers)

Server Object: A prefab using a pixel art block/rack placeholder.

States:

OFF: Costs M to purchase (static price). Generates 0 income. Cannot accrue Tech Debt.

ON: Generates income. Accrues Tech Debt. Can fail (spawn Fire).

Toggle Mechanism: Player clicks server; NPCDevOps runs to it. On arrival, NPCDevOps flips the state (ON to OFF or vice-versa).

Tech Debt: Each ON server has a visible Tech Debt counter that increases over time. This counter is converted into an increasing chance of a fire event when it hits certain thresholds. A dedicated NPCDevOps assigned to Maintenance expends EH (S2) to slowly reduce this counter.

S4: Events and Hazards (Background Teams)

Background Teams: The company has two passive, non-controllable teams: Developers and Interns.

Team Count: The number of Developers and Interns grows passively with the game day, separate from the player's controllable team. (e.g., +1 Dev and +2 Interns every 5 days).

Risk Factor: The total number of Developers and Interns directly increases the baseline probability or accumulation rate of Tech Debt and Fire Events on active servers. (More people = More Complexity = More Problems).

Visual Placeholder: A static "Bullpen" area on the map will display the current count of developers and interns (or a visual cluster of non-controllable sprites).

S5: Hiring Mechanic (MVP)

Hiring Pool: In the Planning Phase, the player is presented with a small pool of available Deb candidates.

Decision Metrics: For each candidate, the player sees:

Daily Salary (M): The cost deducted at the end of the day.

Skill Set (Placeholder): For the MVP, this is a single, static value (e.g., "Efficiency: 1.0"), but the structure for future skills (V2.0+) must be present.

Action: The player can hire a candidate if they have enough money and are below the Team Size Cap (S1). Once hired, the NPCDevOps is added to the controllable team.

4. Future Scope (V2.0+)

These features are out of scope for the MVP prototype but are vital for the full game.

Skill-Based Efficiency (EH): NPCDevOps' task speed (EH effectiveness) will be based on their skill (Junior/Senior).

Tech Tree Implementation: Implement the research mechanic (e.g., NPCDevOps assigned to "Research Station" generate Research Points). Unlockable nodes include:

Auto-scaling: Servers toggle state instantly without a Deb running to them.

CI/CD Pipeline: Permanently reduces Tech Debt accumulation globally.

Advanced Hazards: Introducing different types of events beyond just fire (e.g., DDoS attacks requiring a security-focused NPCDevOps, Database corruption requiring a special repair tool).

NPC Types: Differentiating between Junior Devs (cheap, slow, low skill) and Senior Engineers (expensive, fast, higher skill).

Larger Map/Unlocks: Expanding the playable area through purchasing "datacenter wings" or cloud capacity.

5. MVP Completion Criteria

The prototype is complete when the following can be successfully demonstrated:

A player can view the max Infrastructure NPCDevOps Team Size based on the current game day (S1).

In the Planning Phase, a player can view candidates' Salaries and Skills and hire an NPCDevOps up to the team cap (S5).

The Day Phase starts and servers generate money. Engineering Hours are visible and actively consumed during work.

The player can see the Bullpen count of non-controllable Developers and Interns (S4).

A Task Switching Penalty is displayed and enforced when interrupting an NPCDevOps' work.

The daily loop correctly transitions, summarizes resource usage, pays the Debs, and resets for the next day.quick