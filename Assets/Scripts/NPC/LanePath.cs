using System.Collections.Generic;
using UnityEngine;

public class LanePath : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;

    public int WaypointCount => waypoints.Length;

    public Vector3 GetWaypointPosition(int index) => waypoints[index].position;

    public bool IsValidIndex(int index) => index >= 0 && index < waypoints.Length;

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            Gizmos.DrawSphere(waypoints[i].position, 0.1f);
        }
        if (waypoints[waypoints.Length - 1] != null)
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.1f);
    }
}