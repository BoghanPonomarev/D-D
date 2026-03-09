# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SliceDungeon — a turn-based combat prototype inspired by Slice & Dice (skill pool, random hand) with Darkest Dungeon's visual presentation (two teams of 4 facing each other). Built in Unity 6 (6000.1.11f1).

## Unity Workflow

No CLI build/test pipeline — everything runs through the Unity Editor.

- **Open project:** Unity Hub → open this directory with Unity 6000.1.11f1
- **Run tests:** Window → General → Test Runner
- **Build:** File → Build Settings

## Architecture

### Three Core Principles

1. **Battle logic is Unity-independent** — `Battle/Core/` and `Battle/Services/` are plain C# with no MonoBehaviour/Transform/GameObject. The simulation runs and tests without a Unity scene.

2. **Definitions are separate from runtime state** — ScriptableObjects hold static definitions (base HP, skill pool, icons). Plain C# objects hold runtime state (current HP, active statuses). Never mutate a ScriptableObject during battle.

3. **Actions and Events pipeline** — units never affect each other via direct calls:
   ```
   Player/AI creates BattleAction → BattleResolver resolves it → returns List<BattleEvent> → Presentation plays back events
   ```
   The presentation layer never contains battle rules.

### Script Structure (`Assets/Scripts/`)

```
Battle/
├── Core/          # plain C#: BattleState, TeamState, UnitState, BattleAction, BattleEvent, BattleFlowController, BattleResolver
├── Data/          # ScriptableObjects: UnitDefinition, SkillDefinition, EffectDefinition, EncounterDefinition
├── Effects/       # EffectDefinition (abstract SO), DamageEffect, HealEffect
├── Services/      # SkillRollService, TargetingService, InitiativeService, WinConditionService
├── AI/            # BattleAI — picks skill/target for enemy units
└── Bootstrap/     # BattleFactory — creates BattleState from EncounterDefinition
Presentation/      # MonoBehaviours only: BattleController, UnitView, FormationLayout, ActionLog, BattleDebugRunner
```

### Game Loop

```
BattleFactory.Create(EncounterDefinition) → BattleState
BattleFlowController.StartBattle(state) → InitiativeService builds turn queue by Speed
LOOP while WinConditionService.Check() == Ongoing:
  SkillRollService.Roll(unit, count: 3) → unit.CurrentSkills
  if hero: wait for player BattleAction via Presentation
  else: BattleAI.DecideAction(unit, state)
  events = BattleResolver.Resolve(action, state) → emit all events
```

### Key Technologies
- **Rendering:** URP with 2D renderer — use URP-compatible shaders/materials only
- **Input:** New Input System (`InputSystem_Actions.inputactions`) — never use legacy `Input` class
- **UI:** UGUI + TextMesh Pro
- **Animation:** 2D Animation + Timeline

### Unity-MCP
`com.ivanmurzak.unity.mcp` (v0.51.4) bridges Claude directly to the Unity Editor via MCP. Setup: `Assets/com.IvanMurzak/AI Game Dev Installer/README.md`.

### Project Settings
- Desktop: 1920×1080 | Web: 960×600 | Landscape | Linear color space

## Implementation Order

Phase 1 files must be created strictly in this order (each must compile before proceeding):

1. `Battle/Core/BattleEvent.cs`
2. `Battle/Core/BattleAction.cs`
3. `Battle/Core/UnitState.cs`
4. `Battle/Core/TeamState.cs`
5. `Battle/Core/BattleState.cs`
6. `Battle/Effects/EffectDefinition.cs`
7. `Battle/Effects/DamageEffect.cs`
8. `Battle/Effects/HealEffect.cs`
9. `Battle/Data/SkillDefinition.cs`
10. `Battle/Data/UnitDefinition.cs`
11. `Battle/Data/EncounterDefinition.cs`
12. `Battle/Core/BattleResolver.cs`
13. `Battle/Services/SkillRollService.cs`
14. `Battle/Services/TargetingService.cs`
15. `Battle/Services/InitiativeService.cs`
16. `Battle/Services/WinConditionService.cs`
17. `Battle/AI/BattleAI.cs`
18. `Battle/Core/BattleFlowController.cs`
19. `Battle/Bootstrap/BattleFactory.cs`
20. `Presentation/BattleDebugRunner.cs` ← verify simulation works before Phase 2

Phases 2–3 (visuals, player input) only after Phase 1 simulation is verified.

## Out of Scope

Positional skill restrictions, duration-based status effects, animations, meta-progression, procedural generation.

## Conventions
- All game scripts go in `Assets/Scripts/`
- No comments in code — use self-documenting names only