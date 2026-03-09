using System.Collections.Generic;
using UnityEngine;

namespace Battle.Data
{
    [CreateAssetMenu(menuName = "SliceDungeon/Unit")]
    public class UnitDefinition : ScriptableObject
    {
        public string UnitName;
        public Sprite Icon;
        public int BaseHp;
        public int Speed;
        public List<SkillDefinition> SkillPool;
    }
}
