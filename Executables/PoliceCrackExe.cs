using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;

public class PoliceCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "PoliceCrack";
    public int num;
    public bool exe = false;
    public PoliceCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 350;
        this.IdentifierName = "Police Protocol Crusher";
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(188);

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
private float lifetime = 0f;
    private float maxLifetime = 17f;
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        if (lifetime < (maxLifetime - 5))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X-100, Bounds.Center.Y-100), "HACKING", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime - 5) && lifetime < (maxLifetime - 2))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "HACKING.", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime - 2) && lifetime < (maxLifetime))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "HACKING..", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "HACKING...", new Color(255, 0, 0));
        }
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= 17f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("police", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}