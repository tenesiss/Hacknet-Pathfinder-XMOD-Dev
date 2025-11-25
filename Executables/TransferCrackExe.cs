using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet.Gui;
using XMOD;

public class TransferExe : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "TransferCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: moving data blocks
    private struct Packet { public Vector2 Pos; public float Speed; public float Size; public float Phase; }
    private readonly System.Collections.Generic.List<Packet> packets = new System.Collections.Generic.List<Packet>(64);
    private float spawnAcc = 0f;
    private readonly Random rng = new Random();
    public TransferExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Transfer Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(211);

        if (this.Args.Length < 2)
        {
            os.write("No port number Provided");
            os.write("Execution failed");
            needsRemoval = true;
        } else if (Int32.Parse(this.Args[1]) != num)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
        }
        Programs.getComputer(os, targetIP).hostileActionTaken();
        packets.Clear();
    }
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));

        // Path line across the center
        float y = Bounds.Center.Y;
        var pathCol = new Color(0, 220, 255) * (0.25f * fade);
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(Bounds.X + 6, (int)y, Bounds.Width - 12, 2), pathCol);

        // Draw packets
        for (int i = 0; i < packets.Count; i++)
        {
            var pk = packets[i];
            Color c = Color.Lerp(new Color(0, 255, 220), new Color(255, 60, 100), (float)Math.Sin(pk.Phase + lifetime * 2f) * 0.5f + 0.5f) * (0.7f * fade);
            float sz = pk.Size;
            // Body block
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)(pk.Pos.X - sz), (int)(y - sz), (int)(sz * 2f), (int)(sz * 2f)), c * 0.25f);
            // Outline
            DrawRectOutline(new Rectangle((int)(pk.Pos.X - sz), (int)(y - sz), (int)(sz * 2f), (int)(sz * 2f)), c, 1);
            // Trailing line
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)(pk.Pos.X - sz - 14), (int)y - 1, 14, 2), c * 0.6f);
        }

        // Status
        string status = p >= 1f ? "CRACKED" : "CRACKING";
        TextItem.doFontLabel(new Vector2(Bounds.X + 8f, Bounds.Bottom - 22f), status, GuiData.smallfont, Color.DarkGray * fade);
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("transfer", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            // spawn along left side
            float rate = 10f + 20f * (1f - Math.Max(0f, Math.Min(1f, lifetime / Math.Max(0.0001f, maxLifetime))));
            spawnAcc += rate * t;
            int toSpawn = (int)spawnAcc;
            if (toSpawn > 0) spawnAcc -= toSpawn;
            for (int i = 0; i < toSpawn; i++)
            {
                var pk = new Packet
                {
                    Pos = new Vector2(Bounds.X + 8, Bounds.Center.Y + (float)(rng.NextDouble() * 6.0 - 3.0)),
                    Speed = 90f + (float)rng.NextDouble() * 180f,
                    Size = 4f + (float)rng.NextDouble() * 6f,
                    Phase = (float)rng.NextDouble() * MathHelper.TwoPi
                };
                packets.Add(pk);
            }
            for (int i = packets.Count - 1; i >= 0; i--)
            {
                var pk = packets[i];
                pk.Pos.X += pk.Speed * t;
                if (pk.Pos.X > Bounds.Right - 8) packets.RemoveAt(i); else packets[i] = pk;
            }
        }

    }

    private void DrawRectOutline(Rectangle r, Color c, int th)
    {
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(r.X, r.Y, r.Width, th), c);
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(r.X, r.Bottom - th, r.Width, th), c);
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(r.X, r.Y, th, r.Height), c);
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(r.Right - th, r.Y, th, r.Height), c);
    }
}
