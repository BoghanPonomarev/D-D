using System.Collections.Generic;
using Battle.Data;

namespace Battle.Core
{
    public class UnitState
    {
        public string Id;
        public UnitDefinition Definition;

        public int CurrentHp;
        public bool IsAlive => CurrentHp > 0;
        public bool IsHero;
        public int SlotIndex;

        public List<SkillDefinition> SkillPool;
        public List<SkillDefinition> CurrentSkills;
    }
}
