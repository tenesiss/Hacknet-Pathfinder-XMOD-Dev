using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using Hacknet;

public class ConditionalHasChosen : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string choiceId;
    [XMLStorage]
    public string checkOnce = "false";

    public override bool Check(object os_obj)
    {
        OS finalOS = (OS) os_obj;
        if (checkOnce == "true")
        {
            finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
        }
        return XMOD.XMOD.lastChoice.id == choiceId;
    }
}
