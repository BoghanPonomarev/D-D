using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CellData", menuName = "Game/CellData")]
    public class CellData : ScriptableObject
    {
        public int orderId;
        
        public Vector3Int worldToCellPosition;
        public Vector3 centerWorldPosition;
        
        public BuildingData buildingData;
        
    }
}