public interface INPCState
{
    void Enter(NPCEntity npc);
    void Tick(NPCEntity npc);
    void Exit(NPCEntity npc);
}