using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using XMOD;
using Pathfinder;
using Pathfinder.Util.XML;
using System.Xml.Linq;
using Hacknet.Extensions;

public class LoadMissionX : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public string MissionName;
    override public void Trigger(OS os)
    {
        ParalellMissionManager._addMission(Reader.ReadMission(XDocument.Load(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + MissionName)));
    }
}
