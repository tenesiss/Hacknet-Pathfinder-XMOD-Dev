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

    public override bool Check(object os_obj)
    {
        Computer compTarget = Pathfinder.Util.ComputerLookup.FindById(target);
        if(compTarget.getFolderFromPath(path) != null)
        {
            if (!compTarget.getFolderFromPath(path).containsFile(targetFile))
            {
                return true;
            } else
            {
                return false;
            }
        } else
        {
            return true;
        }
    }
}
