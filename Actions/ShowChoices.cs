using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using XMOD;

public class ShowChoices : Pathfinder.Action.DelayablePathfinderAction
{

    override public void Trigger(OS os)
    {
        ChoiceManager.showChoices(os);
    }
}