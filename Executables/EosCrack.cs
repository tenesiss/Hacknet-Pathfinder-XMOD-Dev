using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet;

public class EosCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EosCrack";
    public int num;
    public bool exe = false;
    public EosCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 225;
        this.IdentifierName = "Eos Crack";
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(3659);

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
        if (lifetime >= 9.5f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("eos", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}