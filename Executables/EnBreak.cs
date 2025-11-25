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
using System.Collections.Generic;

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
    
    // Visuals
    // Wormhole visuals (replaces the old ring)
    private Texture2D wormholeTex;
    private readonly List<Particle> particles = new List<Particle>(256);
    private static readonly Random rng = new Random();
    private float baseRadius;
    private float spawnAccumulator = 0f; // keeps spawning steady at small dt
    private float holeInnerFrac = 0.12f; // inner hole as fraction of outer radius
    private Color neon1 = new Color(0, 255, 230);
    private Color neon2 = new Color(170, 0, 255);
    private Color innerDark = new Color(5, 8, 12);
    private float animTime = 0f;
    // Background dots for subtle texture
    private struct BgDot { public Vector2 Pos; public float Size; public float Alpha; public Color Col; }
    private readonly List<BgDot> bgDots = new List<BgDot>(96);
    
    private struct Particle
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public float Life;
        public float MaxLife;
        public float Size;
        public Color Col;
    }
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
        // Prepare visual assets
        baseRadius = Math.Min(Bounds.Width, Bounds.Height) * 0.4f;
        wormholeTex = CreateWormholeTexture(256);
        // Reach 100% in ~12 seconds
        maxLifetime = 12f;

        // Generate subtle background dots inside the bounds
        bgDots.Clear();
        int dotCount = Math.Max(24, (Bounds.Width * Bounds.Height) / 9000); // scale by area
        for (int i = 0; i < dotCount; i++)
        {
            float margin = 6f;
            float x = Bounds.X + margin + (float)rng.NextDouble() * Math.Max(1f, (Bounds.Width - 2 * margin));
            float y = Bounds.Y + margin + (float)rng.NextDouble() * Math.Max(1f, (Bounds.Height - 2 * margin));
            float size = 1f + (float)rng.NextDouble() * 1.5f; // 1..2.5px
            float alpha = 0.05f + (float)rng.NextDouble() * 0.08f; // very subtle
            var col = Color.White;
            bgDots.Add(new BgDot { Pos = new Vector2(x, y), Size = size, Alpha = alpha, Col = col });
        }
    }
    
    
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        var center = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0f ? lifetime / maxLifetime : 1f));
        float radius = ComputeHoleRadius(p);

        // Background subtle dots
        for (int i = 0; i < bgDots.Count; i++)
        {
            var d = bgDots[i];
            var c = d.Col * (d.Alpha * fade);
            spriteBatch.Draw(Hacknet.Utils.white, d.Pos, null, c, 0f, Vector2.Zero, new Vector2(d.Size, d.Size), SpriteEffects.None, 0f);
        }

        DrawWormhole(center, radius);

        DrawParticles(center, radius);

        // Status text
        if (p >= 1f || isExiting)
        {
            TextItem.doFontLabel(new Vector2(Bounds.X + 10f, Bounds.Bottom - 22f), "HACK COMPLETED", GuiData.smallfont, Color.LightGray * fade);
        }
        else
        {
            string pct = ((int)(p * 100f)).ToString() + "%";
            TextItem.doFontLabel(new Vector2(Bounds.X + 10f, Bounds.Bottom - 22f), "Absorbing data... " + pct, GuiData.smallfont, Color.Cyan * fade);
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
            animTime += t;
        }
        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0f ? lifetime / maxLifetime : 1f));
        SpawnParticles(p, t);
        var centerVec = new Vector2(Bounds.Center.X, Bounds.Center.Y);
        float outerR = ComputeHoleRadius(p);
        float innerHoleR = Math.Max(2f, outerR * holeInnerFrac);
        bool fastSuck = (maxLifetime - lifetime) <= 2f || isExiting;
        UpdateParticles(t, centerVec, innerHoleR, fastSuck);
    }

    private float SmoothStep01(float x)
    {
        x = Math.Max(0f, Math.Min(1f, x));
        return x * x * (3f - 2f * x);
    }

    private float ComputeHoleRadius(float progress)
    {
        progress = Math.Max(0f, Math.Min(1f, progress));
        float openPhase = SmoothStep01(Math.Min(progress / 0.2f, 1f));
        float closePhase = SmoothStep01(Math.Max((progress - 0.85f) / 0.15f, 0f));
        float r = baseRadius * Math.Max(0.02f, openPhase * (1f - 0.98f * closePhase));
        float maxExtent = (Math.Min(Bounds.Width, Bounds.Height) * 0.5f) - 4f; // small margin
        if (r > maxExtent) r = maxExtent;
        return r;
    }

    private Texture2D CreateWormholeTexture(int size)
    {
        var tex = new Texture2D(GuiData.spriteBatch.GraphicsDevice, size, size);
        var data = new Color[size * size];
        Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float maxR = c.X;

        // Spiral parameters
        float arms = 3.5f;      // number of spiral arms
        float bandFreq = 24f;   // spiral band frequency
        float radialFade = 2.6f; // how fast it darkens outward
        float innerDarken = 0.85f; // how dark the very center gets

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = x - c.X;
                float fy = y - c.Y;
                float r = (float)Math.Sqrt(fx * fx + fy * fy);
                if (r > maxR)
                {
                    data[y * size + x] = Color.Transparent;
                    continue;
                }

                float nr = r / maxR; // 0..1
                float a = (float)Math.Atan2(fy, fx); // -pi..pi

                float spiral = (float)Math.Sin(a * arms + nr * bandFreq);
                float spiralMask = 0.55f + 0.45f * spiral; // 0.1..1

                float depth = (float)Math.Pow(1f - nr, radialFade);
                depth = MathHelper.Clamp(depth, 0f, 1f);

                float core = 1f - (float)Math.Pow(1f - Math.Max(0f, 1f - nr * 3.0f), 3.0f);
                float inner = MathHelper.Clamp(1f - innerDarken * core, 0f, 1f);

                float intensity = depth * spiralMask * inner;

                float hue = (a / (float)Math.PI + 1f) * 0.5f; // 0..1
                var col = Color.Lerp(new Color(30, 200, 255), new Color(200, 60, 255), hue);

                float alpha = MathHelper.Clamp(intensity, 0f, 1f);
                var v4 = col.ToVector4();
                v4.X *= alpha; v4.Y *= alpha; v4.Z *= alpha; v4.W = alpha;
                col = new Color(v4);
                data[y * size + x] = col;
            }
        }
        tex.SetData(data);
        return tex;
    }

    private void DrawWormhole(Vector2 center, float outerRadius)
    {
        if (wormholeTex == null || outerRadius <= 0.5f) return;
        var origin = new Vector2(wormholeTex.Width / 2f, wormholeTex.Height / 2f);
        float scale = (outerRadius * 2f) / wormholeTex.Width;

        // Base spinning layer
        float rot = animTime * 0.9f;
        Color baseCol = Color.White * (0.9f * fade);
        spriteBatch.Draw(wormholeTex, center, null, baseCol, rot, origin, scale, SpriteEffects.None, 0f);

        // Inner tighter layer for parallax/depth
        float rot2 = -animTime * 1.3f;
        Color layerCol = new Color(180, 240, 255) * (0.6f * fade);
        spriteBatch.Draw(wormholeTex, center, null, layerCol, rot2, origin, scale * 0.85f, SpriteEffects.None, 0f);

        // Outer faint glow
        Color glow = new Color(80, 120, 200) * (0.35f * fade);
        spriteBatch.Draw(wormholeTex, center, null, glow, rot * 0.5f, origin, scale * 1.15f, SpriteEffects.None, 0f);

        // Central darkening to sell the depth (use same circular texture as a mask)
        float coreScale = Math.Max(0.08f, scale * 0.18f);
        Color core = new Color(0, 0, 0) * (0.6f * fade);
        spriteBatch.Draw(wormholeTex, center, null, core, rot2 * 0.5f, origin, coreScale, SpriteEffects.None, 0f);
    }

    private void SpawnParticles(float progress, float dt)
    {
        if (progress >= 1f || isExiting) return;

        float baseRate = 30f; // particles/sec minimum
        float rate = baseRate + 90f * (1f - progress);
        spawnAccumulator += rate * dt;
        int toSpawn = (int)spawnAccumulator;
        if (toSpawn <= 0) return;
        spawnAccumulator -= toSpawn;
        for (int i = 0; i < toSpawn; i++)
        {
            // Spawn around the window edges
            Vector2 pos;
            int side = rng.Next(4);
            if (side == 0) pos = new Vector2(Bounds.X + rng.Next(Bounds.Width), Bounds.Y - 4);
            else if (side == 1) pos = new Vector2(Bounds.X + rng.Next(Bounds.Width), Bounds.Bottom + 4);
            else if (side == 2) pos = new Vector2(Bounds.X - 4, Bounds.Y + rng.Next(Bounds.Height));
            else pos = new Vector2(Bounds.Right + 4, Bounds.Y + rng.Next(Bounds.Height));

            Vector2 towards = new Vector2(Bounds.Center.X, Bounds.Center.Y) - pos;
            if (towards.LengthSquared() < 0.001f) towards = new Vector2(0, 1);
            towards.Normalize();
            // Speed and swirl
            float speed = 80f + (float)rng.NextDouble() * 140f;
            Vector2 perp = new Vector2(-towards.Y, towards.X) * (float)(rng.NextDouble() * 40f - 20f);
            Vector2 vel = towards * speed + perp;

            var c = Color.Lerp(neon1, neon2, (float)rng.NextDouble());
            float size = 1.0f + (float)rng.NextDouble() * 2.0f;
            float life = 0.9f + (float)rng.NextDouble() * 1.6f;

            particles.Add(new Particle
            {
                Pos = pos,
                Vel = vel,
                Life = life,
                MaxLife = life,
                Size = size,
                Col = c
            });
        }
    }

    private void UpdateParticles(float dt, Vector2 center, float holeInnerRadius, bool fastSuck)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            // Keep particles alive until consumed; use Life only for fade
            p.Life = Math.Max(0f, p.Life - dt);

            // Accelerate toward center (gravity-like)
            Vector2 toCenter = center - p.Pos;
            float dist = toCenter.Length();
            if (dist > 0.0001f)
            {
                Vector2 dir = toCenter / dist;

                // Consume particles when reaching the small center hole
                float consumeR = fastSuck ? Math.Max(40f, holeInnerRadius * 6f)
                                          : Math.Max(2f, holeInnerRadius * 1.2f);
                if (dist <= consumeR)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                // Desired radial acceleration increases as we get closer
                float accel = fastSuck ? (1200f + 4000f / Math.Max(20f, dist))
                                       : (80f + 600f / Math.Max(40f, dist));

                // Decompose velocity into radial and tangential
                float vRad = Vector2.Dot(p.Vel, dir);
                Vector2 vTan = p.Vel - dir * vRad;

                // Remove any outward motion to prevent bouncing away
                if (vRad < 0f) vRad = 0f;

                // Dampen tangential component to spiral inward smoothly
                float tangentialDamping = Math.Max(0f, 1f - (fastSuck ? 18f : 8f) * dt);
                vTan *= tangentialDamping;

                // Increase inward speed smoothly
                vRad = Math.Min(vRad + accel * dt, fastSuck ? 2000f : 600f);

                p.Vel = dir * vRad + vTan;
            }

            p.Pos += p.Vel * dt;
            particles[i] = p;
        }
    }

    private void DrawParticles(Vector2 center, float radius)
    {
        // draw as short streaks pointing toward the center
        for (int i = 0; i < particles.Count; i++)
        {
            var p = particles[i];
            float a = Math.Max(0f, Math.Min(1f, p.Life / p.MaxLife));
            // Fade in then out
            float fadeA = a < 0.2f ? (a / 0.2f) : (1f - (a - 0.2f) / 0.8f);
            Color c = p.Col * (0.9f * fade * fadeA + 0.1f);

            Vector2 dir = center - p.Pos;
            float d = dir.Length();
            if (d > 0.0001f) dir /= d; else dir = new Vector2(1, 0);
            float len = Math.Min(14f, 3f + (140f / Math.Max(20f, d)));
            float rot = (float)Math.Atan2(dir.Y, dir.X);

            // head point
            spriteBatch.Draw(Hacknet.Utils.white, p.Pos, null, c, rot, Vector2.Zero, new Vector2(len, p.Size), SpriteEffects.None, 0f);
        }
    }
}
