using UnityEngine;

public class NPCWalkState : INPCState
{
    public void Enter(NPCEntity npc)
    {
        MoveToCurrentWaypoint(npc);
        SetUpNextWaypoint(npc);
    }

    public void Tick(NPCEntity npc)
    {
        SetUpNextWaypoint(npc);
        
        if (!npc.Movement.ReachedTarget) return;

        // TODO remove it for smooth moving
        npc.CurrentWaypointIndex++;
        
        if (!npc.AssignedPath.IsValidIndex(npc.CurrentWaypointIndex))
        {
            npc.StateMachine.ChangeState(new NPCLeaveState());
            return;
        }

        if (Random.value < npc.Data.waitChancePerWaypoint)
        {
            npc.StateMachine.ChangeState(new NPCWaitState());
            return;
        }

        MoveToCurrentWaypoint(npc);
    }

    public void Exit(NPCEntity npc) { }

    private void SetUpNextWaypoint(NPCEntity npc)
    {
        /*
         // TODO return when need smooth moving
        
        if (npc.Movement.NextTargetPosition == null 
            && npc.AssignedPath.IsValidIndex(npc.CurrentWaypointIndex + 1))
        {
            npc.Movement.NextTargetPosition = npc.AssignedPath.GetWaypointPosition(npc.CurrentWaypointIndex + 1);
            npc.CurrentWaypointIndex++;
        }
         */
    }
    
    private void MoveToCurrentWaypoint(NPCEntity npc)
    {
        npc.Movement.MoveTo(npc.AssignedPath.GetWaypointPosition(npc.CurrentWaypointIndex));
    }
}