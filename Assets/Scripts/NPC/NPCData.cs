using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "NPC/NPCData")]
public class NPCData : ScriptableObject
{
    public string archetypeId;
    public GameObject prefab;
    public float moveSpeed = 2f;
    public float movementStopDistance = 0.15f;
    public float movementReachTargetDistance = 0.5f;
    public float waitDurationMin = 1f;
    public float waitDurationMax = 3f;
    public float waitChancePerWaypoint = 0.15f;
}