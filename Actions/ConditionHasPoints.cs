using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using Hacknet;
using XMOD;
using System;

public class ConditionHasPoints : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string placeholder;
    [XMLStorage]
    public string has;
    [XMLStorage]
    public string checkOnce = "false";

    public override bool Check(object os_obj)
    {
        if(PointsManager.GetPlaceholder(placeholder) != null)
        {
            PointsPlaceholder placeholderMT = PointsManager.GetPlaceholder(placeholder);
            OS finalOS = (OS)os_obj;
            if (has.StartsWith(">") && !has.StartsWith(">="))
            {
                int finalNumD = Int32.Parse(has.Substring(1));
                if (placeholderMT.points > finalNumD)
                {
                    return true;
                }
                else
                {
                    if (checkOnce == "true")
                    {
                        finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
            }
            else if (has.StartsWith("-") && !has.StartsWith("<="))
            {
                int finalNumD = Int32.Parse(has.Substring(1));
                if (placeholderMT.points < finalNumD)
                {
                    return true;
                }
                else
                {
                    if (checkOnce == "true")
                    {
                        finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
            }
            else if (has.StartsWith(">="))
            {
                Console.WriteLine(has);
                int finalNumD = Int32.Parse(has.Substring(2));
                if (placeholderMT.points >= finalNumD)
                {
                    return true;
                }
                else
                {
                    if (checkOnce == "true")
                    {
                        finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
            } else if(has.StartsWith("<="))
            {
                int finalNumD = Int32.Parse(has.Substring(2));
                if (placeholderMT.points <= finalNumD)
                {
                    return true;
                }
                else
                {
                    if (checkOnce == "true")
                    {
                        finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
            }
            else
            {
                Console.WriteLine("ERROR -- FORMAT ERROR -- In <HasPoints... - Introduce a valid HAS attribute value (>10, <5, ...)");
                return false;
            }
        } else
        {
            OS finalOS = (OS)os_obj;
            if (checkOnce == "true")
            {
                finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
            }
            return false;
        }
    }
}
