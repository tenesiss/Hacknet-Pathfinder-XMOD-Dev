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

public class SocketExe : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "SocketCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: network nodes and pulses
    private struct Node { public Vector2 Pos; public float Phase; }
    private struct Link { public int A; public int B; public float Phase; }
    private Node[] nodes = System.Array.Empty<Node>();
    private Link[] links = System.Array.Empty<Link>();
    private float time = 0f;
    private readonly System.Random rng = new System.Random();
    private Microsoft.Xna.Framework.Rectangle lastBounds;
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
        else if (Int32.Parse(this.Args[1]) != num)
        {
            os.write("Target Port is Closed");
            os.write("Execution failed");
            needsRemoval = true;
        }
        Programs.getComputer(os, targetIP).hostileActionTaken();
        lastBounds = this.Bounds;
        BuildNetwork(lastBounds);
    }
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));

        // Links
        for (int i = 0; i < links.Length; i++)
        {
            var l = links[i];
            var a = nodes[l.A];
            var b = nodes[l.B];
            float pulse = (float)(Math.Sin(time * 1.8f + l.Phase) * 0.5 + 0.5);
            Color lc = Color.Lerp(new Color(50, 90, 120), new Color(0, 255, 220), pulse) * (0.55f * fade);
            DrawLine(a.Pos, b.Pos, lc, 1.5f);

            // Moving dot toward center as connection establishes
            float tdot = (float)((time * 0.5f + l.Phase) % 1.0);
            Vector2 pt = Vector2.Lerp(a.Pos, b.Pos, tdot);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)pt.X - 2, (int)pt.Y - 2, 4, 4), new Color(0, 255, 220) * (0.8f * fade));
        }
        // Nodes
        for (int i = 0; i < nodes.Length; i++)
        {
            var n = nodes[i];
            float r = 2f + 1.5f * (float)Math.Sin(time * 2.4f + n.Phase) * 0.5f;
            Color c = Color.Lerp(new Color(200, 200, 255), new Color(0, 255, 220), p) * (0.9f * fade);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)(n.Pos.X - r), (int)(n.Pos.Y - r), (int)(r * 2f), (int)(r * 2f)), c);
        }

        // Status
        string status = p >= 1f ? "CRACKED" : "CRACKING";
        TextItem.doFontLabel(new Vector2(Bounds.X + 8f, Bounds.Bottom - 22f), status, GuiData.smallfont, Color.DarkGray * fade);
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        // Rebuild layout if window moved or resized so patterns stay in their rectangle
        if (this.Bounds.X != lastBounds.X || this.Bounds.Y != lastBounds.Y || this.Bounds.Width != lastBounds.Width || this.Bounds.Height != lastBounds.Height)
        {
            lastBounds = this.Bounds;
            BuildNetwork(lastBounds);
        }
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("socket", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            time += t;
        }

    }

    private void BuildNetwork(Microsoft.Xna.Framework.Rectangle rect)
    {
        // Use inner margins to avoid top/status overlap and edges
        int marginLeft = 10, marginRight = 10, marginTop = 26, marginBottom = 26;
        var inner = new Microsoft.Xna.Framework.Rectangle(
            rect.X + marginLeft,
            rect.Y + marginTop,
            Math.Max(8, rect.Width - marginLeft - marginRight),
            Math.Max(8, rect.Height - marginTop - marginBottom)
        );

        int n = Math.Max(6, (inner.Width * inner.Height) / 9000);
        nodes = new Node[n];
        for (int i = 0; i < n; i++)
        {
            float x = inner.X + (float)rng.NextDouble() * inner.Width;
            float y = inner.Y + (float)rng.NextDouble() * inner.Height;
            nodes[i] = new Node { Pos = new Vector2(x, y), Phase = (float)rng.NextDouble() * MathHelper.TwoPi };
        }
        var linkList = new System.Collections.Generic.List<Link>();
        for (int i = 0; i < n; i++)
        {
            int other = rng.Next(n);
            if (other == i) other = (other + 1) % n;
            linkList.Add(new Link { A = i, B = other, Phase = (float)rng.NextDouble() });
        }
        links = linkList.ToArray();
    }

    private void DrawLine(Vector2 a, Vector2 b, Color col, float thickness)
    {
        Vector2 d = b - a;
        float len = d.Length();
        if (len < 0.0001f) return;
        float rot = (float)Math.Atan2(d.Y, d.X);
        spriteBatch.Draw(Hacknet.Utils.white, a, null, col, rot, Vector2.Zero, new Vector2(len, thickness), SpriteEffects.None, 0f);
    }
}
