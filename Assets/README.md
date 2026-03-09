# SliceDungeon — Combat Prototype (Unity)

A turn-based combat system inspired by **Slice & Dice** (skill pool, random hand)
with the visual presentation of **Darkest Dungeon** (two teams standing side by side facing each other).

---

## Gameplay Rules (MVP scope)

- 2 teams of 4 units each, standing in a line facing each other
- Turn order determined by Speed (descending)
- At the start of its turn, each unit receives **3 random skills** from its pool
- The unit picks 1 skill and a target → skill resolves → next turn
- Battle continues until one team is fully eliminated
- **No positional mechanics**
- **No dice system**Yes

---

## Three Core Architecture Principles

### Principle 1 — Battle logic is independent from Unity
All core classes (`Battle/Core/`, `Battle/Services/`) are plain C# with no `MonoBehaviour`, `Transform`, or `GameObject`.
The entire combat simulation must be runnable and testable without a Unity scene.

### Principle 2 — Definitions are separate from runtime state

| Definition (ScriptableObject) | State (runtime, plain C#) |
|-------------------------------|---------------------------|
| base HP, Speed, skill pool    | current HP, alive/dead    |
| name, icon, prefab reference  | active statuses           |
| effect description            | skills dealt this turn    |

Never mutate a ScriptableObject during a battle.

### Principle 3 — Actions and Events pipeline (most important)
Units never damage each other through direct method calls. Instead:

```
Player / AI creates a BattleAction
        ↓
BattleResolver resolves it
        ↓
Returns List<BattleEvent>
        ↓
Presentation layer plays back the events
```

The presentation layer **never contains battle rules**.

---

## Project Structure

```
Assets/
└── Scripts/
    ├── Battle/
    │   ├── Core/                        # plain C#, no Unity dependencies
    │   │   ├── BattleState.cs           # root object for the entire battle state
    │   │   ├── TeamState.cs             # state of one team
    │   │   ├── UnitState.cs             # state of one unit (HP, statuses, skills)
    │   │   ├── BattleAction.cs          # intent: who, which skill, which target
    │   │   ├── BattleEvent.cs           # fact: what happened (damage, heal, death)
    │   │   ├── BattleFlowController.cs  # orchestrator: turns, rounds, end of battle
    │   │   └── BattleResolver.cs        # Action → List<BattleEvent>
    │   │
    │   ├── Data/                        # ScriptableObject content definitions
    │   │   ├── UnitDefinition.cs
    │   │   ├── SkillDefinition.cs
    │   │   ├── EffectDefinition.cs
    │   │   └── EncounterDefinition.cs
    │   │
    │   ├── Effects/                     # skill effect implementations
    │   │   ├── EffectDefinition.cs      # abstract base ScriptableObject
    │   │   ├── DamageEffect.cs
    │   │   └── HealEffect.cs
    │   │
    │   ├── Services/                    # isolated logic services
    │   │   ├── SkillRollService.cs      # randomly deal skills from the pool
    │   │   ├── TargetingService.cs      # get valid targets for a skill
    │   │   ├── InitiativeService.cs     # build turn queue sorted by Speed
    │   │   └── WinConditionService.cs   # check for battle end
    │   │
    │   ├── AI/
    │   │   └── BattleAI.cs             # picks skill and target for enemy units
    │   │
    │   └── Bootstrap/
    │       └── BattleFactory.cs        # creates BattleState from EncounterDefinition
    │
    └── Presentation/                   # Unity visuals only
        ├── BattleController.cs         # MonoBehaviour, starts battle, listens to events
        ├── UnitView.cs                 # unit display: sprite, HP bar, name
        ├── FormationLayout.cs          # places UnitViews into scene slots
        ├── ActionLog.cs                # scrollable text log of battle events
        └── BattleDebugRunner.cs        # simulates a full battle via Debug.Log, no scene needed
```

---

## Key Classes

### `UnitState.cs`
```csharp
public class UnitState
{
    public string Id;
    public UnitDefinition Definition;   // read-only reference to SO

    public int CurrentHp;
    public bool IsAlive => CurrentHp > 0;
    public bool IsHero;
    public int SlotIndex;               // position in the formation (0–3)

    public List<SkillDefinition> SkillPool;
    public List<SkillDefinition> CurrentSkills;  // random hand for this turn
}
```

### `BattleAction.cs`
```csharp
public class BattleAction
{
    public UnitState Caster;
    public SkillDefinition Skill;
    public List<UnitState> Targets;
}
```

### `BattleEvent.cs`
```csharp
public enum BattleEventType
{
    DamageDealt,
    HealApplied,
    UnitDied,
    TurnStarted,
    TurnEnded,
    BattleEnded,
    SkillsRolled,
}

public class BattleEvent
{
    public BattleEventType Type;
    public UnitState Source;
    public UnitState Target;
    public int Value;           // damage / heal amount
    public string Description;  // human-readable log entry
}
```

### `SkillDefinition.cs` (ScriptableObject)
```csharp
[CreateAssetMenu(menuName = "SliceDungeon/Skill")]
public class SkillDefinition : ScriptableObject
{
    public string SkillName;
    public Sprite Icon;
    [TextArea] public string Description;
    public TargetType TargetType;   // SingleEnemy, SingleAlly, Self, AllEnemies, AllAllies
    public List<EffectDefinition> Effects;
}
```

### `EffectDefinition.cs` + `DamageEffect.cs`
```csharp
// EffectDefinition.cs — abstract ScriptableObject
public abstract class EffectDefinition : ScriptableObject
{
    public abstract List<BattleEvent> Apply(UnitState caster, UnitState target, BattleState battle);
}

// DamageEffect.cs
[CreateAssetMenu(menuName = "SliceDungeon/Effect/Damage")]
public class DamageEffect : EffectDefinition
{
    public int BaseDamage;

    public override List<BattleEvent> Apply(UnitState caster, UnitState target, BattleState battle)
    {
        target.CurrentHp -= BaseDamage;

        var events = new List<BattleEvent>
        {
            new BattleEvent { Type = BattleEventType.DamageDealt,
                              Source = caster, Target = target, Value = BaseDamage }
        };

        if (!target.IsAlive)
            events.Add(new BattleEvent { Type = BattleEventType.UnitDied, Target = target });

        return events;
    }
}
```

### `BattleResolver.cs`
```csharp
public class BattleResolver
{
    public List<BattleEvent> Resolve(BattleAction action, BattleState state)
    {
        var allEvents = new List<BattleEvent>();

        foreach (var target in action.Targets)
            foreach (var effect in action.Skill.Effects)
                allEvents.AddRange(effect.Apply(action.Caster, target, state));

        return allEvents;
    }
}
```

### `BattleFlowController.cs`
```csharp
public class BattleFlowController
{
    // Presentation subscribes here and updates visuals
    public event Action<BattleEvent> OnEvent;

    private BattleState _state;
    private BattleResolver _resolver;
    private SkillRollService _roller;
    private InitiativeService _initiative;
    private WinConditionService _winCheck;
    private BattleAI _ai;

    public void StartBattle(BattleState state) { ... }

    // Called from Presentation once the player picks a skill and target
    public void SubmitPlayerAction(BattleAction action) { ... }

    private void ProcessAITurn(UnitState unit) { ... }

    private void EmitEvents(List<BattleEvent> events)
    {
        foreach (var e in events) OnEvent?.Invoke(e);
    }
}
```

---

## Game Loop

```
BattleFactory.Create(EncounterDefinition)
  → BattleState (both teams initialized)

BattleFlowController.StartBattle(state)
  → InitiativeService builds turn queue sorted by Speed

LOOP while WinConditionService.Check() == Ongoing:

  currentUnit = next in queue
  Emit(TurnStarted)

  SkillRollService.Roll(currentUnit, count: 3)
    → currentUnit.CurrentSkills populated
  Emit(SkillsRolled)

  if currentUnit.IsHero:
    → wait for BattleAction from player (via Presentation)
  else:
    → BattleAI.DecideAction(currentUnit, state)

  events = BattleResolver.Resolve(action, state)
  Emit(all events)

  Emit(TurnEnded)

Emit(BattleEnded, winning team)
```

---

## Implementation Phases

### Phase 1 — Core (plain C# only, no Unity scene)
Goal: a full battle simulates and logs correctly without opening Unity.

- [ ] `BattleEvent.cs`, `BattleAction.cs`
- [ ] `UnitState.cs`, `TeamState.cs`, `BattleState.cs`
- [ ] `EffectDefinition.cs` (abstract SO)
- [ ] `DamageEffect.cs`, `HealEffect.cs`
- [ ] `SkillDefinition.cs`, `UnitDefinition.cs`, `EncounterDefinition.cs`
- [ ] `BattleResolver.cs`
- [ ] `SkillRollService.cs`, `TargetingService.cs`, `InitiativeService.cs`, `WinConditionService.cs`
- [ ] `BattleAI.cs` (random skill + random valid target)
- [ ] `BattleFlowController.cs`
- [ ] `BattleFactory.cs`
- [ ] **`BattleDebugRunner.cs` — run a full battle, verify the log looks correct**

### Phase 2 — Formation + Visuals
- [ ] `FormationLayout.cs` — 4 slots on the left (heroes), 4 on the right (enemies)
- [ ] `UnitView.cs` — sprite + HP bar + name label
- [ ] `BattleController.cs` — subscribes to `OnEvent`, updates Views

### Phase 3 — Player Input
- [ ] Skill buttons showing `CurrentSkills` of the active hero
- [ ] Target selection by clicking a unit
- [ ] `BattleController` passes `BattleAction` to `BattleFlowController`

### Phase 4 — Content
- [ ] 3–5 skills as ScriptableObject assets
- [ ] 2 hero types, 2 enemy types
- [ ] `ActionLog.cs` — scrollable UI log

---

## Out of Scope for This Version

- Positional skill restrictions
- Duration-based status effects (DoT, Stun, Buff) — structure is extensible, not implemented yet
- Animations
- Meta-progression, inventory, relics
- Procedural generation

---

## File Creation Order for Claude Code

Create strictly in this order. Do not move to the next file until the previous one compiles.

**Phase 1 (all plain C#, no MonoBehaviour):**
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
20. `Presentation/BattleDebugRunner.cs` ← **stop here and verify the battle simulation works**

**Phases 2–3:** only after Phase 1 passes the debug simulation check.
