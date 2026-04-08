public class NPCStateMachine
{
    NPCEntity owner;
    INPCState currentState;

    public INPCState CurrentState => currentState;

    public NPCStateMachine(NPCEntity owner)
    {
        this.owner = owner;
    }

    public void ChangeState(INPCState newState)
    {
        currentState?.Exit(owner);
        currentState = newState;
        currentState?.Enter(owner);
    }

    public void Tick()
    {
        currentState?.Tick(owner);
    }
}