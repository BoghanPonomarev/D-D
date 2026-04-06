using System;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingPlacer
{
    private GameObject currentMarker;
    private bool wasHoveringLastFrame = false;
    private Vector3Int currentCellPosition;


    public BuildingPlacer(GameObject currentMarker)
    {
        this.currentMarker = currentMarker;
    }

    public void UpdatePlacing(CellData cellData)
    {
        bool isValidForPlacing = IsValidForPlacing(cellData);
        
        // Мышка зашла на тайл
        if (isValidForPlacing && !wasHoveringLastFrame)
        {
            currentMarker.SetActive(true);
            currentMarker.transform.position = cellData.centerWorldPosition; // buildAreaTilemap.GetCellCenterWorld(cellPos);
            currentCellPosition = cellData.worldToCellPosition;
        }
        // Мышка всё ещё на тайле
        else if (isValidForPlacing && wasHoveringLastFrame)
        {
            if (cellData.worldToCellPosition != currentCellPosition)
            {
                currentCellPosition = cellData.worldToCellPosition;

                if (currentMarker != null)
                {
                    currentMarker.transform.position = cellData.centerWorldPosition;
                }
            }
        }
        // Мышка вышла с тайла
        else if (!isValidForPlacing && wasHoveringLastFrame)
        {
            if (currentMarker != null)
            {
                currentMarker.SetActive(false);
            }
        }

        wasHoveringLastFrame = isValidForPlacing;
    }


    private bool IsValidForPlacing(CellData cellData)
    {
        return cellData != null && cellData.buildingData == null;
    }
}
