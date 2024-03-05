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

public class DoesNotHaveFlagsNew : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string Flags;

    [XMLStorage]
    public string checkOnce = "false";


    public override bool Check(object os_obj)
    {
        OS oS = (OS)os_obj;
        if (!string.IsNullOrWhiteSpace(Flags))
        {
            string[] array = Flags.Split(Utils.commaDelim, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                if (oS.Flags.HasFlag(array[i]))
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
