public abstract class GameState
{
    protected GameManager gameManager;

    protected GameState(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
}
