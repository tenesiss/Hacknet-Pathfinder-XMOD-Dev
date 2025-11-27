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

public class RemoveChoice : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public string id;

    override public void Trigger(OS os)
    {
        ChoiceManager.choices.RemoveAll(c => c.id == id);
    }
}