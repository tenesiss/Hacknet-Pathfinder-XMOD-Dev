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

public class SMTPFastCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "SMTPFastCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: fast envelopes
    private struct Env { public Vector2 Pos; public Vector2 Vel; public float Rot; public float Size; public float Life; }
    private readonly System.Collections.Generic.List<Env> envs = new System.Collections.Generic.List<Env>(64);
    private float spawnAcc = 0f;
    private readonly Random rng = new Random();
    private readonly int marginLeft = 8, marginRight = 8, marginTop = 20, marginBottom = 28; // avoid status text/top
    public SMTPFastCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "SMTP Sprint Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(25);

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
        envs.Clear();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));
        var inner = ComputeInnerRect();
        // Draw envelopes as simple diamonds with flap
        for (int i = 0; i < envs.Count; i++)
        {
            var e = envs[i];
            if (!inner.Contains(new Point((int)e.Pos.X, (int)e.Pos.Y))) continue; // skip anything out of area
            DrawEnvelope(e.Pos, 0f, e.Size, new Color(255, 255, 255) * (0.7f * fade));
            // speed lines (clamped inside inner)
            float backX = e.Pos.X - (e.Size * 10f);
            int sx = (int)Math.Max(backX, inner.X);
            int sw = (int)Math.Max(0, Math.Min(16, inner.Right - sx));
            if (sw > 0)
                spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(sx, (int)e.Pos.Y, sw, 2), new Color(0, 255, 220) * (0.5f * fade));
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
            Programs.getComputer(os, targetIP).openPort("smtp", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            var inner = ComputeInnerRect();
            // spawn envelopes faster at start
            float rate = 12f + 24f * (1f - Math.Max(0f, Math.Min(1f, lifetime / Math.Max(0.0001f, maxLifetime))));
            spawnAcc += rate * t;
            int toSpawn = (int)spawnAcc;
            if (toSpawn > 0) spawnAcc -= toSpawn;
            for (int i = 0; i < toSpawn; i++)
            {
                // spawn just inside left margin at a safe vertical band
                float y = inner.Y + 4 + rng.Next(Math.Max(1, inner.Height - 8));
                Vector2 pos = new Vector2(inner.X - 6f, y);
                float speed = 140f + (float)rng.NextDouble() * 220f;
                Vector2 vel = new Vector2(speed, 0f); // purely horizontal
                envs.Add(new Env { Pos = pos, Vel = vel, Rot = 0f, Size = 0.6f + (float)rng.NextDouble() * 0.6f, Life = 4f });
            }
            // update
            for (int i = envs.Count - 1; i >= 0; i--)
            {
                var e = envs[i];
                e.Pos += e.Vel * t;
                e.Life -= t;
                // keep inside inner rect horizontally; remove when out the right side or life ends
                if (e.Pos.X > inner.Right + 8 || e.Life <= 0f) envs.RemoveAt(i); else envs[i] = e;
            }
        }

    }

    private Rectangle ComputeInnerRect()
    {
        int x = Bounds.X + marginLeft;
        int y = Bounds.Y + marginTop;
        int w = Math.Max(1, Bounds.Width - marginLeft - marginRight);
        int h = Math.Max(1, Bounds.Height - marginTop - marginBottom);
        return new Rectangle(x, y, w, h);
    }

    private void DrawEnvelope(Vector2 center, float rot, float scale, Color col)
    {
        float w = 18f * scale;
        float h = 12f * scale;
        // Body
        var rect = new Rectangle((int)(center.X - w / 2f), (int)(center.Y - h / 2f), (int)w, (int)h);
        spriteBatch.Draw(Hacknet.Utils.white, rect, col * 0.25f);
        // Outline
        DrawLine(new Vector2(rect.Left, rect.Top), new Vector2(rect.Right, rect.Top), col, 1f);
        DrawLine(new Vector2(rect.Right, rect.Top), new Vector2(rect.Right, rect.Bottom), col, 1f);
        DrawLine(new Vector2(rect.Right, rect.Bottom), new Vector2(rect.Left, rect.Bottom), col, 1f);
        DrawLine(new Vector2(rect.Left, rect.Bottom), new Vector2(rect.Left, rect.Top), col, 1f);
        // Flap (X)
        DrawLine(new Vector2(rect.Left, rect.Top), new Vector2(rect.Center.X, rect.Center.Y), col, 1f);
        DrawLine(new Vector2(rect.Right, rect.Top), new Vector2(rect.Center.X, rect.Center.Y), col, 1f);
    }

    private void DrawLine(Vector2 a, Vector2 b, Color col, float thickness)
    {
        Vector2 d = b - a;
        float len = d.Length();
        if (len < 0.0001f) return;
        float r = (float)Math.Atan2(d.Y, d.X);
        spriteBatch.Draw(Hacknet.Utils.white, a, null, col, r, Vector2.Zero, new Vector2(len, thickness), SpriteEffects.None, 0f);
    }
}
