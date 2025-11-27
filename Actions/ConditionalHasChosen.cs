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
    public bool checkOnce = false;
    [XMLStorage]
    public bool checkUntilChoice = true;
    private double _t = OS.currentElapsedTime;

    public override bool Check(object os_obj)
    {
        OS finalOS = (OS)os_obj;
        if (checkOnce)
        {
            finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
        }
        if (checkUntilChoice && (_t - XMOD.XMOD.lastChoiceTime) < 0)
        {
            finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
        }
        if(XMOD.XMOD.lastChoice != null)
        {
            return XMOD.XMOD.lastChoice.id == choiceId;
        }
        return false;
    }
}
