using System.Collections.Generic;
using Battle.Data;

namespace Battle.Core
{
    public class BattleAction
    {
        public UnitState Caster;
        public SkillDefinition Skill;
        public List<UnitState> Targets;
    }
}
