# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Single-street isometric city builder inspired by *Minami Lane*. The player drags building cards from a hand panel onto a two-sided street grid. Buildings generate income and happiness; residents walk the street as visual NPCs. Built in Unity 6 (6000.1.11f1), URP 2D renderer.

## Unity Workflow

No CLI build/test pipeline — everything runs through the Unity Editor.

- **Open project:** Unity Hub → open this directory with Unity 6000.1.11f1
- **Run tests:** Window → General → Test Runner
- **Build:** File → Build Settings

## Architecture

### Central Coordination

`GameManager` is a DontDestroyOnLoad singleton that owns a `StateMachine` (Init → Gameplay → GameOver) and holds references to all managers. Nothing calls managers directly — all communication goes through GameManager. States signal GameManager when transitions are needed; they never call each other.

**Initialization order (InitState):**
1. AssetLibrary → 2. ObjectPoolManager → 3. SoundManager + UIManager → 4. DataManager → 5. LocalizationManager → 6. TimeManager → 7. Transition to GameplayState

### Key Systems

**Street (`Street.cs`)** — wrapper over TGS2. Owns `Dictionary<int, BuildingInstance> occupants` (TGS2 cell index → occupant) and `HashSet<int> anchorCells`. Buildings occupy 1–3 cells (`widthInCells` in BuildingData); all occupied cells map to the same `BuildingInstance`, but only anchor cells are iterated for happiness/income to avoid double-counting.

**BuildingData** — ScriptableObject. `buildingId` is a localization key; display name = `LocalizationManager.Get("building.{id}.name")`. No hardcoded display strings. Includes `widthInCells` for multi-cell footprint.

**AssetLibrary** — ScriptableObject. All game data (BuildingData, prefabs, audio, sprites) assigned in Inspector. No `Resources.Load` anywhere in the codebase.

**HandCard / HandController** — card drag & drop using `IBeginDragHandler`/`IDragHandler`/`IEndDragHandler`. `CanvasGroup.blocksRaycasts = false` during drag is critical — without it the card blocks TGS2 from receiving mouse events and hover detection breaks.

**BuildingPlacer** — exposes `static bool IsDragging` and `static BuildingData CurrentData` (read by Street during hover). Calls `Street.CanPlace` / `Street.PlaceBuilding` on drop; deducts cost via GameManager, triggers `HappinessSystem.Recalculate()`. Does NOT do raycasting — TGS2 handles all cell detection.

**HappinessSystem** — recalculates on every placement/removal. Per-building happiness = base + Σ synergy bonuses from neighbors within radius. Global = average, clamped 0–100.

**IncomeSystem** — called by DayEndProcessor: `income = Σ(building.incomePerDay × happinessMultiplier) - Σ(upkeep)`. Multiplier = `0.5 + happiness/100`.

**TimeManager** — 60 real seconds per day at 1x speed. Speed: 0x/1x/2x/3x. Signals GameManager at day end; never touches UI directly.

**ResidentManager** — spawns/despawns NPCs from Object Pool. Residents appear when happiness > 40%; leave at < 30%. Walk along a fixed sidewalk spline between random buildings.

**ObjectPoolManager** — used for hand cards, resident NPCs, and VFX. Never `Instantiate`/`Destroy` pooled objects directly.

### Key Technologies

- **Grid:** Terrain Grid System 2 (TGS2) by Kronnect at `Assets/TerrainGridSystem/`. Box topology, 10 columns × 2 rows. Handles grid rendering, cell detection, highlighting, and neighbor queries. Do NOT write custom raycasting or grid rendering — use TGS2 events and API.
  - Events: `OnCellEnter`, `OnCellExit`, `OnCellClick`
  - API: `CellGetPosition`, `CellSetColor`, `CellToggleRegionSurface`, `CellFlash`, `CellGetNeighbors`, `CellGetAtWorldPosition`, `CellGetRow`/`CellGetColumn`/`CellGetIndex`
  - Cell index (single `int`) is the canonical slot identifier. Use `CellGetRow`/`CellGetColumn` only when you need side-of-street or along-street position.
- **Isometric rendering:** Ultimate Isometric Toolkit (UIT) at `Assets/UltimateIsometricToolkit/`. Every `SpriteRenderer` must use `Assets/UltimateIsometricToolkit/Materials/IsometricInstancingMaterial` (shader: `IsometricInstancingUnlit`, GPU Instancing enabled).
- **Depth sorting:** Handled automatically by UIT shader — do not set manual `sortingOrder` based on position. `Order in Layer` is valid for broad categories only (floor = -10, buildings = 0, decorations = +10).
- **Input:** New Input System only — never use legacy `Input` class.
- **UI:** UGUI + TextMesh Pro, Canvas in **Screen Space - Camera** mode (not Overlay).
- **Unity-MCP:** `com.ivanmurzak.unity.mcp` (v0.51.4) bridges Claude to the Unity Editor via MCP.

### UIT Scene Setup

1. `Tools > UIT > Toggle SceneView` (Ctrl+G) — aligns Scene View to isometric angle
2. `Tools > UIT > Projection > Isometric`
3. `Tools > Snapping Tool` (Ctrl+L) — snap objects to grid during level design
4. Camera is orthographic and aligned automatically by UIT — do not change its rotation manually

### TGS2 Inspector Settings (Street GameObject)

| Setting | Value |
|---------|-------|
| Topology | Box |
| Column Count | 10 |
| Row Count | 2 |
| Cells Flat | true |
| Cell Highlight Color | Transparent (alpha = 0) |
| Show Cells | true |

## Critical Constraints

- No `Resources.Load` — all asset references go through AssetLibrary only.
- No hardcoded display strings — all text through `LocalizationManager.Get(key)`.
- Never `Instantiate`/`Destroy` pooled objects — use ObjectPoolManager.
- Never write custom mouse-to-cell raycasting — TGS2 events handle all cell detection.
- Canvas must be **Screen Space - Camera**, not Overlay.
- All SpriteRenderers must use `IsometricInstancingMaterial`.
- Any Rigidbody on a sprite object must have `freezeRotation = true` (UIT requirement).
- Do not modify files inside `Assets/UltimateIsometricToolkit/` or the TGS2 folder.

## Conventions

- All game scripts go in `Assets/Scripts/`
- No comments in code — use self-documenting names only
- Project settings: 1920×1080, Landscape, Linear color space
