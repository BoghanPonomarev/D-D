using System;
using System.Collections.Generic;
using Battle.Core;
using Battle.Data;
using Battle.Services;

namespace Battle.AI
{
    public class BattleAI
    {
        private readonly Random _random = new Random();
        private readonly TargetingService _targeting;

        public BattleAI(TargetingService targeting)
        {
            _targeting = targeting;
        }

        public BattleAction DecideAction(UnitState unit, BattleState state)
        {
            var skill = unit.CurrentSkills[_random.Next(unit.CurrentSkills.Count)];
            var validTargets = _targeting.GetValidTargets(skill, unit, state);

            List<UnitState> chosen;
            if (skill.TargetType == TargetType.AllEnemies || skill.TargetType == TargetType.AllAllies)
                chosen = validTargets;
            else
                chosen = new List<UnitState> { validTargets[_random.Next(validTargets.Count)] };

            return new BattleAction { Caster = unit, Skill = skill, Targets = chosen };
        }
    }
}
