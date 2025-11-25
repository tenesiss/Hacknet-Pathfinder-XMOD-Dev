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
using System.Collections.Generic;

public class ShadingCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "ShadingCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: dot-splitting growth inside inner rectangle
    private readonly Random rng = new Random();
    private readonly int marginLeft = 10, marginRight = 10, marginTop = 26, marginBottom = 28;
    private Rectangle innerRect;
    private int cols, rows;
    private float spacing;
    private float originX, originY; // centered origin of grid within innerRect
    private bool[] occupied = Array.Empty<bool>();
    private Queue<Point> frontier = new Queue<Point>();
    private System.Collections.Generic.HashSet<int> frontierSet = new System.Collections.Generic.HashSet<int>();
    private int occupiedCount = 0;
    private float spawnAccumulator = 0f;
    private float animTime = 0f;
    private const float GrowthFrac = 0.7f; // fraction of complete time used for growth
    private const float PulseFrac = 0.3f;  // fraction used for pulse
    private float growthDuration;
    private float pulseDuration;
    public ShadingCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Shading Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(591);

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
        // Use ExeSettings-provided complete time, split into growth/pulse phases
        growthDuration = Math.Max(0f, maxLifetime * GrowthFrac);
        pulseDuration = Math.Max(0f, maxLifetime - growthDuration);
        BuildGridLayout();
        SeedCenter();
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));

        // Fill inner rect with growing dots
        bool pulsing = lifetime >= growthDuration;
        float dotRBase = Math.Max(2f, spacing * 0.26f);
        float pulseScale = pulsing ? (0.9f + 0.15f * (float)Math.Sin(animTime * 6.0f)) : 1f;
        float dotR = dotRBase * pulseScale;
        for (int gy = 0; gy < rows; gy++)
        {
            for (int gx = 0; gx < cols; gx++)
            {
                int idx = gy * cols + gx;
                if (idx < 0 || idx >= occupied.Length || !occupied[idx]) continue;
                Vector2 pos = GridToPos(gx, gy);
                Color c = pulsing
                    ? new Color(255, 50, 70) * (0.85f * fade)
                    : Color.Lerp(new Color(0, 90, 140), new Color(0, 255, 220), 0.65f) * (0.85f * fade);
                var rect = new Rectangle((int)(pos.X - dotR), (int)(pos.Y - dotR), (int)(dotR * 2f), (int)(dotR * 2f));
                spriteBatch.Draw(Hacknet.Utils.white, rect, c);
            }
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
            Programs.getComputer(os, targetIP).openPort("shading", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            animTime += t;
            // During growth phase, ensure fill reaches 100% by 7s
            if (lifetime < growthDuration)
            {
                // Accelerate spawning as more dots exist
                float rate = 3f + 0.06f * occupiedCount + 0.0009f * occupiedCount * occupiedCount;
                spawnAccumulator += rate * t;
                int toSpawn = (int)spawnAccumulator;
                if (toSpawn > 0) spawnAccumulator -= toSpawn;
                for (int i = 0; i < toSpawn; i++) ExpandOne();

                // Guarantee target fill based on elapsed fraction
                int total = cols * rows;
                if (total > 0)
                {
                    float targetFrac = Math.Max(0f, Math.Min(1f, growthDuration > 0f ? lifetime / growthDuration : 1f));
                    int targetCount = (int)Math.Ceiling(targetFrac * total);
                    while (occupiedCount < targetCount && frontier.Count > 0) ExpandOne();
                }
            }
            else
            {
                // If we just crossed into pulse phase and there are any gaps left, fill them instantly
                int total = cols * rows;
                if (occupiedCount < total)
                {
                    // Fast path: exhaust frontier
                    while (frontier.Count > 0 && occupiedCount < total) ExpandOne();
                    // Safety: fill any remaining cells (should be rare if frontier exhausted)
                    if (occupiedCount < total)
                    {
                        for (int gy = 0; gy < rows; gy++)
                            for (int gx = 0; gx < cols; gx++)
                                if (!IsOccupied(gx, gy)) SetOccupied(gx, gy);
                    }
                }
            }
            // No more growth during pulse phase
            // Recompute layout if bounds changed
            Rectangle currentInner = GetInnerRect();
            if (!currentInner.Equals(innerRect))
            {
                RebuildPreservingCenter();
            }
        }

    }

    private Rectangle GetInnerRect()
    {
        return new Rectangle(
            Bounds.X + marginLeft,
            Bounds.Y + marginTop,
            Math.Max(8, Bounds.Width - marginLeft - marginRight),
            Math.Max(8, Bounds.Height - marginTop - marginBottom)
        );
    }

    private void BuildGridLayout()
    {
        innerRect = GetInnerRect();
        float minDim = Math.Min(innerRect.Width, innerRect.Height);
        spacing = MathHelper.Clamp(minDim / 22f, 8f, 16f);
        cols = Math.Max(1, (int)Math.Floor(innerRect.Width / spacing));
        rows = Math.Max(1, (int)Math.Floor(innerRect.Height / spacing));
        occupied = new bool[cols * rows];
        frontier.Clear();
        frontierSet.Clear();
        occupiedCount = 0;
        // Center grid horizontally (and vertically for consistency)
        float gridW = (cols - 1) * spacing;
        float gridH = (rows - 1) * spacing;
        originX = innerRect.X + (innerRect.Width - gridW) * 0.5f;
        originY = innerRect.Y + (innerRect.Height - gridH) * 0.5f;
    }

    private void SeedCenter()
    {
        Vector2 center = new Vector2(innerRect.X + innerRect.Width / 2f, innerRect.Y + innerRect.Height / 2f);
        int gx = (int)Math.Round((center.X - originX) / spacing);
        int gy = (int)Math.Round((center.Y - originY) / spacing);
        gx = Math.Max(0, Math.Min(cols - 1, gx));
        gy = Math.Max(0, Math.Min(rows - 1, gy));
        SetOccupied(gx, gy);
        EnqueueNeighbors(gx, gy);
    }

    private void RebuildPreservingCenter()
    {
        float fill = (cols * rows) > 0 ? (float)occupiedCount / (cols * rows) : 0f;
        BuildGridLayout();
        SeedCenter();
        // Ensure fill is at least as far along as the current phase dictates
        float phaseTargetFrac;
        if (growthDuration <= 0f) phaseTargetFrac = 1f;
        else if (lifetime >= growthDuration) phaseTargetFrac = 1f;
        else phaseTargetFrac = Math.Max(fill, Math.Max(0f, Math.Min(1f, lifetime / growthDuration)));
        int phaseTarget = (int)Math.Ceiling(phaseTargetFrac * cols * rows);
        while (occupiedCount < phaseTarget && frontier.Count > 0) ExpandOne();
    }

    private Vector2 GridToPos(int gx, int gy)
    {
        float x = originX + gx * spacing;
        float y = originY + gy * spacing;
        return new Vector2(x, y);
    }

    private bool IsOccupied(int gx, int gy)
    {
        int idx = gy * cols + gx;
        if (idx < 0 || idx >= occupied.Length) return false;
        return occupied[idx];
    }
    private void SetOccupied(int gx, int gy)
    {
        int idx = gy * cols + gx;
        if (idx < 0 || idx >= occupied.Length) return;
        if (!occupied[idx]) { occupied[idx] = true; occupiedCount++; }
    }

    private void EnqueueNeighbors(int gx, int gy)
    {
        TryEnqueue(gx + 1, gy);
        TryEnqueue(gx - 1, gy);
        TryEnqueue(gx, gy + 1);
        TryEnqueue(gx, gy - 1);
    }
    private void TryEnqueue(int gx, int gy)
    {
        if (gx < 0 || gy < 0 || gx >= cols || gy >= rows) return;
        int idx = gy * cols + gx;
        if (occupied[idx]) return;
        if (frontierSet.Contains(idx)) return;
        frontier.Enqueue(new Point(gx, gy));
        frontierSet.Add(idx);
    }

    private void ExpandOne()
    {
        if (frontier.Count == 0) return;
        var p = frontier.Dequeue();
        int idx = p.Y * cols + p.X;
        frontierSet.Remove(idx);
        if (idx < 0 || idx >= occupied.Length || occupied[idx]) return;
        SetOccupied(p.X, p.Y);
        EnqueueNeighbors(p.X, p.Y);
    }
}
