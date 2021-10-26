using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;

public class FirewallDefacer : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "FirewallDefacer";
    public bool exe = false;
    public FirewallDefacer(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 300;
        this.IdentifierName = "Firewall Defacer";
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();

        if (os.connectedComp.firewall == null)
        {
            os.write("No firewall detected!");
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
        if (lifetime >= 10.3f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).firewall.solved = true;
        }
        else
        {
            lifetime += t;
        }

    }
}