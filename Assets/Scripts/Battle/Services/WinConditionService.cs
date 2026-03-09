using Battle.Core;

namespace Battle.Services
{
    public enum BattleResult
    {
        Ongoing,
        HeroesWin,
        EnemiesWin,
    }

    public class WinConditionService
    {
        public BattleResult Check(BattleState state)
        {
            if (state.Enemies.IsDefeated) return BattleResult.HeroesWin;
            if (state.Heroes.IsDefeated) return BattleResult.EnemiesWin;
            return BattleResult.Ongoing;
        }
    }
}
