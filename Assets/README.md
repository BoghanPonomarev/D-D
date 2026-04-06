# Minami Lane-Style City Builder — Unity Project Specification

## Overview

Build a **single-street isometric city builder** inspired by *Minami Lane*. The player places buildings along one linear street, manages happiness, earns income, and attracts residents. The game uses **2D sprites** rendered in a pseudo-isometric perspective with an **orthographic camera**.

This document is a complete technical specification. Generate the full Unity project structure, all scripts, ScriptableObjects, prefab configurations, and editor tooling described below.

---

## Tech Stack

- **Engine**: Unity 2022.3 LTS (URP)
- **Rendering**: 2D Sprite Renderer with Ultimate Isometric Toolkit shader + per-frame depth sorting
- **Isometric Toolkit**: Ultimate Isometric Toolkit (free, already in project at `Assets/UltimateIsometricToolkit/`)
- **Grid System**: Terrain Grid System 2 by Kronnect (paid, already in project) — handles grid rendering, cell detection, hover/highlight, neighbor queries
- **UI**: Unity Canvas (Screen Space Camera mode — NOT Overlay, for correct render order over the game world)
- **Language**: C# (.NET Standard 2.1)
- **Architecture**: Manager-based with central Game Manager, State Machine, and ScriptableObject data

---

## Ultimate Isometric Toolkit — Integration Guide

The project uses the **Ultimate Isometric Toolkit** (UIT) by codebeans for all isometric rendering and depth sorting. The asset is already imported at `Assets/UltimateIsometricToolkit/`. Documentation: https://code-beans.github.io/Ultimate-Isometric-Toolkit-Docs-and-Issue-Tracker/

### What UIT Provides (do NOT reimplement)

- **Isometric shader** (`IsometricInstancingUnlit`) — rotates sprites toward the camera on the GPU so they appear 3D. This replaces any manual sorting-order hacks.
- **Per-frame depth sorting** — automatic, handled by the toolkit's rendering pipeline. No need for manual `sortingOrder` calculations.
- **Isometric Scene View** — toggle via `Tools > UIT > Toggle SceneView` (or Ctrl+G / Cmd+G). Aligns both Scene View and Main Camera to the correct isometric angle.
- **Projection selector** — `Tools > UIT > Projection`. Set to **Isometric** (default for the included sprites). If using Kenney or other dimetric sprites, switch to **Dimetric**.
- **Snapping Tool** — `Tools > Snapping Tool` (Ctrl+L / Cmd+L). Snaps objects to grid increments during level design. Supports auto-snap.
- **3D Physics support** — uses Unity's built-in 3D Rigidbody + Collider components. Freeze rotation on Rigidbodies (`freezeRotation = true`).

### How Our Project Uses UIT

**Every SpriteRenderer in the game** (buildings, residents, decorations, road segments) must use the UIT material:

1. Material: `Assets/UltimateIsometricToolkit/Materials/IsometricInstancingMaterial`
2. Shader: `IsometricInstancingUnlit` (set automatically by the material)
3. GPU Instancing: **enabled** (check `Enable GPU Instancing` on the material)

This means:
- Building prefabs: SpriteRenderer → material = `IsometricInstancingMaterial`
- Resident prefabs: SpriteRenderer → material = `IsometricInstancingMaterial`
- Road/environment prefabs: SpriteRenderer → material = `IsometricInstancingMaterial`
- Slot highlight sprites: SpriteRenderer → material = `IsometricInstancingMaterial`

**You may duplicate the material** for different tinting or settings, but always keep the `IsometricInstancingUnlit` shader.

### Demo Sprites from UIT

For the demo/prototype phase, use the **built-in sprites from UIT** located at `Assets/UltimateIsometricToolkit/Art/`. These are isometric-perspective sprites that work out-of-the-box with the toolkit's shader and projection settings.

**Sprite assignment for demo buildings:**

| Building | UIT Demo Sprite to Use | Notes |
|----------|----------------------|-------|
| Small House | Use any small building sprite from `Art/` | Tint light blue via SpriteRenderer.color |
| Apartment | Use any taller building sprite from `Art/` | Tint light blue |
| Café | Use a building sprite from `Art/` | Tint warm orange |
| Bakery | Use a building sprite from `Art/` | Tint warm orange |
| Bookshop | Use a building sprite from `Art/` | Tint yellow |
| Bar | Use a building sprite from `Art/` | Tint purple |
| Small Park | Use a tree/nature sprite from `Art/` | Tint green |
| Large Park | Use a larger nature sprite from `Art/` | Tint green |
| Shrine | Use a decorative building from `Art/` | Tint red |
| Convenience Store | Use a building sprite from `Art/` | Tint gray |
| Road segments | Use floor/ground tiles from `Art/` | No tint |
| Slot markers | Use floor tiles from `Art/` with alpha | Semi-transparent |

**If UIT does not include enough variety**, fall back to generating colored placeholder rectangles (see Placeholder Art Instructions section below). But prefer UIT sprites first — they have correct proportions and work with the shader.

### Pixels Per Unit for Custom Sprites

If adding custom sprites (e.g. from Kenney), calculate PPU so each tile covers 1×1 world units:
```
PPU = sprite_width_px / sqrt(2)
Example: 256px wide tile → 256 / 1.414 = 181.02 PPU
```
Set this in the sprite's Import Settings. The included UIT sprites already have correct PPU.

### Sorting with UIT — What Changes

Since UIT handles depth sorting per-frame via its shader, the sorting approach changes from the original spec:

**REMOVE from our code:**
- Manual `sortingOrder` assignments based on slot index
- Manual sorting layer assignments per building

**KEEP in our code:**
- `Order in Layer` on SpriteRenderer for layering floor vs. walls vs. decorations (floor = -10, buildings = 0, decorations on top = +10)
- Sorting Layers are still useful for broad categories (Background, Road, Buildings, Residents, UI_World) but depth within a layer is handled by UIT

**Sorting artifacts** are inevitable in 2.5D. Minimize overlap between sprites. Use `Order in Layer` to force floor tiles behind everything else. The UIT docs recommend reducing sprite overlap and being intentional about render order during level design.

### Scene Setup with UIT

When setting up the game scene:
1. Open the scene
2. `Tools > UIT > Toggle SceneView` to switch to isometric perspective
3. `Tools > UIT > Projection > Isometric` (confirm it matches the sprites)
4. Camera is automatically aligned by UIT — keep it orthographic
5. Use the Snapping Tool (`Ctrl+L`) when manually placing environment objects in the editor

### Physics for Clickable Buildings

UIT uses Unity's **3D physics** for colliders. This means:
- On any Rigidbody, set `freezeRotation = true` to prevent the shader rotation from interfering
- Buildings can have **BoxCollider** (3D) for future click/select detection (e.g., bulldoze mode)

**Cell/slot detection is handled by TGS2, not by raycasts.** TGS2 fires `OnCellEnter`/`OnCellExit` events and exposes `CellGetAtWorldPosition()`. We do NOT write raycast code for grid interaction.

---

## Global Architecture

The entire game is built around a **central Game Manager** that coordinates specialized managers. No manager knows about other managers directly — all communication goes through Game Manager. This is the target architecture:

```
Localization / Text Manager
        |
   Object Pools
        |
   Sound Manager  ←──────────────────────┐
        |                                 |
   UI Manager  ←────────────────────┐     |
        |                            |    |
   Asset Library ───────────────► Game Manager ◄── Data Manager
                                     ↕
                               State Machine
                            ┌──────┬────────┐
                           Init  Gameplay  GameOver
```

### Game Manager

Central coordination node. Does NOT contain game logic itself — only coordinates managers and drives the State Machine. Implemented as a MonoBehaviour on a dedicated GameObject marked with `DontDestroyOnLoad`. Holds references to all other managers and provides access to them.

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Manager references — assigned in inspector or created in Init state
    [SerializeField] private AssetLibrary assetLibrary;
    private ObjectPoolManager poolManager;
    private SoundManager soundManager;
    private UIManager uiManager;
    private DataManager dataManager;
    private LocalizationManager localizationManager;
    private TimeManager timeManager;

    // State machine
    private StateMachine stateMachine;

    // Global game state
    public int Money { get; set; }
    public int CurrentDay { get; set; }

    // Access points for managers (other systems call these, never managers directly)
    public AssetLibrary Assets => assetLibrary;
    public ObjectPoolManager Pools => poolManager;
    public SoundManager Sound => soundManager;
    public UIManager UI => uiManager;
    public DataManager Data => dataManager;
    public LocalizationManager Localization => localizationManager;
    public TimeManager Time => timeManager;
}
```

### State Machine

Game Manager owns a finite state machine. Each state is a separate class: `InitState`, `GameplayState`, `GameOverState`. The State Machine holds the current active state and switches it on command from Game Manager. States do NOT call each other — they only signal Game Manager that a transition is needed.

```csharp
public interface IGameState
{
    void Enter();
    void Update();
    void Exit();
}

public class StateMachine
{
    private IGameState currentState;

    public void ChangeState(IGameState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update() => currentState?.Update();
}
```

**InitState** — loads all data and prepares the scene before the player sees the game field. Creates and initializes all managers in the correct order.

**GameplayState** — main game loop. This is where the street, building placement, happiness/income simulation, residents, and time progression all live. BuildingPlacer, HandController, and all gameplay systems are activated in this state and deactivated on exit.

**GameOverState** — shows results, offers restart. Deactivates gameplay systems.

### Asset Library (ScriptableObject)

Storage for all game data — BuildingData ScriptableObjects, sprites, balance settings. Implemented as a ScriptableObject. Does NOT load resources at runtime — all data is assigned in the inspector ahead of time.

```csharp
[CreateAssetMenu(menuName = "MiniCity/AssetLibrary")]
public class AssetLibrary : ScriptableObject
{
    [Header("Buildings")]
    [SerializeField] private BuildingData[] allBuildings;
    [SerializeField] private SynergyTable synergyTable;

    [Header("Residents")]
    [SerializeField] private ResidentData[] residentVariants;

    [Header("Audio")]
    [SerializeField] private AudioClip placeBuildingSound;
    [SerializeField] private AudioClip removeBuildingSound;
    [SerializeField] private AudioClip dayEndSound;

    [Header("Prefabs")]
    [SerializeField] private GameObject handCardPrefab;
    [SerializeField] private GameObject residentPrefab;

    // Public read-only access
    public BuildingData[] AllBuildings => allBuildings;
    public SynergyTable Synergy => synergyTable;
    public BuildingData GetBuilding(string id) => ...;
}
```

**CRITICAL**: No string paths to Resources anywhere in the code. All references go through Asset Library only.

### Object Pool Manager

Reuses objects instead of creating/destroying them each time. Game Manager initializes pools during the Init state.

In this project, pools are needed for:
- **UI hand cards** (building selection cards in the player's hand)
- **Resident NPCs** (walking sprites)
- **Visual effects** (placement flash, highlight effects)

```csharp
public class ObjectPoolManager : MonoBehaviour
{
    public GameObject Get(string poolName);
    public void Return(string poolName, GameObject obj);
    public void CreatePool(string name, GameObject prefab, int initialSize);
}
```

### UI Manager

Controls all screens and panels. Receives commands from Game Manager — e.g. "show GameOver screen with this score". Does NOT make decisions — only displays what it's told.

Manages:
- HUD (top-left stats)
- Build Panel / Hand (bottom card hand)
- Time Controls (bottom-right)
- Tooltip System
- Day Summary Popup
- GameOver Screen

```csharp
public class UIManager : MonoBehaviour
{
    public void ShowHUD();
    public void HideHUD();
    public void UpdateMoney(int amount);
    public void UpdateHappiness(float percent);
    public void UpdateResidents(int count);
    public void ShowDaySummary(DaySummaryData data);
    public void ShowGameOver(GameOverData data);
    public void ShowBuildPanel(BuildingData[] available);
    public void HideBuildPanel();
}
```

### Sound Manager

Plays sounds on request from Game Manager or other systems. Gets AudioClip references through Asset Library. Does not decide when to play — waits for calls.

```csharp
public class SoundManager : MonoBehaviour
{
    public void PlaySFX(AudioClip clip);
    public void PlayMusic(AudioClip clip);
    public void StopMusic();
    public void SetVolume(float sfx, float music);
}
```

### Data Manager

Handles saving/loading player data — progress, settings, high scores. Uses PlayerPrefs for simple data or JSON serialization for complex data. Game Manager calls Data Manager at game start to load state and at end to save it.

```csharp
public class DataManager : MonoBehaviour
{
    public void Save(GameSaveData data);
    public GameSaveData Load();
    public void ClearSave();
}
```

### Localization / Text Manager

Stores all game text strings keyed by language. Every text in the game — building name, tooltip, button label — comes ONLY through Text Manager, never hardcoded in components.

```csharp
public class LocalizationManager : MonoBehaviour
{
    public string Get(string key); // e.g. Get("building.cafe.name") → "Café"
    public void SetLanguage(string languageCode);
}
```

### Initialization Order

All managers are created during the Init state in this exact order:

1. **Asset Library** — first, because all others depend on data
2. **Object Pools** — right after, while no active objects exist
3. **Sound Manager** and **UI Manager** — in parallel, both depend only on Asset Library
4. **Data Manager** — loads saved state
5. **Localization Manager** — applies language
6. **Time Manager** — prepares the day/night cycle
7. **State Machine transitions to Gameplay**

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs              # Central coordinator, DontDestroyOnLoad
│   │   ├── StateMachine.cs             # FSM: holds current state, switches states
│   │   ├── States/
│   │   │   ├── IGameState.cs           # Interface: Enter, Update, Exit
│   │   │   ├── InitState.cs            # Creates managers, loads data
│   │   │   ├── GameplayState.cs        # Main loop: activates street & placement
│   │   │   └── GameOverState.cs        # Results screen, restart option
│   │   ├── AssetLibrary.cs             # SO: all game data references
│   │   ├── ObjectPoolManager.cs        # Generic object pooling
│   │   ├── SoundManager.cs             # SFX and music playback
│   │   ├── DataManager.cs              # Save/load player data
│   │   └── LocalizationManager.cs      # Text strings by language
│   ├── Street/
│   │   └── Street.cs                   # Wrapper over TGS2: occupancy dict, placement, neighbors
│   ├── Buildings/
│   │   ├── BuildingData.cs             # SO: stats, sprites, cost, category
│   │   ├── BuildingInstance.cs         # MonoBehaviour on placed building GameObjects
│   │   ├── BuildingPlacer.cs           # Placement logic: validates slot, places building
│   │   ├── BuildingRemover.cs          # Bulldoze / sell buildings
│   │   └── SynergyTable.cs            # SO: NxN category-to-category bonus matrix
│   ├── Cards/
│   │   ├── HandCard.cs                 # UI card: IBeginDrag, IDrag, IEndDrag
│   │   └── HandController.cs           # Spawns cards from pool, manages hand layout
│   ├── Simulation/
│   │   ├── HappinessSystem.cs          # Per-building & global happiness calculation
│   │   ├── IncomeSystem.cs             # Daily income from shops, modified by happiness
│   │   ├── ResidentManager.cs          # Spawns/despawns resident NPCs from pool
│   │   ├── TimeManager.cs              # Day/night cycle, speed controls
│   │   └── DayEndProcessor.cs          # End-of-day: income, happiness, events
│   ├── Residents/
│   │   ├── ResidentAI.cs               # Simple walk-along-street behavior
│   │   └── ResidentData.cs             # SO: appearance variants
│   ├── Camera/
│   │   └── IsometricCamera.cs          # Ortho camera, pan along street, zoom
│   └── UI/
│       ├── UIManager.cs                # Controls all screens, receives commands
│       ├── HUD.cs                      # Top-left stats display
│       ├── BuildPanel.cs               # Bottom bar: building card hand
│       ├── TimeControlsUI.cs           # Pause / 1x / 2x / 3x buttons
│       ├── TooltipSystem.cs            # Hover tooltip: building info
│       ├── DaySummaryPopup.cs          # End-of-day results
│       └── GameOverScreen.cs           # Final score and restart
├── Data/
│   ├── AssetLibrary.asset              # Main Asset Library SO instance
│   ├── Buildings/                      # BuildingData SO assets
│   │   ├── Residential_Small.asset
│   │   ├── Residential_Medium.asset
│   │   ├── Cafe.asset
│   │   ├── Bakery.asset
│   │   ├── Bookshop.asset
│   │   ├── Bar.asset
│   │   ├── Park_Small.asset
│   │   ├── Park_Large.asset
│   │   ├── Shrine.asset
│   │   └── Convenience_Store.asset
│   ├── SynergyTable.asset
│   └── Localization/
│       ├── en.json                     # English strings
│       └── ru.json                     # Russian strings
├── Prefabs/
│   ├── Buildings/                      # One prefab per building type
│   ├── Residents/                      # Resident NPC prefab with animator
│   ├── Cards/                          # HandCard UI prefab (for pool)
│   ├── Street/                         # Street slot marker, road segment
│   ├── Effects/                        # Placement flash VFX prefab (for pool)
│   └── UI/                             # UI screen prefabs
├── Sprites/                            # Placeholder sprites (generated colored rects)
│   ├── Buildings/
│   ├── Residents/
│   ├── Environment/                    # Trees, bushes, power lines, road
│   └── UI/
├── Scenes/
│   └── Game.unity                      # Single game scene
└── Settings/
    └── URP_Settings/                   # 2D Renderer, no post-processing needed
```

---

## Core Systems — Detailed Specifications

### 1. Street Grid (`Street.cs` + Terrain Grid System 2)

**Buildings can occupy 1, 2, or 3 cells along the street.** A small house takes 1 cell, a café takes 2, a large park takes 3. The `widthInCells` value is defined per building type in BuildingData. Height (depth into/out of screen) is always 1 cell — the street is a single row per side.

The grid is powered by **Terrain Grid System 2** (TGS2) by Kronnect. TGS2 handles grid rendering, mouse-over-cell detection, cell highlighting, and neighbor queries. Our `Street.cs` is a **wrapper** over TGS2 that adds multi-cell occupancy tracking, placement validation, and synergy neighbor lookup.

Documentation: https://kronnect.com/docs/terrain-grid/

#### Key Concept: Anchor Cell + Occupied Cells

Every placed building has:
- **Anchor cell** — the leftmost cell it occupies (lowest column index). This is the cell the player's cursor was hovering when they dropped the card. The building prefab is positioned at the center of all its occupied cells.
- **Occupied cells** — all cells the building covers: `[anchorColumn, anchorColumn + widthInCells - 1]` on the same row (side of street).

All occupied cells point to the same `BuildingInstance` in the occupancy dictionary. Only the anchor cell is used for iteration (to avoid counting a building multiple times in happiness/income loops).

#### TGS2 Inspector Settings

Configure the TerrainGridSystem component on the Street GameObject:

| Setting | Value | Why |
|---------|-------|-----|
| Topology | **Box** | Square cells for our street grid |
| Column Count | **10** | Slots along the street (can be adjusted for longer streets) |
| Row Count | **2** | Two sides of the road |
| Cells Flat | **true** | Flat surface, no terrain height |
| Cell Border Color | White, alpha ~0.3 | Subtle grid lines |
| Cell Border Thickness | 0.5–1.0 | Thin visible borders |
| Cell Highlight Color | Transparent (alpha = 0) | We control highlight ourselves via code |
| Show Cells | **true** | Grid lines visible during gameplay |

#### TGS2 Events We Subscribe To

From the Events API (https://kronnect.com/docs/terrain-grid/events/):

- `OnCellEnter(TerrainGridSystem tgs, int cellIndex)` — mouse enters a cell. Used during drag to highlight the full multi-cell footprint.
- `OnCellExit(TerrainGridSystem tgs, int cellIndex)` — mouse leaves a cell. Used to clear previous highlight.
- `OnCellClick(TerrainGridSystem tgs, int cellIndex, int buttonIndex)` — cell clicked. Useful for bulldoze mode.

#### TGS2 Cell API We Use

From the Cells API (https://kronnect.com/docs/terrain-grid/cells-api/):

- `CellGetPosition(cellIndex, worldSpace: true)` — get world position of a cell
- `CellSetColor(cellIndex, color)` — highlight cells green/red during drag
- `CellToggleRegionSurface(cellIndex, false)` — clear highlight
- `CellFlash(cellIndex, color, duration)` — visual feedback on successful placement (flash all occupied cells)
- `CellGetNeighbors(cellIndex, maxSteps)` — find neighboring cells for synergy
- `CellGetAtWorldPosition(position)` — convert world position to cell index
- `CellGetRow(cellIndex)` / `CellGetColumn(cellIndex)` — row = side of street (0 or 1), column = position along street (0..9)
- `CellGetIndex(row, column)` — get cell index from row + column
- `cellHighlightedIndex` — which cell is currently under the mouse

#### Street.cs Implementation

```csharp
public class Street : MonoBehaviour
{
    [SerializeField] private TerrainGridSystem tgs;
    [SerializeField] private int columnsPerSide = 10;
    
    // Game state: TGS cell index → building occupying it
    // Multiple cell indices can point to the same BuildingInstance
    private Dictionary<int, BuildingInstance> occupants = new();
    
    // Track which cells are anchor cells (to iterate buildings without duplicates)
    private HashSet<int> anchorCells = new();
    
    // Currently hovered cell during drag (-1 = none)
    public int HoveredCellIndex { get; private set; } = -1;
    
    // Cells currently highlighted (need to clear them on exit)
    private List<int> currentHighlightedCells = new();

    void Awake()
    {
        tgs.OnCellEnter += HandleCellEnter;
        tgs.OnCellExit += HandleCellExit;
    }

    // --- Multi-cell helpers ---

    /// Returns all cell indices that a building would occupy if placed at anchorCellIndex.
    /// Returns null if any cell is out of bounds.
    public List<int> GetFootprint(int anchorCellIndex, BuildingData data)
    {
        int row = tgs.CellGetRow(anchorCellIndex);
        int startCol = tgs.CellGetColumn(anchorCellIndex);
        
        var cells = new List<int>(data.widthInCells);
        for (int c = startCol; c < startCol + data.widthInCells; c++)
        {
            if (c >= columnsPerSide) return null; // out of bounds
            int idx = tgs.CellGetIndex(row, c);
            if (idx < 0) return null;
            cells.Add(idx);
        }
        return cells;
    }

    // --- Occupancy ---

    public bool IsOccupied(int cellIndex) => occupants.ContainsKey(cellIndex);

    public bool CanPlace(int anchorCellIndex, BuildingData data)
    {
        if (anchorCellIndex < 0) return false;
        
        var footprint = GetFootprint(anchorCellIndex, data);
        if (footprint == null) return false; // extends beyond street edge
        
        foreach (int idx in footprint)
        {
            if (IsOccupied(idx)) return false; // any cell in footprint already taken
        }
        
        if (!GameManager.Instance.CanAfford(data.purchaseCost)) return false;
        return true;
    }

    // --- Placement ---

    public void PlaceBuilding(int anchorCellIndex, BuildingData data)
    {
        var footprint = GetFootprint(anchorCellIndex, data);
        
        // Position = center of all occupied cells
        Vector3 centerPos = Vector3.zero;
        foreach (int idx in footprint)
            centerPos += tgs.CellGetPosition(idx, worldSpace: true);
        centerPos /= footprint.Count;
        
        var instance = GameManager.Instance.Pools.Get("buildings");
        instance.Init(data, anchorCellIndex, footprint);
        instance.transform.position = centerPos;
        
        // Mark ALL cells as occupied, pointing to same instance
        foreach (int idx in footprint)
            occupants[idx] = instance;
        anchorCells.Add(anchorCellIndex);

        // Visual feedback — flash all cells
        tgs.CellFlash(footprint, Color.white, 0.3f);
    }

    public void RemoveBuilding(int anyCellIndex)
    {
        if (!occupants.TryGetValue(anyCellIndex, out var instance)) return;
        
        // Remove ALL cells this building occupies
        foreach (int idx in instance.OccupiedCells)
        {
            occupants.Remove(idx);
            tgs.CellToggleRegionSurface(idx, false);
        }
        anchorCells.Remove(instance.AnchorCellIndex);
        
        GameManager.Instance.Pools.Return("buildings", instance.gameObject);
    }

    // --- Neighbors (for synergy) ---

    public List<BuildingInstance> GetNeighbors(BuildingInstance building, int radius)
    {
        // Collect neighbors from ALL cells of this building's footprint
        var neighborSet = new HashSet<BuildingInstance>();
        
        foreach (int cellIdx in building.OccupiedCells)
        {
            var nearby = tgs.CellGetNeighbors(cellIdx, radius);
            foreach (int nearIdx in nearby)
            {
                if (occupants.TryGetValue(nearIdx, out var neighbor) 
                    && neighbor != building) // don't count self
                {
                    neighborSet.Add(neighbor);
                }
            }
        }
        return neighborSet.ToList();
    }

    // --- Iterate all buildings (no duplicates) ---

    public IEnumerable<BuildingInstance> GetAllBuildings()
    {
        foreach (int anchor in anchorCells)
            yield return occupants[anchor];
    }

    // --- Hover highlight during drag ---

    void HandleCellEnter(TerrainGridSystem sender, int cellIndex)
    {
        HoveredCellIndex = cellIndex;
        
        if (BuildingPlacer.IsDragging)
            HighlightFootprint(cellIndex, BuildingPlacer.CurrentData);
    }

    void HandleCellExit(TerrainGridSystem sender, int cellIndex)
    {
        HoveredCellIndex = -1;
        ClearHighlight();
    }

    void HighlightFootprint(int anchorCellIndex, BuildingData data)
    {
        ClearHighlight(); // clear previous highlight
        
        var footprint = GetFootprint(anchorCellIndex, data);
        bool valid = CanPlace(anchorCellIndex, data);
        Color color = valid
            ? new Color(0, 1, 0, 0.35f)   // green
            : new Color(1, 0, 0, 0.35f);  // red
        
        if (footprint != null)
        {
            foreach (int idx in footprint)
                tgs.CellSetColor(idx, color);
            currentHighlightedCells.AddRange(footprint);
        }
        else
        {
            // Building extends beyond edge — highlight just the anchor cell red
            tgs.CellSetColor(anchorCellIndex, new Color(1, 0, 0, 0.35f));
            currentHighlightedCells.Add(anchorCellIndex);
        }
    }

    void ClearHighlight()
    {
        foreach (int idx in currentHighlightedCells)
            tgs.CellToggleRegionSurface(idx, false);
        currentHighlightedCells.Clear();
    }

    // --- Utility ---

    public int GetCellAtWorldPosition(Vector3 worldPos)
    {
        Cell cell = tgs.CellGetAtWorldPosition(worldPos);
        return cell != null ? tgs.CellGetIndex(cell) : -1;
    }

    public Vector3 GetCellWorldPosition(int cellIndex)
    {
        return tgs.CellGetPosition(cellIndex, worldSpace: true);
    }

    public int GetSide(int cellIndex) => tgs.CellGetRow(cellIndex);
    public int GetColumn(int cellIndex) => tgs.CellGetColumn(cellIndex);
}
```

#### BuildingInstance — Knows Its Footprint

```csharp
public class BuildingInstance : MonoBehaviour
{
    public BuildingData Data { get; private set; }
    public int AnchorCellIndex { get; private set; }
    public List<int> OccupiedCells { get; private set; }

    public void Init(BuildingData data, int anchorIndex, List<int> footprint)
    {
        Data = data;
        AnchorCellIndex = anchorIndex;
        OccupiedCells = new List<int>(footprint);
        
        // Apply visuals
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = data.buildingSprite;
        sr.color = data.tintColor;
    }
}
```

#### What TGS2 Replaces

With TGS2, we do NOT need to write:
- Grid rendering (cell borders, grid lines) — TGS2 draws them
- Mouse-to-cell detection / raycasting — TGS2 events handle it
- Cell highlight rendering — `CellSetColor` / `CellToggleRegionSurface`
- Neighbor search algorithm — `CellGetNeighbors(cellIndex, radius)`
- World position ↔ cell index conversion — `CellGetAtWorldPosition` / `CellGetPosition`
- Row/column lookup — `CellGetRow` / `CellGetColumn` / `CellGetIndex(row, col)`

What we still own:
- Multi-cell occupancy dictionary (`Dictionary<int, BuildingInstance>`)
- Anchor cell tracking (`HashSet<int>`)
- Footprint calculation (which cells a building covers based on `widthInCells`)
- Placement validation (all cells free + affordability)
- Multi-cell highlight logic during drag
- Synergy calculation (union of neighbors across entire footprint, deduplicated)
- Building prefab instantiation and pooling

### 2. Building Data (`BuildingData.cs` — ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "NewBuilding", menuName = "MiniCity/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Identity")]
    public string buildingId;            // Localization key: "building.cafe"
    public Sprite icon;                  // For build panel / hand card
    public Sprite buildingSprite;        // Main isometric sprite (from UIT Art/ or custom)
    public BuildingCategory category;

    [Header("Grid Size")]
    public int widthInCells = 1;         // How many cells along the street (1, 2, or 3)

    [Header("Cost")]
    public int purchaseCost;
    public int dailyUpkeep;

    [Header("Effects")]
    public int happinessBonus;           // Base happiness contribution
    public int effectRadius;             // How many slots this affects neighbors
    public int incomePerDay;             // Revenue generated

    [Header("Residents")]
    public int residentsProvided;        // Housing capacity (residential only)
    public int residentsRequired;        // Staff needed (shops only)

    [Header("Visuals")]
    public Color tintColor = Color.white; // Category tint applied via SpriteRenderer.color
    public float spriteYOffset;          // Vertical offset for tall buildings
    public GameObject prefab;            // Building prefab (SpriteRenderer MUST use IsometricInstancingMaterial)
}

public enum BuildingCategory
{
    Residential,
    Food,            // Cafe, Bakery, Restaurant
    Shop,            // Bookshop, Convenience Store
    Entertainment,   // Bar, Arcade
    Nature,          // Park, Garden
    Cultural,        // Shrine, Temple
    Service          // Post Office, Clinic
}
```

**NOTE**: `buildingId` is used as a key for LocalizationManager. Display name = `LocalizationManager.Get($"building.{buildingId}.name")`. Description = `LocalizationManager.Get($"building.{buildingId}.desc")`. No hardcoded display strings in BuildingData.

### 3. Synergy Table (`SynergyTable.cs`)

A ScriptableObject holding an NxN matrix of category-to-category bonuses:

```
                 Residential  Food  Shop  Entertainment  Nature  Cultural  Service
Residential          0         +3    +2       -3          +5      +3        +2
Food                 +3         0    +1       +2          +1       0         0
Shop                 +2        +1     0        0          +1       0        +1
Entertainment        -3        +2     0        0          -1       0         0
Nature               +5        +1    +1       -1           0      +3         0
Cultural             +3         0     0        0          +3       0         0
Service              +2         0    +1        0           0       0         0
```

The synergy bonus is applied per-building-pair within radius.

### 4. Happiness System (`HappinessSystem.cs`)

Recalculates whenever a building is placed or removed. Reports result to Game Manager, which forwards to UI Manager.

Iterates over all buildings using `Street.GetAllBuildings()` (which only yields anchor cells to avoid counting multi-cell buildings multiple times).

For each building, calls `Street.GetNeighbors(buildingInstance, radius)` which collects neighbors from ALL cells in the building's footprint and deduplicates them.

```
Per-building happiness = baseHappiness + Σ(synergy with each unique neighbor in radius)
Global happiness % = average of all per-building happiness values, clamped 0–100
```

### 5. Income System (`IncomeSystem.cs`)

Called by DayEndProcessor at end of day:
```
Daily income = Σ(building.incomePerDay * happinessMultiplier) - Σ(building.dailyUpkeep)
happinessMultiplier = 0.5 + (buildingHappiness / 100) * 1.0
  → at 0% happiness: 50% income
  → at 100% happiness: 150% income
```

Result is passed to Game Manager which updates `Money` and tells UI Manager to refresh.

### 6. Time System (`TimeManager.cs`)

- One "day" = 60 real seconds at 1x speed
- Time of day displayed as clock (6:00 AM → 10:00 PM)
- Speed buttons: Pause (0x), Play (1x), Fast (2x), Faster (3x)
- At 10:00 PM → signals Game Manager → triggers `DayEndProcessor`
- Visual: slight ambient color shift (warm morning → neutral midday → warm evening)

TimeManager does NOT directly update UI — it informs Game Manager of time changes, which forwards to UI Manager.

### 7. Resident System

Residents are **visual NPCs** that walk along the street. They use the **Object Pool** — never Instantiate/Destroy directly.

**Spawning rules:**
- Each residential building provides N resident slots
- Residents appear if global happiness > 40%
- At < 30% happiness, residents start leaving (1 per day, returned to pool)

**Walking behavior:**
- Residents are taken from pool and placed at their home building
- Pick a random destination building on the street
- Walk along the sidewalk (a fixed spline/path parallel to the road)
- Pause at destination for a few seconds, then pick a new one
- Use simple sprite-flip for direction
- Appearance is picked from ResidentData variants via Asset Library

### 8. Building Placement — Drag & Drop from Hand

This is a **card-based drag & drop system**, not a click-to-select panel. The player drags a card from their hand onto the street grid.

**HandController (`HandController.cs`):**
- At the start of Gameplay state, creates cards from Object Pool
- Cards are populated with BuildingData from Asset Library
- Cards display: building sprite, localized name (from Localization Manager), cost
- Laid out in a horizontal container at bottom of screen
- When a card is used (placed successfully), it is returned to pool
- At end of each day, HandController refills the hand with new cards

**HandCard (`HandCard.cs` — implements IBeginDragHandler, IDragHandler, IEndDragHandler):**

1. **Begin Drag**: Card lifts above all UI. `CanvasGroup.blocksRaycasts = false` — CRITICAL: without this, the card blocks the grid from receiving mouse events and hover detection breaks. Card remembers its original position in hand.

2. **During Drag**: Card follows cursor. The Street detects hover and BuildingPlacer highlights the nearest slot — green if free, red if occupied or can't afford.

3. **End Drag**: HandCard asks BuildingPlacer to attempt placement at the current highlighted slot.
   - **Success**: BuildingPlacer places the building, deducts cost via Game Manager, recalculates happiness. Sound Manager plays placement SFX. Card is returned to Object Pool (NOT Destroy).
   - **Failure** (occupied slot or cursor off-grid): Card animates smoothly back to its original hand position.

4. **Right-click or Escape during drag**: Cancel — card returns to hand.

**BuildingPlacer (`BuildingPlacer.cs`):**
- Lives on the Street GameObject alongside TerrainGridSystem and Street
- Exposes `static bool IsDragging` and `static BuildingData CurrentData` — read by Street during hover to highlight the full multi-cell footprint
- `bool TryPlace(BuildingData data)` — reads `Street.HoveredCellIndex` as the anchor cell, calls `Street.CanPlace(anchorCellIndex, data)` which checks ALL cells in the footprint, then `Street.PlaceBuilding(anchorCellIndex, data)` which marks all cells occupied
- On successful placement: deducts cost via GameManager, calls `HappinessSystem.Recalculate()`, notifies GameManager for sound/UI updates
- Does NOT do raycasting or cell detection — TGS2 handles all of that through its events
- Does NOT know about multi-cell logic — Street.cs handles footprint calculation internally

### 9. Camera (`IsometricCamera.cs`)

```
Type: Orthographic (configured automatically by UIT's Toggle SceneView)
Rotation: fixed — set by UIT projection, do NOT rotate manually
  - Use Tools > UIT > Toggle SceneView (Ctrl+G) to align camera
  - Use Tools > UIT > Projection > Isometric to match the included sprites
Zoom: orthographic size 5–15, scroll wheel
Pan: click-drag middle mouse OR edge scrolling OR WASD
  - Constrained to street bounds (don't let camera go off into empty space)
```

**NOTE**: UIT aligns the Main Camera automatically when toggling the isometric scene view. The `IsometricCamera.cs` script only handles zoom and pan — it must NOT override the camera's rotation or projection type.

### 10. UI Layout

All text comes from LocalizationManager. UI Manager controls visibility of all elements.

**HUD (top-left corner):**
- 😊 Happiness: 100%
- 💰 Money: $92,856
- 👤 Residents: 31
- 🌸 Decoration score: 67

**Hand / Build Panel (bottom of screen):**
- Horizontal row of draggable building cards
- Each card shows: building sprite, localized name, cost
- Cards are pooled objects managed by HandController
- Drag a card onto the grid to place it

**Time Controls (bottom-right):**
- ⏸ Pause | ▶ Play | ⏩ Fast | ⏩⏩ Faster
- Clock display: "9:50 AM"
- Day counter: "Day 5"

**Day Summary Popup:**
- Appears at end of each day (UI Manager shows it on Game Manager's command)
- Shows: income earned, expenses, net profit, happiness trend, new residents

**GameOver Screen:**
- Shown by UI Manager when State Machine enters GameOver state
- Displays final score, offers restart button

---

## Sorting & Rendering

Since this project uses the **Ultimate Isometric Toolkit**, depth sorting is handled automatically by the UIT shader per frame. We do NOT manually assign `sortingOrder` based on slot index.

**What we still control:**

Sorting Layers (broad categories, back to front):
1. `Background` — sky, clouds
2. `Road` — road surface, markings (floor tiles with `Order in Layer = -10`)
3. `Buildings` — all buildings, both sides (UIT shader handles depth within this layer)
4. `Decorations` — trees, bushes, power lines (`Order in Layer = +10` relative to buildings)
5. `Residents` — walking NPCs
6. `UI_World` — slot highlights, placement preview

**Per-building depth** (which building renders in front of which) is handled by UIT's `IsometricInstancingUnlit` shader based on 3D position. No manual code needed.

**Floor tiles** should always have a lower `Order in Layer` value (e.g., -10) to render behind everything else. This prevents sorting artifacts where floor tiles bleed through walls.

All SpriteRenderers must use the `IsometricInstancingMaterial` from `Assets/UltimateIsometricToolkit/Materials/` with GPU Instancing enabled.

---

## Initial Building Roster (10 Buildings)

| Name | Category | Width | Cost | Income | Happiness | Radius | Residents Provided | Residents Required |
|------|----------|-------|------|--------|-----------|--------|-------------------|-------------------|
| Small House | Residential | 1 | $500 | 0 | +5 | 0 | 2 | 0 |
| Apartment | Residential | 2 | $1200 | 0 | +3 | 0 | 5 | 0 |
| Café | Food | 1 | $800 | $50 | +8 | 2 | 0 | 1 |
| Bakery | Food | 1 | $600 | $40 | +6 | 1 | 0 | 1 |
| Bookshop | Shop | 1 | $700 | $35 | +7 | 1 | 0 | 1 |
| Bar | Entertainment | 2 | $900 | $80 | +4 | 2 | 0 | 2 |
| Small Park | Nature | 1 | $400 | 0 | +10 | 2 | 0 | 0 |
| Large Park | Nature | 3 | $1000 | 0 | +15 | 3 | 0 | 0 |
| Shrine | Cultural | 2 | $1500 | $20 | +12 | 3 | 0 | 0 |
| Convenience Store | Service | 1 | $500 | $60 | +3 | 1 | 0 | 1 |

Width = `widthInCells`. Small buildings (house, café, bakery, bookshop, small park, store) = 1 cell. Medium buildings (apartment, bar, shrine) = 2 cells. Large buildings (large park) = 3 cells. With a 10-column street per side, a full side can fit e.g. 10 small buildings, or 5 medium, or 3 large + 1 small, etc.

---

## Demo & Placeholder Art Instructions

### Primary: Use UIT Built-in Sprites

For the demo, use sprites from `Assets/UltimateIsometricToolkit/Art/` first. These have correct proportions and work with the UIT shader out of the box. Differentiate building types by tinting via `SpriteRenderer.color`:

- Residential = light blue tint (`#A8D8EA`)
- Food = warm orange tint (`#FFB347`)
- Shop = yellow tint (`#FDFD96`)
- Entertainment = purple tint (`#C3B1E1`)
- Nature = green tint (`#77DD77`)
- Cultural = red tint (`#FF6961`)
- Service = gray tint (`#B0B0B0`)

The Asset Library should reference both the sprite AND a tint color per BuildingData, so tinting is data-driven not hardcoded.

### Fallback: Generated Placeholder Sprites

If UIT does not provide enough sprite variety, generate **colored rectangle sprites** as fallback placeholders:

- Each building: a colored rectangle with the building name as text
- Colors: same category-based scheme as above
- Road: dark gray rectangle
- Residents: small colored circles (random pastel colors)
- Slot markers: semi-transparent white squares
- Hand cards: rounded rectangles with building sprite and text

Create these via a helper editor script that generates placeholder `.png` files.

### Sprite Import Rules

- All sprites used by UIT must have `IsometricInstancingMaterial` on their SpriteRenderer
- Building tile sprites must NOT be packed into a Sprite Atlas (texture methods would get the atlas instead of individual sprite)
- For custom sprites: set Pixels Per Unit = `sprite_width_px / sqrt(2)` (e.g., 256px → 181.02 PPU)
- UIT's included sprites already have correct PPU — do not change their import settings

---

## Game Loop Summary

```
INIT STATE:
  → Asset Library already assigned (SO, inspector)
  → Create Object Pools (cards, residents, effects)
  → Initialize Sound Manager, UI Manager
  → Load saved data via Data Manager
  → Apply language via Localization Manager
  → Initialize Time Manager
  → Transition to Gameplay State

GAMEPLAY STATE — ENTER:
  → Generate street with empty slots
  → Give player starting money ($5000) (or load from save)
  → Start Day 1 at 6:00 AM
  → Spawn initial hand cards from pool via HandController
  → Show HUD via UI Manager

GAMEPLAY STATE — EACH FRAME:
  → Time Manager advances time based on timeScale
  → Resident NPCs update positions (walking)
  → Handle drag & drop input (HandCard + BuildingPlacer)
  → Camera pan/zoom input

GAMEPLAY STATE — EACH DAY END (10:00 PM):
  → DayEndProcessor calculates income (modified by happiness)
  → Subtracts upkeep costs
  → Game Manager updates Money
  → Check happiness → ResidentManager spawns/despawns from pool
  → Game Manager tells UI Manager to show Day Summary Popup
  → Advance to next day (6:00 AM)
  → HandController refills hand with new cards from pool

GAMEPLAY STATE — EXIT (Day 30 or player quits):
  → Data Manager saves progress
  → Return all pooled objects
  → Transition to GameOver State

GAMEOVER STATE:
  → UI Manager shows GameOver screen with final score
  → Restart button → transition back to Init State
```

---

## Communication Flow Examples

To clarify how managers interact through Game Manager — no direct manager-to-manager calls:

**Building placed successfully (e.g. 2-cell Apartment):**
```
HandCard.OnEndDrag()
  → BuildingPlacer.TryPlace(data)
    → Street.CanPlace(anchorCellIndex, data)
      → GetFootprint(anchor, data) → [cell 3, cell 4]
      → Check both cells free + can afford → true
    → Street.PlaceBuilding(anchorCellIndex, data)
      → Position prefab at center of cells 3 & 4
      → occupants[3] = instance, occupants[4] = instance
      → anchorCells.Add(3)
      → tgs.CellFlash([3, 4], white, 0.3f)
    → HappinessSystem.Recalculate()
      → Street.GetAllBuildings() (iterates anchors only)
      → For each: Street.GetNeighbors(instance, radius) (unions neighbors from all footprint cells)
    → GameManager.SpendMoney(data.purchaseCost)
      → UIManager.UpdateMoney(newAmount)
    → GameManager.OnHappinessChanged(newPercent)
      → UIManager.UpdateHappiness(newPercent)
    → GameManager.PlaySound("place_building")
      → SoundManager.PlaySFX(AssetLibrary.placeBuildingSound)
    → HandCard returned to Object Pool
```

**Day ends:**
```
TimeManager signals → GameManager.OnDayEnd()
  → DayEndProcessor.Process()
    → IncomeSystem.CalculateDailyIncome() → returns int
    → GameManager.AddMoney(income)
      → UIManager.UpdateMoney()
    → ResidentManager.EvaluateResidents()
      → Spawn/despawn via Object Pool
      → UIManager.UpdateResidents(count)
    → GameManager.PlaySound("day_end")
    → UIManager.ShowDaySummary(summaryData)
  → HandController.RefillHand()
    → Cards from Object Pool, data from Asset Library
  → GameManager.CurrentDay++
  → TimeManager.StartNewDay()
```

---

## Editor Tooling

Create a custom editor window `MiniCityDebugWindow.cs`:
- Show all slot states (occupied/empty, building name) for both sides
- Override money, happiness, time
- Force day-end
- Buttons to place/remove buildings by side + index
- Pool inspector: show active/inactive count per pool
- State Machine: show current state, buttons to force transitions

---

## What to Generate

When processing this spec, generate:

1. **All C# scripts** listed in the project structure, fully implemented
2. **All ScriptableObject assets** (AssetLibrary, 10 buildings, synergy table) as `.asset` files or a setup script — BuildingData assets should reference sprites from `Assets/UltimateIsometricToolkit/Art/` and include category tint colors
3. **Localization JSON files** (en.json, ru.json) with all building names, descriptions, UI strings
4. **A scene setup script** (`SetupGameScene.cs`) that creates the game scene hierarchy:
   - GameManager (DontDestroyOnLoad) with all manager references
   - Street GameObject with TerrainGridSystem component (Box topology, 10 cols × 2 rows) + Street.cs + BuildingPlacer.cs
   - Camera with correct ortho settings (aligned via UIT)
   - Canvas (Screen Space Camera) with HUD, Hand Panel, Time Controls
5. **An editor script** to generate fallback placeholder sprites (for buildings not covered by UIT Art/)
6. **A prefab setup script** that ensures all building/resident/environment prefabs use `IsometricInstancingMaterial`
7. **Assembly definition files** for clean compilation

The project assumes these assets already exist and are imported. Do NOT regenerate or modify any files inside their folders:
- `Assets/UltimateIsometricToolkit/` — Ultimate Isometric Toolkit
- Terrain Grid System 2 folder (wherever it was imported by the user)

Do NOT generate:
- The Ultimate Isometric Toolkit itself (already in project)
- Terrain Grid System 2 itself (already in project)
- Audio files (only the AudioClip reference slots in Asset Library)
- Main menu scene (just the game scene)

---

## Code Style

- Use `[SerializeField]` for inspector fields, not public fields
- Use `#region` blocks for organization in larger files
- Null-check with `if (x != null)` not `x?.` chains (clearer for Unity)
- Use `TryGetComponent` over `GetComponent` where failure is possible
- Events: use `System.Action` or UnityEvents, not C# event keyword
- Comments: brief, explain WHY not WHAT
- All display text goes through LocalizationManager — zero hardcoded strings in UI
- All object creation/destruction for pooled types goes through ObjectPoolManager
- No manager-to-manager direct calls — always route through GameManager
- Canvas must be Screen Space Camera mode, not Overlay
- **UIT rules**: all SpriteRenderers use `IsometricInstancingMaterial`, never `Sprites-Default`
- **UIT rules**: any Rigidbody on a sprite object must have `freezeRotation = true`
- **UIT rules**: do not manually set `sortingOrder` for depth — UIT shader handles it; only use `Order in Layer` for broad layer separation (floor vs buildings vs decorations)
- **UIT rules**: do not modify files inside `Assets/UltimateIsometricToolkit/`
- **TGS2 rules**: never write custom grid rendering or mouse-to-cell raycasting — use TGS2 events and API
- **TGS2 rules**: cell index (single int from TGS2) is the canonical identifier for a slot, not row+column pairs
- **TGS2 rules**: use `CellGetRow`/`CellGetColumn` only when you need to know which side of street / position along street
- **TGS2 rules**: highlight cells via `CellSetColor`/`CellToggleRegionSurface`, not custom GameObjects
- **TGS2 rules**: do not modify files inside the Terrain Grid System 2 folder
