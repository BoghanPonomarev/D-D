using System.Collections.Generic;
using UnityEngine;

namespace Battle.Data
{
    [CreateAssetMenu(menuName = "SliceDungeon/Encounter")]
    public class EncounterDefinition : ScriptableObject
    {
        public List<UnitDefinition> Heroes;
        public List<UnitDefinition> Enemies;
    }
}
