using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Hacknet.Gui;
using XMOD;

public class TXCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "TXCrack";
    public int num;
    private float y2;
    public float velocity = 2f;
    public float currentPosX;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    public TXCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        currentPosX = Bounds.Center.X;
        IdentifierName = "Tech Xeno Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(500);

        if (this.Args.Length < 2)
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
        drawOutline();
        drawTarget();
        if (lifetime >= 21.2f)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 + 3f, bounds.Center.Y + 10f), "HACK COMPLETED", GuiData.smallfont, Color.LightGray * fade);
        }
        if ((currentPosX + velocity) >= Bounds.Right - 10 || (currentPosX + velocity) <= Bounds.Left + 5)
        {
         velocity = -velocity;
        }
        currentPosX = currentPosX + velocity;
        y2 = Bounds.Center.Y - 10f;
        spriteBatch.Draw(Hacknet.Utils.white, new Vector2(currentPosX, y2), null, Color.DarkGray * fade, 0f, Vector2.Zero, 10f, SpriteEffects.None, 0f);
        if (lifetime < 21.2f)
        {
           velocity = (velocity / Math.Abs(velocity)) * (Math.Abs(velocity) + 0.01f);
        }
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            velocity = 50f;
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("tx", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
        }

    }
}