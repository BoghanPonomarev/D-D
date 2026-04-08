using Building;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap buildableCellsTileMap;

    
    private GridManager gridManager;

    private GameObject currentPlaceMarker;
    private GameObject buildingsObjectContainer;
    
    private void Awake()
    {
        currentPlaceMarker = GameObject.Find("Place Marker");
        currentPlaceMarker.SetActive(false);
        
        buildingsObjectContainer = GameObject.Find("Buildings Container");
        
        gridManager = new GridManager(buildableCellsTileMap);
    }
    
    public void UpdatePlacingMarker(BuildingData buildingData)
    {
        CellData cellData = GetCurrentMouseCell();
        bool isValidForPlacing = IsValidForPlacing(cellData);
        
        if (isValidForPlacing)
        {
            currentPlaceMarker.SetActive(true);
            currentPlaceMarker.transform.position = cellData.centerWorldPosition;
        }
        else
        {
            currentPlaceMarker.SetActive(false);
        }
    }

    private CellData GetCurrentMouseCell()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        return gridManager.GetBuildableCellByWorldPosition(worldPos);
    }

    public void RemovePlacingMarker()
    {
        currentPlaceMarker.SetActive(false);
    }

    public void PlaceBuilding(BuildingData buildingData)
    {
        CellData cellData = GetCurrentMouseCell();
        bool isValidForPlacing = IsValidForPlacing(cellData);
     
        if (isValidForPlacing)
        {
            Vector3 spawnPosition = cellData.centerWorldPosition;
            
            GameObject building = Instantiate(buildingData.prefab, spawnPosition, Quaternion.identity, 
                buildingsObjectContainer.transform);
            
            building.name = $"{buildingData.name}_{cellData.worldToCellPosition.x}_{cellData.worldToCellPosition.y}";
            
            SpriteRenderer[] renderers = building.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = BuildableCellsFactory.CellsSlotsLength - cellData.orderId;
            }
            cellData.buildingData = buildingData;
        }
    }
    
    private bool IsValidForPlacing(CellData cellData)
    {
        return cellData != null && cellData.buildingData == null;
    }
    
}