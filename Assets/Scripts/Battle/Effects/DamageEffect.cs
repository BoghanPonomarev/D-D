using System.Collections.Generic;
using Battle.Core;
using UnityEngine;

namespace Battle.Effects
{
    [CreateAssetMenu(menuName = "SliceDungeon/Effect/Damage")]
    public class DamageEffect : EffectDefinition
    {
        public int BaseDamage;

        public override List<BattleEvent> Apply(UnitState caster, UnitState target, BattleState state)
        {
            target.CurrentHp -= BaseDamage;

            var events = new List<BattleEvent>
            {
                new BattleEvent { Type = BattleEventType.DamageDealt, Source = caster, Target = target, Value = BaseDamage }
            };

            if (!target.IsAlive)
                events.Add(new BattleEvent { Type = BattleEventType.UnitDied, Target = target });

            return events;
        }
    }
}
