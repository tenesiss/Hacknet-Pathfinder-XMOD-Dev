using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet.Gui;
using Microsoft.Xna.Framework.Graphics;
using XMOD;

public class EnBreak : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EnBreak";
    public int num;
    private float velocity;
    private float currentPosX;
    private float maxLifetime;
    private float lifetime = 0f;
    private float y2;
    public bool exe = false;
    public EnBreak(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        currentPosX = Bounds.Center.X;
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "En Breaker";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
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

    
    public override void Completed()
    {

    }
    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            velocity = 50f;
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