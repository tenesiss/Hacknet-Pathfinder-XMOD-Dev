using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;

public class ShadingCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "ShadingCrack";
    public int num;
    public bool exe = false;
    public ShadingCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 200;
        this.IdentifierName = "Shading Crack";
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(591);

        if (this.Args.Length < 1)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
        }
        else if (Int32.Parse(this.Args[1]) != num)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
        }
        Programs.getComputer(os, targetIP).hostileActionTaken();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X, Bounds.Center.Y), "HACKING...", new Color(255, 0, 0));
    }

    private float lifetime = 0f;

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= 16.5f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("shading", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}