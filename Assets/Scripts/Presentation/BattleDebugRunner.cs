using System.Collections.Generic;
using Battle.AI;
using Battle.Bootstrap;
using Battle.Core;
using Battle.Data;
using Battle.Effects;
using Battle.Services;
using UnityEngine;

namespace Presentation
{
    public class BattleDebugRunner : MonoBehaviour
    {
        private BattleState _state;

        private void Start()
        {
            var encounter = BuildEncounter();
            _state = new BattleFactory().Create(encounter);

            var targeting = new TargetingService();
            var ai = new BattleAI(targeting);
            var controller = new BattleFlowController(ai);

            controller.OnEvent += LogEvent;
            controller.OnWaitingForPlayerAction += unit =>
            {
                var action = ai.DecideAction(unit, _state);
                controller.SubmitPlayerAction(action);
            };

            controller.StartBattle(_state);
        }

        private EncounterDefinition BuildEncounter()
        {
            var strike = CreateSkill("Strike", TargetType.SingleEnemy, CreateDamageEffect(6));
            var heavyStrike = CreateSkill("Heavy Strike", TargetType.SingleEnemy, CreateDamageEffect(12));
            var bandage = CreateSkill("Bandage", TargetType.Self, CreateHealEffect(8));

            var hero = CreateUnit("Knight", baseHp: 30, speed: 5, strike, heavyStrike, bandage);
            var rogue = CreateUnit("Rogue", baseHp: 24, speed: 8, strike, strike, heavyStrike);
            var goblin = CreateUnit("Goblin", baseHp: 18, speed: 4, strike, strike, heavyStrike);
            var orc = CreateUnit("Orc", baseHp: 28, speed: 2, heavyStrike, heavyStrike, strike);

            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.Heroes = new List<UnitDefinition> { hero, rogue };
            encounter.Enemies = new List<UnitDefinition> { goblin, orc };
            return encounter;
        }

        private void LogEvent(BattleEvent e)
        {
            var msg = e.Type switch
            {
                BattleEventType.TurnStarted  => $"\n--- {e.Source.Definition.UnitName}'s turn ---",
                BattleEventType.DamageDealt  => $"  {e.Source.Definition.UnitName} hits {e.Target.Definition.UnitName} for {e.Value} damage (HP: {e.Target.CurrentHp})",
                BattleEventType.HealApplied  => $"  {e.Source.Definition.UnitName} heals {e.Target.Definition.UnitName} for {e.Value} HP (HP: {e.Target.CurrentHp})",
                BattleEventType.UnitDied     => $"  ✗ {e.Target.Definition.UnitName} has died!",
                BattleEventType.BattleEnded  => $"\n=== {e.Description} ===",
                _                            => null
            };

            if (msg != null) Debug.Log(msg);
        }

        private static SkillDefinition CreateSkill(string skillName, TargetType targetType, params EffectDefinition[] effects)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.SkillName = skillName;
            skill.TargetType = targetType;
            skill.Effects = new List<EffectDefinition>(effects);
            return skill;
        }

        private static UnitDefinition CreateUnit(string unitName, int baseHp, int speed, params SkillDefinition[] skills)
        {
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.UnitName = unitName;
            unit.BaseHp = baseHp;
            unit.Speed = speed;
            unit.SkillPool = new List<SkillDefinition>(skills);
            return unit;
        }

        private static DamageEffect CreateDamageEffect(int damage)
        {
            var effect = ScriptableObject.CreateInstance<DamageEffect>();
            effect.BaseDamage = damage;
            return effect;
        }

        private static HealEffect CreateHealEffect(int amount)
        {
            var effect = ScriptableObject.CreateInstance<HealEffect>();
            effect.HealAmount = amount;
            return effect;
        }
    }
}
