using System.Collections.Generic;
using Battle.AI;
using Battle.Bootstrap;
using Battle.Core;
using Battle.Data;
using Battle.Services;
using TMPro;
using UnityEngine;

namespace Presentation
{
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private EncounterDefinition encounter;
        [SerializeField] private FormationLayout formation;
        [SerializeField] private GameObject unitViewPrefab;
        [SerializeField] private TextMeshProUGUI resultLabel;
        [SerializeField] private SkillButtonPanel skillButtonPanel;

        private BattleState _state;
        private BattleFlowController _controller;
        private TargetingService _targeting;
        private readonly Dictionary<string, UnitView> _views = new();

        private void Start()
        {
            _state = new BattleFactory().Create(encounter);

            SpawnUnits(_state.Heroes.Units);
            SpawnUnits(_state.Enemies.Units);

            _targeting = new TargetingService();
            _controller = new BattleFlowController(new BattleAI(_targeting));

            _controller.OnEvent += HandleEvent;
            _controller.OnWaitingForPlayerAction += OnPlayerTurn;

            resultLabel.gameObject.SetActive(false);
            skillButtonPanel.Hide();

            _controller.StartBattle(_state);
        }

        private void SpawnUnits(List<UnitState> units)
        {
            foreach (var unit in units)
            {
                var go = formation.PlaceUnit(unit, unitViewPrefab);
                var view = go.GetComponent<UnitView>();
                view.Initialize(unit);
                _views[unit.Id] = view;
            }
        }

        private void OnPlayerTurn(UnitState unit)
        {
            skillButtonPanel.Show(unit.CurrentSkills, skill => OnSkillSelected(unit, skill));
        }

        private void OnSkillSelected(UnitState caster, SkillDefinition skill)
        {
            skillButtonPanel.Hide();

            var validTargets = _targeting.GetValidTargets(skill, caster, _state);

            if (skill.TargetType == TargetType.AllEnemies || skill.TargetType == TargetType.AllAllies)
            {
                SubmitAction(caster, skill, validTargets);
                return;
            }

            foreach (var target in validTargets)
            {
                var captured = target;
                _views[target.Id].SetTargetable(true, _ =>
                {
                    foreach (var t in validTargets)
                        _views[t.Id].SetTargetable(false);
                    SubmitAction(caster, skill, new List<UnitState> { captured });
                });
            }
        }

        private void SubmitAction(UnitState caster, SkillDefinition skill, List<UnitState> targets)
        {
            _controller.SubmitPlayerAction(new BattleAction { Caster = caster, Skill = skill, Targets = targets });
        }

        private void HandleEvent(BattleEvent e)
        {
            switch (e.Type)
            {
                case BattleEventType.DamageDealt:
                    _views[e.Target.Id].UpdateHp(e.Target.CurrentHp, e.Target.Definition.BaseHp);
                    break;
                case BattleEventType.HealApplied:
                    _views[e.Target.Id].UpdateHp(e.Target.CurrentHp, e.Target.Definition.BaseHp);
                    break;
                case BattleEventType.UnitDied:
                    _views[e.Target.Id].PlayDeathAnimation();
                    break;
                case BattleEventType.BattleEnded:
                    resultLabel.text = e.Description;
                    resultLabel.gameObject.SetActive(true);
                    break;
            }
        }
    }
}
