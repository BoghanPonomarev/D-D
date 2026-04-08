using UnityEngine;

public class NPCWaitState : INPCState
{
    float endTime;

    public void Enter(NPCEntity npc)
    {
        npc.Movement.Stop();
        float duration = Random.Range(npc.Data.waitDurationMin, npc.Data.waitDurationMax);
        endTime = Time.time + duration;
    }

    public void Tick(NPCEntity npc)
    {
        if (Time.time >= endTime)
            npc.StateMachine.ChangeState(new NPCWalkState());
    }

    public void Exit(NPCEntity npc) { }
}
