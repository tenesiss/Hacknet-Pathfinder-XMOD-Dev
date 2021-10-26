using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;

public class EnBreak : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EnBreak";
    public int num;
    public bool exe = false;
    public EnBreak(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 151;
        this.IdentifierName = "En Breaker";
    }
    private int x = 0;
    public override void LoadContent()
    {
        base.LoadContent();
        if(Programs.getComputer(os, targetIP).portsNeededForCrack < 50)
        {
            needsRemoval = true;
        }
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "HACKING...", new Color(255, 0, 0));
    }

    private float lifetime = 0f;
    public override void Completed()
    {

    }
    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= 10f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).giveAdmin(targetIP);
            os.write("En Breaker Finished!");
            os.write("Admin password: " + Programs.getComputer(os, targetIP).adminPass);
        } else
        {
            lifetime += t;
        }

    }
}