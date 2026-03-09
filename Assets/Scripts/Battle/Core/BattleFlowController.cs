using System;
using System.Collections.Generic;
using Battle.AI;
using Battle.Services;

namespace Battle.Core
{
    public class BattleFlowController
    {
        public event Action<BattleEvent> OnEvent;
        public event Action<UnitState> OnWaitingForPlayerAction;

        private readonly BattleResolver _resolver = new BattleResolver();
        private readonly SkillRollService _roller = new SkillRollService();
        private readonly InitiativeService _initiative = new InitiativeService();
        private readonly WinConditionService _winCheck = new WinConditionService();
        private readonly BattleAI _ai;

        private BattleState _state;
        private List<UnitState> _turnQueue;
        private int _turnIndex;

        public BattleFlowController(BattleAI ai)
        {
            _ai = ai;
        }

        public void StartBattle(BattleState state)
        {
            _state = state;
            _turnQueue = _initiative.BuildTurnQueue(state);
            _turnIndex = 0;
            AdvanceTurns();
        }

        public void SubmitPlayerAction(BattleAction action)
        {
            ResolveAction(action);
            if (_winCheck.Check(_state) == BattleResult.Ongoing)
                AdvanceTurns();
        }

        private void AdvanceTurns()
        {
            while (_winCheck.Check(_state) == BattleResult.Ongoing)
            {
                if (_turnIndex >= _turnQueue.Count)
                {
                    _turnQueue = _initiative.BuildTurnQueue(_state);
                    _turnIndex = 0;
                }

                var unit = _turnQueue[_turnIndex++];
                if (!unit.IsAlive) continue;

                Emit(new BattleEvent { Type = BattleEventType.TurnStarted, Source = unit });

                _roller.Roll(unit, 3);
                Emit(new BattleEvent { Type = BattleEventType.SkillsRolled, Source = unit });

                if (unit.IsHero)
                {
                    OnWaitingForPlayerAction?.Invoke(unit);
                    return;
                }

                var action = _ai.DecideAction(unit, _state);
                ResolveAction(action);
            }

            var result = _winCheck.Check(_state);
            Emit(new BattleEvent
            {
                Type = BattleEventType.BattleEnded,
                Description = result == BattleResult.HeroesWin ? "Heroes win!" : "Enemies win!"
            });
        }

        private void ResolveAction(BattleAction action)
        {
            var events = _resolver.Resolve(action, _state);
            foreach (var e in events) Emit(e);
            Emit(new BattleEvent { Type = BattleEventType.TurnEnded, Source = action.Caster });
        }

        private void Emit(BattleEvent e) => OnEvent?.Invoke(e);
    }
}
