using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class DecisionManager
    {
        public static Decision activeDecision = null;

        public static void SetMainDecision(Decision newDecision)
        {
            activeDecision = newDecision;
        }
    }
}
