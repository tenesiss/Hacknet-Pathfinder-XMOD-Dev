using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet.Gui;
using XMOD;

public class SocketExe : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "SocketCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    public SocketExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Socket Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    public override void LoadContent()
    {
        
        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(3500);

        if (this.Args.Length < 2)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
        }
        else if (Int32.Parse(this.Args[1]) != num || Pathfinder.Util.ComputerLookup.FindByIp(targetIP).ports.Find(p => p == 3500) == 0)
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
        if (lifetime < maxLifetime / 3)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "CRACKING", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime / 3 && lifetime < maxLifetime / 2)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "CRACKING.", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime / 2 && lifetime < maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "CRACKING..", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y), "CRACKED", GuiData.font, Color.DarkGray * fade);
        }
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("socket", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}