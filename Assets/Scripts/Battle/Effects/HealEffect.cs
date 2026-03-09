using System.Collections.Generic;
using Battle.Core;
using UnityEngine;

namespace Battle.Effects
{
    [CreateAssetMenu(menuName = "SliceDungeon/Effect/Heal")]
    public class HealEffect : EffectDefinition
    {
        public int HealAmount;

        public override List<BattleEvent> Apply(UnitState caster, UnitState target, BattleState state)
        {
            target.CurrentHp = System.Math.Min(target.CurrentHp + HealAmount, target.Definition.BaseHp);

            return new List<BattleEvent>
            {
                new BattleEvent { Type = BattleEventType.HealApplied, Source = caster, Target = target, Value = HealAmount }
            };
        }
    }
}
