using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;

public class SMTPFastCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "SMTPFastCrack";
    public int num;
    public bool exe = false;
    public SMTPFastCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 205;
        this.IdentifierName = "SMTP Sprint Crack";
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(25);

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
    private float maxLifetime = 14.2f;
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        if (lifetime < (maxLifetime - 5))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "HACKING", new Color(255, 0, 0));
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
        if (lifetime >= 14.2f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("smtp", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}