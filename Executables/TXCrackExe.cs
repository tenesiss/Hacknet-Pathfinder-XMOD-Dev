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
    // Visuals: hex grid pulse
    private struct Hex { public Vector2 C; public float R; public float Phase; public float OffTime; }
    private System.Collections.Generic.List<Hex> hexes = new System.Collections.Generic.List<Hex>();
    private float anim = 0f;
    private Microsoft.Xna.Framework.Rectangle lastBounds;
    private readonly int marginLeft = 10, marginRight = 10, marginTop = 28, marginBottom = 30;
    private Microsoft.Xna.Framework.Rectangle innerRect;
    private Microsoft.Xna.Framework.Rectangle gridRect;
    public TXCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        currentPosX = Bounds.Center.X;
        IdentifierName = "Tech Xeno Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    private int offSeed = 1337;
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
        lastBounds = this.Bounds;
        BuildHexGrid(lastBounds);
    }
    public override void Draw(float t)
    {
        base.Draw(t);
        drawOutline();
        drawTarget();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));
        float ease = p * p * (3f - 2f * p);

        // Draw hexes
        // Global brightness decays to 0 exactly at completion (no forced black)
        float globalBright = 1f - Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));
        for (int i = 0; i < hexes.Count; i++)
        {
            var h = hexes[i];
            float pulse = (float)(Math.Sin(anim * 2.2f + h.Phase) * 0.5 + 0.5);
            Color c = Color.Lerp(new Color(50, 120, 160), new Color(0, 255, 220), pulse) * ((0.5f + 0.5f * ease) * Math.Max(0f, globalBright) * fade);
            DrawHex(h.C, h.R, c, 2f);
        }

        // Central X pulse emblem centered on the hex grid area
        Vector2 center = (gridRect.Width > 0 && gridRect.Height > 0)
            ? new Vector2(gridRect.Center.X, gridRect.Center.Y)
            : new Vector2(Bounds.Center.X, Bounds.Center.Y);
        float baseSpan = (gridRect.Width > 0 && gridRect.Height > 0)
            ? Math.Min(gridRect.Width, gridRect.Height)
            : Math.Min(Bounds.Width, Bounds.Height);
        float s = baseSpan * 0.35f * (0.9f + 0.1f * (float)Math.Sin(anim * 4.2f));
        // Clamp X size so endpoints remain inside the grid area
        if (gridRect.Width > 0 && gridRect.Height > 0)
        {
            float smax = Math.Min(
                Math.Min(center.X - gridRect.Left, gridRect.Right - center.X),
                Math.Min(center.Y - gridRect.Top, gridRect.Bottom - center.Y)
            );
            s = Math.Min(s, Math.Max(2f, smax - 3f));
        }
        Color xc = Color.Lerp(new Color(0, 200, 255), new Color(255, 60, 80), (float)(Math.Sin(anim * 1.6f) * 0.5 + 0.5)) * (0.65f * fade);
        DrawLine(center + new Vector2(-s, -s), center + new Vector2(s, s), xc, 3f);
        DrawLine(center + new Vector2(-s, s), center + new Vector2(s, -s), xc, 3f);

        // Status
        string status = p >= 1f ? "CRACKED" : "CRACKING";
        TextItem.doFontLabel(new Vector2(Bounds.X + 8f, Bounds.Bottom - 22f), status, GuiData.smallfont, Color.LightGray * fade);
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        // Rebuild grid if bounds changed so layout stays centered and respects margins
        if (this.Bounds.X != lastBounds.X || this.Bounds.Y != lastBounds.Y || this.Bounds.Width != lastBounds.Width || this.Bounds.Height != lastBounds.Height)
        {
            lastBounds = this.Bounds;
            BuildHexGrid(lastBounds);
        }
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("tx", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            anim += t;
        }

    }

    private void BuildHexGrid(Microsoft.Xna.Framework.Rectangle rect)
    {
        hexes.Clear();
        // Inner area with margins to avoid top/status
        var inner = new Microsoft.Xna.Framework.Rectangle(
            rect.X + marginLeft,
            rect.Y + marginTop,
            Math.Max(8, rect.Width - marginLeft - marginRight),
            Math.Max(8, rect.Height - marginTop - marginBottom)
        );
        innerRect = inner;

        float r = 14f; // hex radius
        float w = r * 2f;
        float h = (float)(Math.Sqrt(3) * r);
        float stepX = w * 0.75f;
        float stepY = h * 0.85f;

        int cols = Math.Max(1, (int)Math.Floor((inner.Width - w) / stepX) + 1);
        int rows = Math.Max(1, (int)Math.Floor((inner.Height - h) / stepY) + 1);

        float gridW = (cols - 1) * stepX + w;
        float gridH = (rows - 1) * stepY + h;

        // Center grid around the X (center of rect). Account for staggered rows (odd rows shifted right)
        var center = new Vector2(rect.Center.X, rect.Center.Y);
        int oddRows = rows / 2; // number of rows with stagger (every second row)
        float stagger = r * 0.75f;
        float avgStaggerOffset = rows > 1 ? (oddRows * stagger) / Math.Max(1, rows) : 0f;
        float desiredStartX = center.X - ((cols - 1) * stepX) / 2f - avgStaggerOffset; // center average of hex centers on X
        float startX = MathHelper.Clamp(desiredStartX, inner.X, inner.Right - gridW);
        float startY = MathHelper.Clamp(center.Y - gridH / 2f, inner.Y, inner.Bottom - gridH);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float cx = startX + x * stepX + ((y % 2 == 0) ? 0 : r * 0.75f);
                float cy = startY + y * stepY;
                if (cx >= inner.X && cx <= inner.Right && cy >= inner.Y && cy <= inner.Bottom)
                    hexes.Add(new Hex { C = new Vector2(cx, cy), R = r, Phase = (x + y * 0.7f) * 0.4f, OffTime = 0f });
            }
        }
        // Per-cell schedule removed: brightness now decays uniformly over time
        // Compute grid bounding rectangle from hex centers including radius
        if (hexes.Count > 0)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < hexes.Count; i++)
            {
                var hcx = hexes[i].C.X;
                var hcy = hexes[i].C.Y;
                minX = Math.Min(minX, hcx - r);
                maxX = Math.Max(maxX, hcx + r);
                minY = Math.Min(minY, hcy - r);
                maxY = Math.Max(maxY, hcy + r);
            }
            gridRect = new Microsoft.Xna.Framework.Rectangle((int)Math.Floor(minX), (int)Math.Floor(minY), (int)Math.Ceiling(maxX - minX), (int)Math.Ceiling(maxY - minY));
        }
        else
        {
            gridRect = inner;
        }
    }

    private void DrawHex(Vector2 c, float r, Color col, float thickness)
    {
        Vector2 prev = Vector2.Zero;
        Vector2 first = Vector2.Zero;
        for (int i = 0; i < 6; i++)
        {
            float ang = MathHelper.TwoPi * (i / 6f);
            Vector2 pt = c + new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang)) * r;
            if (i > 0) DrawLine(prev, pt, col, thickness);
            else first = pt;
            prev = pt;
        }
        DrawLine(prev, first, col, thickness);
    }

    private void DrawLine(Vector2 a, Vector2 b, Color c, float th)
    {
        Vector2 d = b - a;
        float len = d.Length();
        if (len < 0.0001f) return;
        float rot = (float)Math.Atan2(d.Y, d.X);
        spriteBatch.Draw(Hacknet.Utils.white, a, null, c, rot, Vector2.Zero, new Vector2(len, th), SpriteEffects.None, 0f);
    }
}
