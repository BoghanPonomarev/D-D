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
            var skillPool = new List<SkillDefinition>(unit.SkillPool);

            for (int currentIndex = skillPool.Count - 1; currentIndex > 0; currentIndex--)
            {
                int nextIndex = _random.Next(currentIndex + 1);
                (skillPool[currentIndex], skillPool[nextIndex]) = (skillPool[nextIndex], skillPool[currentIndex]);
            }

            unit.CurrentSkills = skillPool.GetRange(0, Math.Min(count, skillPool.Count));
        }
    }
}
