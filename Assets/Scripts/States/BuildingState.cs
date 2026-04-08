using Data;
using UnityEngine;

public class BuildingState : GameState
{
    private BuildingData buildingData;
    
    public BuildingState(GameManager gameManager, BuildingData buildingData) : base(gameManager)
    {
        this.buildingData = buildingData;
    }

    public override void Enter() { }

    public override void Tick()
    {
        gameManager.BuildingManager.UpdatePlacingMarker(buildingData);

        // TODO remove after UI
        if (Input.GetMouseButtonDown(1))
        {
            gameManager.TransitionTo(new GameplayState(gameManager));
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.BuildingManager.PlaceBuilding(buildingData);
            gameManager.TransitionTo(new GameplayState(gameManager));
        }
    }

    public override void Exit()
    {
        gameManager.BuildingManager.RemovePlacingMarker();
    }
}