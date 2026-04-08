using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Building
{
    public class BuildableCellsFactory
    {
        public const int CellsSlotsLength = 10;
        private const int StartCellXIndex = -3;

        private readonly Tilemap buildableCellsTileMap;

        public BuildableCellsFactory(Tilemap buildableCellsTileMap)
        {
            this.buildableCellsTileMap = buildableCellsTileMap;
        }
        
        public Dictionary<Vector3Int, CellData> CreateBuildableGridCells()
        {
            Dictionary<Vector3Int, CellData> resultBuildableCells = new Dictionary<Vector3Int, CellData>();

            int currentXIndex = StartCellXIndex;
            for (int i = 0; i < CellsSlotsLength; i++)
            {
                CellData cellData = ScriptableObject.CreateInstance<CellData>();
                Vector3Int currentCellPosition = new Vector3Int(currentXIndex, -1, 0);

                cellData.orderId = currentXIndex + 3;
                cellData.worldToCellPosition = currentCellPosition;
                cellData.centerWorldPosition = buildableCellsTileMap.GetCellCenterWorld(currentCellPosition);
            
                resultBuildableCells[currentCellPosition] = cellData;
                currentXIndex++;
            }
            
            return resultBuildableCells;
        }
    }
}