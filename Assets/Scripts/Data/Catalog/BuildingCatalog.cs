using System.Collections.Generic;
using UnityEngine;
using Data;

[CreateAssetMenu(fileName = "BuildingCatalog", menuName = "Game/BuildingCatalog")]
public class BuildingCatalog : ScriptableObject
{
    [SerializeField] private List<BuildingData> buildings = new();

    private Dictionary<string, BuildingData> buildingsById;

    public void Initialize()
    {
        buildingsById = new Dictionary<string, BuildingData>();

        foreach (var building in buildings)
        {
            if (building == null || string.IsNullOrEmpty(building.id))
                continue;

            buildingsById[building.id] = building;
        }
    }

    public BuildingData GetById(string id)
    {
        if (buildingsById == null)
            Initialize();

        buildingsById.TryGetValue(id, out var buildingData);
        return buildingData;
    }

    public IReadOnlyList<BuildingData> GetAll()
    {
        return buildings;
    }
}