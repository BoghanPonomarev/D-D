using System.Collections.Generic;

namespace Battle.Core
{
    public class BattleResolver
    {
        public List<BattleEvent> Resolve(BattleAction action, BattleState state)
        {
            var allEvents = new List<BattleEvent>();

            foreach (var target in action.Targets)
                foreach (var effect in action.Skill.Effects)
                    allEvents.AddRange(effect.Apply(action.Caster, target, state));

            return allEvents;
        }
    }
}
