using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using Hacknet;

public class ConditionFileDeletion : Pathfinder.Action.PathfinderCondition
    {
        [XMLStorage]
        public string target;
    [XMLStorage]
    public string targetFile;
    [XMLStorage]
    public string path;
    [XMLStorage]
    public string checkOnce = "false";

    public override bool Check(object os_obj)
    {
        Computer compTarget = Pathfinder.Util.ComputerLookup.FindById(target);
        OS finalOS = (OS)os_obj;
        if (compTarget.getFolderFromPath(path) != null)
        {
            if (!compTarget.getFolderFromPath(path).containsFile(targetFile))
            {
                return true;
            } else
            {
                if (checkOnce == "true")
                {
                    finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                }
                return false;
            }
        } else
        {
            return true;
        }
    }
}
