using System.Collections.Generic;
using Building;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager
{
    private readonly Tilemap buildableCellsTileMap;

    private readonly Dictionary<Vector3Int, CellData> buildableCells;

    
    public GridManager(Tilemap buildableCellsTileMap)
    {
        this.buildableCellsTileMap = buildableCellsTileMap;
        
        // For future if we have some special rules on map gen - we can modify this factory,
        // for example add some dependencies
        BuildableCellsFactory buildableCellsFactory = new BuildableCellsFactory(buildableCellsTileMap);

        buildableCells = buildableCellsFactory.CreateBuildableGridCells();
    }
    
    public CellData GetBuildableCellByWorldPosition(Vector3 worldPos)
    {
        Vector3Int cellPos = buildableCellsTileMap.WorldToCell(worldPos);
        
        return buildableCells.GetValueOrDefault(cellPos);
    }
    
    
}