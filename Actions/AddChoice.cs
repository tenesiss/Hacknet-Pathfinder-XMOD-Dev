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

public class AddChoice : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public string id;
    [XMLStorage]
    public string title;
    [XMLStorage]
    public string description = "";
    [XMLStorage]
    public bool resetAfterChoose = true;

    override public void Trigger(OS os)
    {
        if(ChoiceManager.choices.Any(choice => choice.id == id))
        {
            Error err = new Error("There is already a choice with id '" + id + "'.", false, 2);
        } else
        {
            Choice c = new Choice(id, title, description, resetAfterChoose);
            ChoiceManager.choices.Add(c);
        }
    }
}