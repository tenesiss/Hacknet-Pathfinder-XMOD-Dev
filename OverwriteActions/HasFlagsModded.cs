using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathfinder;
using XMOD;
using Hacknet;
using System.Threading.Tasks;
using System.Xml.Linq;
using Pathfinder.Util;
using Pathfinder.Util.XML;

public class HasFlagsNew : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string requiredFlags;

    // Custom attribute that you are adding to the class.
    [XMLStorage]
    public string checkOnce = "false";

    // Copied method from SCHasFlags. You could also use a reverse patch to copy the code for you.
    public override bool Check(object os_obj)
    {
        OS oS = (OS)os_obj;
        if (!string.IsNullOrWhiteSpace(requiredFlags))
        {
            string[] array = requiredFlags.Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                if (!oS.Flags.HasFlag(array[i]))
                {
                    if (checkOnce == "true")
                    {
                        oS.delayer.Post(ActionDelayer.NextTick(), () => { oS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
            }
        }
        return true;
    }
}
