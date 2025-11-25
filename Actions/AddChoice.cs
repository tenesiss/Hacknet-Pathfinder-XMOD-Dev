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
    public string resetAfterChoose = "true";

    override public void Trigger(OS os)
    {
        bool rac;
        if(resetAfterChoose == "true")
        {
            rac = true;
        } else if(resetAfterChoose == "false")
        {
            rac = false;
        } else
        {
            Error err = new Error("Invalid value for \"resetAfterChoose\" attribute at choice: " + id + ". Default value will be used (true)", false, 2);
            err.Emit();
            rac = true;
        }
        Choice c = new Choice(id, title, description, rac);
        ChoiceManager.choices.Add(c);
    }
}