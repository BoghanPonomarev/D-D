public class StateMachine
{
    GameState current;

    public void ChangeState(GameState next)
    {
        current?.Exit();
        current = next;
        current.Enter();
    }

    public void Tick()
    {
        current?.Tick();
    }
}
