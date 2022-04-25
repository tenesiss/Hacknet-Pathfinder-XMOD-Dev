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
    public string amount;
    [XMLStorage]
    public string placeholder;
    override public void Trigger(OS os)
    {
        if (PointsManager.GetPlaceholder(placeholder) != null)
        {
            PointsPlaceholder placeholderGT = PointsManager.GetPlaceholder(placeholder);
            int ResultPoints = placeholderGT.points - Int32.Parse(amount);
            if(ResultPoints < 0)
            {
                Console.WriteLine("Points can't be negative!!");
            } else
            {
                placeholderGT.SubstractPoints(Int32.Parse(amount));
            }
            
        }
        else
        {
            Console.WriteLine("Points can't be negative!!");
        }
    }
}
