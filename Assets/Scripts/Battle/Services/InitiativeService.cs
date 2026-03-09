using System.Collections.Generic;
using System.Linq;
using Battle.Core;

namespace Battle.Services
{
    public class InitiativeService
    {
        public List<UnitState> BuildTurnQueue(BattleState state)
        {
            return state.Heroes.Units
                .Concat(state.Enemies.Units)
                .Where(u => u.IsAlive)
                .OrderByDescending(u => u.Definition.Speed)
                .ToList();
        }
    }
}
