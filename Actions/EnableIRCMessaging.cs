using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using XMOD;
using Pathfinder;
using Pathfinder.Util.XML;

public class EnableIRCMessaging : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public bool enabled;
    [XMLStorage]
    public string name;
    override public void Trigger(OS os)
    {
        XMOD.XMOD.sendIRCEnabled = enabled;
        XMOD.XMOD.sendIRCName = name;
    }
}
