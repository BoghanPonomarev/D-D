using System.Collections.Generic;
using System.Linq;
using Battle.Core;
using Battle.Data;

namespace Battle.Services
{
    public class TargetingService
    {
        public List<UnitState> GetValidTargets(SkillDefinition skill, UnitState caster, BattleState state)
        {
            var enemyTeam = caster.IsHero ? state.Enemies : state.Heroes;
            var allyTeam = caster.IsHero ? state.Heroes : state.Enemies;

            return skill.TargetType switch
            {
                TargetType.SingleEnemy  => enemyTeam.Units.Where(u => u.IsAlive).ToList(),
                TargetType.SingleAlly   => allyTeam.Units.Where(u => u.IsAlive).ToList(),
                TargetType.Self         => new List<UnitState> { caster },
                TargetType.AllEnemies   => enemyTeam.Units.Where(u => u.IsAlive).ToList(),
                TargetType.AllAllies    => allyTeam.Units.Where(u => u.IsAlive).ToList(),
                _                       => new List<UnitState>()
            };
        }
    }
}
