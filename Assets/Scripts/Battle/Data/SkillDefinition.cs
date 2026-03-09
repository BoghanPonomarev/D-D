using System.Collections.Generic;
using Battle.Effects;
using UnityEngine;

namespace Battle.Data
{
    public enum TargetType
    {
        SingleEnemy,
        SingleAlly,
        Self,
        AllEnemies,
        AllAllies,
    }

    [CreateAssetMenu(menuName = "SliceDungeon/Skill")]
    public class SkillDefinition : ScriptableObject
    {
        public string SkillName;
        public Sprite Icon;
        [TextArea] public string Description;
        public TargetType TargetType;
        public List<EffectDefinition> Effects;
    }
}
