using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;

    public class CmdRunAction : Pathfinder.Action.DelayablePathfinderAction
    {
    [XMLStorage]
    public string Command;

    override public void Trigger(OS os)
    {
        os.runCommand((String)Command);
    }
}
