# SliceDungeon — Full Implementation Guide

Each iteration below is one Claude Code session.
Copy the prompt block, paste it into the terminal, verify the milestone before moving on.

---

# Phase 1 — Core Logic (plain C#, no Unity scene)

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

# Phase 2 — Formation and Visuals

## Iteration 6 — Formation layout

**Prompt:**
```
Create the formation layout for the battle scene.
Read README.md before starting.

File to create:
- Presentation/FormationLayout.cs

Requirements:
- 4 slots on the left side of the screen for heroes (positions 0-3, front to back)
- 4 slots on the right side for enemies (positions 0-3, front to back)
- slots are Transform references assigned in the Inspector
- expose: PlaceUnit(UnitState state, GameObject prefab), GetSlotPosition(int index, bool isHero)
- no battle logic inside this class
```

✅ **Milestone:** 8 empty slots are visible in the scene arranged in two lines facing each other.

---

## Iteration 7 — UnitView

**Prompt:**
```
Create UnitView.cs for the visual representation of a single unit.
Read README.md before starting.

File: Presentation/UnitView.cs

Requirements:
- MonoBehaviour attached to each unit's GameObject in the scene
- displays: unit name (TextMeshPro label), HP bar (UI Slider or simple scale), sprite (SpriteRenderer)
- exposes:
  - Initialize(UnitState state)
  - UpdateHp(int current, int max)
  - PlayDeathAnimation()
- death animation: simple fade out or scale to zero using a coroutine
- no battle logic inside this class
```

✅ **Milestone:** units appear in the scene with visible names and HP bars.

---

## Iteration 8 — BattleController (auto mode)

**Prompt:**
```
Create BattleController.cs connecting core battle logic to the presentation layer.
Read README.md before starting.

File: Presentation/BattleController.cs

Requirements:
- MonoBehaviour, entry point for the battle scene
- on Start: use BattleFactory.Create() then BattleFlowController.StartBattle()
- use FormationLayout to place UnitViews into correct slots on scene start
- subscribe to BattleFlowController.OnEvent and route each event:
  - DamageDealt  → target UnitView.UpdateHp()
  - HealApplied  → target UnitView.UpdateHp()
  - UnitDied     → target UnitView.PlayDeathAnimation()
  - BattleEnded  → show a win/lose text label on screen
- for now all units are AI-controlled (heroes use BattleAI too)
- no battle rules inside this class
```

✅ **Milestone:** press Play, the battle runs automatically, HP bars decrease, units die visually, winner is announced.

---

# Phase 3 — Player Input

## Iteration 9 — Skill buttons and target selection

**Prompt:**
```
Add player input to the battle. Read README.md before starting.

Files to create / modify:
- Presentation/SkillButtonPanel.cs  (new)
- Presentation/BattleController.cs  (modify)

SkillButtonPanel requirements:
- shows 3 buttons, one per skill in currentUnit.CurrentSkills
- each button displays the skill name and a short description
- clicking a button selects that skill and enters target-selection mode
- in target-selection mode enemy UnitViews become clickable and visually highlighted
- clicking a target calls BattleController.SubmitAction(skill, target)
- panel is hidden when it is not the player's turn

BattleController changes:
- when TurnStarted fires for a hero: show SkillButtonPanel, pause AI processing
- when TurnStarted fires for an enemy: hide SkillButtonPanel, let BattleAI act immediately
- SubmitAction(skill, target): build a BattleAction and call BattleFlowController.SubmitPlayerAction()
```

✅ **Milestone:** skill buttons appear on hero turns, clicking a skill then a target resolves the action. Enemies respond automatically on their turns.

---

# Phase 4 — Content and Polish

## Iteration 10 — Action log

**Prompt:**
```
Create ActionLog.cs as a scrollable UI text log.
Read README.md before starting.

File: Presentation/ActionLog.cs

Requirements:
- ScrollRect with TextMeshPro entries
- subscribes to BattleFlowController.OnEvent
- formats each event as a readable line:
  - DamageDealt  → "{source} hits {target} for {value} damage"
  - HealApplied  → "{source} heals {target} for {value} HP"
  - UnitDied     → "{target} has died"
  - SkillsRolled → "{unit} draws: {skill1}, {skill2}, {skill3}"
  - BattleEnded  → "=== VICTORY ===" or "=== DEFEAT ==="
- auto-scrolls to the latest entry after each addition
- keeps a maximum of 50 entries, removes the oldest when exceeded
```

✅ **Milestone:** a live scrollable log in the corner of the screen shows every battle event in plain English.

---

## Iteration 11 — More content

**Prompt:**
```
Create additional ScriptableObject content assets and wire up a proper EncounterDefinition.
Read README.md before starting.

Create the following assets:
Skills:
- Strike         (single enemy, 10 damage)
- Heavy Strike   (single enemy, 18 damage)
- Quick Slash    (single enemy, 7 damage)
- Heal           (single ally, restore 12 HP)
- Cleave         (all enemies, 6 damage)

Unit definitions:
- Warrior    (HP 60, Speed 8,  skills: Strike, Heavy Strike, Cleave)
- Cleric     (HP 45, Speed 6,  skills: Strike, Heal, Heal)
- Goblin     (HP 30, Speed 10, skills: Strike, Quick Slash)
- Orc Brute  (HP 55, Speed 4,  skills: Heavy Strike, Heavy Strike, Strike)

Encounter definition:
- Heroes:  Warrior (slot 0), Cleric (slot 1)
- Enemies: Goblin (slot 0), Orc Brute (slot 1)
```

✅ **Milestone:** a complete playable encounter with meaningful unit variety and skill choices.

---

## If something breaks between iterations

Paste this into Claude Code:
```
The project has a compile error. Here is the error: [paste error here].
Fix it without changing the architecture described in README.md.
```
