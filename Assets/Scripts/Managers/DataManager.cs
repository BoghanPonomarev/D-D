using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] public BuildingCatalog buildingCatalog;

    public void Initialize()
    {
        buildingCatalog.Initialize();
    }
}
