public class NPCLeaveState : INPCState
{
    public void Enter(NPCEntity npc)
    {
        npc.Movement.Stop();
        npc.RequestDespawn();
    }

    public void Tick(NPCEntity npc) { }

    public void Exit(NPCEntity npc) { }
}
