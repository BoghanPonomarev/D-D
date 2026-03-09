using System.Collections.Generic;

namespace Battle.Core
{
    public enum BattleEventType
    {
        DamageDealt,
        HealApplied,
        UnitDied,
        TurnStarted,
        TurnEnded,
        BattleEnded,
        SkillsRolled,
    }

    public class BattleEvent
    {
        public BattleEventType Type;
        public UnitState Source;
        public UnitState Target;
        public int Value;
        public string Description;
    }
}
