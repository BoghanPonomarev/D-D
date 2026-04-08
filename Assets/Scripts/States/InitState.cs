public class InitState : GameState
{
    public InitState(GameManager gameManager) : base(gameManager) { }

    public override void Enter()
    {
        gameManager.Pool.Initialize();
        gameManager.Sound.Initialize();
        gameManager.UI.Initialize();
        gameManager.Data.Initialize();
        gameManager.Localization.Initialize();
        gameManager.GameTime.Initialize();
        gameManager.Residents.Initialize();
        gameManager.Crowd.Initialize();

        gameManager.TransitionTo(new GameplayState(gameManager));
    }

    public override void Tick() { }
    public override void Exit() { }
}
