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

public class VersionCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "VersionCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: commit graph
    private struct Node { public Vector2 Pos; public float R; public float Phase; }
    private struct Edge { public int A; public int B; public float Phase; }
    private Node[] nodes = Array.Empty<Node>();
    private Edge[] edges = Array.Empty<Edge>();
    private readonly Random rng = new Random();
    private float animTime = 0f;
    private Rectangle lastBounds;
    public VersionCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Version Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(9418);

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
        // Build commit graph layout for current bounds
        lastBounds = this.Bounds;
        BuildGraph(lastBounds);
    }
 
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));
        float ease = p * p * (3f - 2f * p);

        // Draw edges with moving highlights
        for (int i = 0; i < edges.Length; i++)
        {
            var e = edges[i];
            var a = nodes[e.A];
            var b = nodes[e.B];
            Color lineC = Color.Lerp(new Color(70, 90, 120), new Color(0, 220, 255), 0.35f + 0.4f * (float)Math.Sin(animTime * 1.2f + e.Phase)) * (0.75f * fade);
            DrawLine(a.Pos, b.Pos, lineC, 2f);

            // Pulse traveling
            float pulse = (float)((animTime * 0.45f + e.Phase) % 1.0);
            Vector2 pt = Vector2.Lerp(a.Pos, b.Pos, pulse);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)pt.X - 2, (int)pt.Y - 2, 4, 4), new Color(0, 255, 220) * (0.8f * fade));
        }

        // Draw nodes (commits)
        for (int i = 0; i < nodes.Length; i++)
        {
            var n = nodes[i];
            float r = 3f + 2f * (float)Math.Sin(animTime * 2f + n.Phase) * 0.5f;
            Color nc = Color.Lerp(Color.White, new Color(0, 255, 220), 0.5f + 0.5f * ease) * (0.9f * fade);
            spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)(n.Pos.X - r), (int)(n.Pos.Y - r), (int)(r * 2f), (int)(r * 2f)), nc);
        }

        // Status
        string status = p >= 1f ? "CRACKED" : "CRACKING";
        TextItem.doFontLabel(new Vector2(Bounds.X + 8f, Bounds.Bottom - 22f), status, GuiData.smallfont, Color.DarkGray * fade);
    }

   

    public override void Update(float t)
    {
        base.Update(t);
        // If window moved/resized, rebuild graph in its new rectangle
        if (this.Bounds.X != lastBounds.X || this.Bounds.Y != lastBounds.Y || this.Bounds.Width != lastBounds.Width || this.Bounds.Height != lastBounds.Height)
        {
            lastBounds = this.Bounds;
            BuildGraph(lastBounds);
        }
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("version", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            animTime += t;
        }

    }

    private void BuildGraph(Rectangle rect)
    {
        // Add margins so the graph isn't tight against top/status
        int marginLeft = 12, marginRight = 12, marginTop = 28, marginBottom = 30;
        Rectangle inner = new Rectangle(
            rect.X + marginLeft,
            rect.Y + marginTop,
            Math.Max(10, rect.Width - marginLeft - marginRight),
            Math.Max(10, rect.Height - marginTop - marginBottom)
        );

        int cols = Math.Max(3, inner.Width / 90);
        int rows = Math.Max(4, inner.Height / 60);
        int nCount = cols * rows;
        nodes = new Node[nCount];
        float padX = 12f, padY = 12f;
        float cellW = (inner.Width - padX * 2f) / Math.Max(1, cols - 1);
        float cellH = (inner.Height - padY * 2f) / Math.Max(1, rows - 1);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int i = r * cols + c;
                float x = inner.X + padX + c * cellW + ((float)rng.NextDouble() - 0.5f) * 8f;
                float y = inner.Y + padY + r * cellH + ((float)rng.NextDouble() - 0.5f) * 6f;
                nodes[i] = new Node { Pos = new Vector2(x, y), R = 3f, Phase = (float)rng.NextDouble() * MathHelper.TwoPi };
            }
        }
        var edgeList = new System.Collections.Generic.List<Edge>();
        // Connect row-wise and some diagonals like branches
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                int i = r * cols + c;
                edgeList.Add(new Edge { A = i, B = i + 1, Phase = (float)rng.NextDouble() });
                if (r < rows - 1 && rng.NextDouble() < 0.6)
                    edgeList.Add(new Edge { A = i, B = (r + 1) * cols + c + (rng.NextDouble() < 0.5 ? 0 : 1), Phase = (float)rng.NextDouble() });
            }
        }
        edges = edgeList.ToArray();
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
