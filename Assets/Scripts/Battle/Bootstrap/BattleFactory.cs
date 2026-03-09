using System.Collections.Generic;
using Battle.Core;
using Battle.Data;

namespace Battle.Bootstrap
{
    public class BattleFactory
    {
        public BattleState Create(EncounterDefinition encounter)
        {
            return new BattleState
            {
                Heroes = CreateTeam(encounter.Heroes, isHero: true),
                Enemies = CreateTeam(encounter.Enemies, isHero: false)
            };
        }

        private TeamState CreateTeam(List<UnitDefinition> definitions, bool isHero)
        {
            var units = new List<UnitState>();
            for (int i = 0; i < definitions.Count; i++)
            {
                var def = definitions[i];
                units.Add(new UnitState
                {
                    Id = $"{def.UnitName}_{i}",
                    Definition = def,
                    CurrentHp = def.BaseHp,
                    IsHero = isHero,
                    SlotIndex = i,
                    SkillPool = new List<SkillDefinition>(def.SkillPool),
                    CurrentSkills = new List<SkillDefinition>()
                });
            }
            return new TeamState { Units = units };
        }
    }
}
