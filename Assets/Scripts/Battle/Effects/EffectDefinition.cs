using System.Collections.Generic;
using Battle.Core;
using UnityEngine;

namespace Battle.Effects
{
    public abstract class EffectDefinition : ScriptableObject
    {
        public abstract List<BattleEvent> Apply(UnitState caster, UnitState target, BattleState state);
    }
}
