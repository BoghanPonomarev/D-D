using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap buildableCellsTileMap;
    
    private GridManager gridManager;
    private BuildingPlacer buildingPlacer;

    private void Awake()
    {
        GameObject placeMarker = GameObject.Find("Place Marker");
        placeMarker.SetActive(false);
        buildingPlacer = new BuildingPlacer(placeMarker);
        
        gridManager = new GridManager(buildableCellsTileMap);
    }
    
    private void Update()
    {
        UpdatePlacingMarker();
    }

    private void UpdatePlacingMarker()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        CellData cellData = gridManager.GetBuildableCellByWorldPosition(worldPos);
        
        buildingPlacer.UpdatePlacing(cellData);
    }
}