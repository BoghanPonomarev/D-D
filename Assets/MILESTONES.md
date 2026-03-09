# Phase 1 — Iterative Implementation Guide

Each iteration below is one Claude Code session.
Copy the prompt block, paste it into the terminal, verify the milestone before moving on.

---

## Iteration 1 — Data structures and events

**Prompt:**
```
Create the following plain C# files with no MonoBehaviour or Unity dependencies.
Follow the structure from README.md exactly.

Files to create:
- Battle/Core/BattleEvent.cs
- Battle/Core/BattleAction.cs
- Battle/Core/UnitState.cs
- Battle/Core/TeamState.cs
- Battle/Core/BattleState.cs

No logic yet, just data structures and enums.
```

✅ **Milestone:** project compiles, all classes are visible in the IDE.

---

## Iteration 2 — Effects and skill definitions

**Prompt:**
```
Create the effect and skill definition files as plain C# ScriptableObjects.
Follow README.md.

Files to create:
- Battle/Effects/EffectDefinition.cs  (abstract base ScriptableObject)
- Battle/Effects/DamageEffect.cs
- Battle/Effects/HealEffect.cs
- Battle/Data/SkillDefinition.cs
- Battle/Data/UnitDefinition.cs
- Battle/Data/EncounterDefinition.cs
```

✅ **Milestone:** you can create `.asset` files via Unity Editor (right-click → Create → SliceDungeon/...).

---

## Iteration 3 — Resolver and services

**Prompt:**
```
Create the battle logic services and resolver. Plain C#, no MonoBehaviour.
Follow README.md.

Files to create:
- Battle/Core/BattleResolver.cs
- Battle/Services/SkillRollService.cs
- Battle/Services/TargetingService.cs
- Battle/Services/InitiativeService.cs
- Battle/Services/WinConditionService.cs
```

✅ **Milestone:** you can manually construct a `BattleState` and call `BattleResolver.Resolve()` — it returns a list of `BattleEvent`.

---

## Iteration 4 — AI and flow controller

**Prompt:**
```
Create the AI and the main flow controller. Plain C#, no MonoBehaviour.
Follow README.md.

Files to create:
- Battle/AI/BattleAI.cs
- Battle/Core/BattleFlowController.cs
- Battle/Bootstrap/BattleFactory.cs

BattleAI should pick a random skill from CurrentSkills and a random valid target.
BattleFlowController should expose event Action<BattleEvent> OnEvent.
```

✅ **Milestone:** `BattleFlowController` exists, accepts a `BattleState`, and exposes the `OnEvent` callback.

---

## Iteration 5 — Debug runner and first full battle

**Prompt:**
```
Create BattleDebugRunner.cs as a MonoBehaviour that:
- creates a hardcoded EncounterDefinition with 2 heroes and 2 enemies
- each unit has 2-3 simple skills (damage only is fine for now)
- runs a full automated battle (all units are AI-controlled)
- logs every BattleEvent to Debug.Log
- declares the winner at the end

File: Presentation/BattleDebugRunner.cs

Also create the minimum required ScriptableObject assets to make it run:
- at least 2 skills (e.g. Strike, Heavy Strike)
- at least 2 unit definitions (Hero, Goblin)
- 1 encounter definition
```

✅ **Main Phase 1 milestone:** press Play, see a full battle in the console from start to finish with a declared winner.
If the log looks correct — Phase 1 is done, move on to Phase 2.

---

## If something breaks between iterations

Paste this into Claude Code:
```
The project has a compile error. Here is the error: [paste error here].
Fix it without changing the architecture described in README.md.
```
