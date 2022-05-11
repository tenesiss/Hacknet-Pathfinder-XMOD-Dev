using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class DecisionOption
    {
        public string id;
        public string flagToAdd;

        public void Trigger()
        {
            Pathfinder.Util.ComputerLookup.FindById("playerComp").os.Flags.AddFlag(flagToAdd);
        }
    }
}
