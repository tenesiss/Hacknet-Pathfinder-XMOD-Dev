using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet.Gui;
using XMOD;

public class FirewallDefacer : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "FirewallDefacer";
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    public FirewallDefacer(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Firewall Defacer";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
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
        if (lifetime < maxLifetime/3)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "DEFACING", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime/3 && lifetime < maxLifetime/2)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "DEFACING.", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime/2 && lifetime < maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "DEFACING..", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "DEFACED", GuiData.font, Color.DarkGray * fade);
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y + 30f), "Password: "+os.connectedComp.firewall.solution, GuiData.smallfont, Color.DarkGray * fade);
        }
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
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