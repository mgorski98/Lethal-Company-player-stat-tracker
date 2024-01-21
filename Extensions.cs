using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalCompanyStatTracker {
    public static class Extensions {
        public static bool IsCompanyBuilding(this SelectableLevel level) => level.PlanetName == "71 Gordion";
    }
}
