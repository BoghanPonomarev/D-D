using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Game/BuildingData")]
    public class BuildingData : ScriptableObject
    {
        public string id;
        public int widthInCells = 1;
        public GameObject prefab;
        public Sprite icon;
    }
}
