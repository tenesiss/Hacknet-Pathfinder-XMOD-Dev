using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using XMOD;
using Pathfinder;
using Pathfinder.Util.XML;

public class RemovePoints : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public int amount;
    [XMLStorage]
    public string placeholder;
    override public void Trigger(OS os)
    {
        if (PointsManager.GetPlaceholder(placeholder) != null)
        {
            PointsPlaceholder placeholderGT = PointsManager.GetPlaceholder(placeholder);
            placeholderGT.SubstractPoints(amount);
        }
        else
        {
            new PointsPlaceholder(placeholder, amount);
        }
    }
}
