using System.Collections.Generic;
using Battle.Core;
using UnityEngine;

namespace Presentation
{
    public class FormationLayout : MonoBehaviour
    {
        [SerializeField] private Transform[] heroSlots = new Transform[4];
        [SerializeField] private Transform[] enemySlots = new Transform[4];

        private readonly Dictionary<string, GameObject> _unitViews = new();

        public GameObject PlaceUnit(UnitState state, GameObject prefab)
        {
            var slot = GetSlot(state.SlotIndex, state.IsHero);
            var go = Instantiate(prefab, slot.position, slot.rotation);
            _unitViews[state.Id] = go;
            return go;
        }

        public Vector3 GetSlotPosition(int index, bool isHero)
        {
            return GetSlot(index, isHero).position;
        }

        private Transform GetSlot(int index, bool isHero)
        {
            return isHero ? heroSlots[index] : enemySlots[index];
        }
    }
}
