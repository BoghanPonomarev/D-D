using Data;
using UnityEngine;

public class GameplayState : GameState
{
    public GameplayState(GameManager gameManager) : base(gameManager) { }

    public override void Enter() { }

    public override void Tick()
    {
        
        // TODO remove after UI
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildingData houseData = gameManager.Data.buildingCatalog.GetById("House");
            gameManager.TransitionTo(new BuildingState(gameManager, houseData));
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuildingData warehouseData = gameManager.Data.buildingCatalog.GetById("Warehouse");
            gameManager.TransitionTo(new BuildingState(gameManager, warehouseData));
        }
    }
    public override void Exit() { }
}
