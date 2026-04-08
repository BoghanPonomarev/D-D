using UnityEngine;

public class NPCEntity : MonoBehaviour
{
    public NPCData Data { get; private set; }
    public NPCMovement Movement { get; private set; }
    public NPCStateMachine StateMachine { get; private set; }
    public LanePath AssignedPath { get; private set; }
    public int CurrentWaypointIndex { get; set; }

    CrowdManager crowdManager;
    bool despawnRequested;

    void Awake()
    {
        Movement = GetComponent<NPCMovement>();
        StateMachine = new NPCStateMachine(this);
    }

    public void Initialize(NPCData data, LanePath path, CrowdManager manager)
    {
        Data = data;
        AssignedPath = path;
        crowdManager = manager;
        CurrentWaypointIndex = 0;
        despawnRequested = false;

        transform.position = path.GetWaypointPosition(0);
        Movement.Setup(data.moveSpeed, data.movementStopDistance, data.movementReachTargetDistance);
        StateMachine.ChangeState(new NPCWalkState());
    }

    public void Tick()
    {
        if (despawnRequested) return;
        StateMachine.Tick();
    }

    public void RequestDespawn()
    {
        if (despawnRequested) return;
        despawnRequested = true;
        crowdManager.DespawnNPC(this);
    }
}
