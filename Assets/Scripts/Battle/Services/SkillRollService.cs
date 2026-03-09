using System;
using System.Collections.Generic;
using Battle.Core;
using Battle.Data;

namespace Battle.Services
{
    public class SkillRollService
    {
        private readonly Random _random = new Random();

        public void Roll(UnitState unit, int count)
        {
            var pool = new List<SkillDefinition>(unit.SkillPool);

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            unit.CurrentSkills = pool.GetRange(0, Math.Min(count, pool.Count));
        }
    }
}
