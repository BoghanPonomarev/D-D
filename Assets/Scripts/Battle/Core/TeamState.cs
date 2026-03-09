using System.Collections.Generic;
using System.Linq;

namespace Battle.Core
{
    public class TeamState
    {
        public List<UnitState> Units;
        public bool IsDefeated => Units.All(u => !u.IsAlive);
    }
}
