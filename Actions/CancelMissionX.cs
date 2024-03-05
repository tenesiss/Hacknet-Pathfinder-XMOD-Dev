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

public class CancelMissionX : Pathfinder.Action.DelayablePathfinderAction
{
	[XMLStorage]
	public string MissionIdentifier;
	override public void Trigger(OS os)
	{
		ParalellMissionManager._removeMission(MissionIdentifier);
	}
}
