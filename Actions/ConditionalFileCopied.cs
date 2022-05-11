using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using Hacknet;

public class ConditionalFileCopied : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string target;
    [XMLStorage]
    public string targetFile;
    [XMLStorage]
    public string path;
    [XMLStorage]
    public string source;
    [XMLStorage]
    public string sourcePath;
    [XMLStorage]
    public string checkOnce = "false";

    public override bool Check(object os_obj)
    {
        // Declaration
        OS finalOS = (OS)os_obj;
        Computer compTarget = Pathfinder.Util.ComputerLookup.FindById(target);
        Computer compSource = Pathfinder.Util.ComputerLookup.FindById(source);
        FileEntry baseFile = null;
        bool targetValidPath = false;
        bool sourceValidPath = false;
        bool validFile = false;

        // Be sure comps exist
        if(compTarget == null)
        {
            finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
            return false;
        }
        if(compSource == null)
        {
            finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
            return false;
        }

        // Check if valid paths and files
        if (compSource.getFolderFromPath(path) != null)
        {
            sourceValidPath = true;
        }
        if (compTarget.getFolderFromPath(path) != null)
        {
            targetValidPath = true;
        }
        if (compSource.getFolderFromPath(sourcePath).searchForFile(targetFile) != null)
        {
            validFile = true;
            baseFile = compSource.getFolderFromPath(sourcePath).searchForFile(targetFile);
        }


        // Main
        if (targetValidPath & sourceValidPath)
        {
            if (validFile)
            {
                FileEntry fileInTarget = compTarget.getFolderFromPath(path).searchForFile(targetFile);
                if (fileInTarget != null & baseFile != null)
                {
                    if(compTarget.getFolderFromPath(path).searchForFile(targetFile).data == baseFile.data)
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
                    if (checkOnce == "true")
                    {
                        finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                    }
                    return false;
                }
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
            if (checkOnce == "true")
            {
                finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
            }
            return false;
        }
    }
}
