using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathfinder.Port;
using Pathfinder;
using System;
using Hacknet;
using Hacknet.Gui;
using XMOD;

public class EosCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EosCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals
    private readonly Random rng = new Random();
    private float jitterTime = 0f;
    private float glitchTimer = 0f;
    private string[] commonPins = new string[] { "0000", "1111", "1234", "2580", "0420", "8008", "0909", "6969" };
    private string[] commonWords = new string[] { "password", "letmein", "qwerty", "dragon", "iloveyou", "admin" };
    private char[] glitchChars = "01$#@%&*+?XYZ<>[]{}".ToCharArray();
    private struct LineGlitch { public float Y; public float Speed; public float Alpha; }
    private LineGlitch[] scanlines = Array.Empty<LineGlitch>();
    private float phoneShakeSeed = 0f;
    public EosCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Eos Crack";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(3659);

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
        // Prepare scanlines (Y is relative to phone screen height to avoid jitter overflow)
        int lines = Math.Max(6, Bounds.Height / 18);
        scanlines = new LineGlitch[lines];
        var sr0 = ComputeScreenRectNoJitter();
        float h0 = Math.Max(2f, sr0.Height);
        for (int i = 0; i < lines; i++)
        {
            scanlines[i] = new LineGlitch
            {
                Y = (float)rng.NextDouble() * h0,
                Speed = 18f + (float)rng.NextDouble() * 36f,
                Alpha = 0.05f + (float)rng.NextDouble() * 0.12f
            };
        }
        phoneShakeSeed = (float)rng.NextDouble() * 1000f;
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));
        float ease = p * p * (3f - 2f * p);

        // Phone body inside bounds
        float pad = 10f;
        var phoneRect = new Rectangle(
            (int)(Bounds.X + pad),
            (int)(Bounds.Y + pad + 8f),
            (int)(Bounds.Width - pad * 2f),
            (int)(Bounds.Height - pad * 2f - 16f)
        );
        float bezel = Math.Max(3f, Math.Min(8f, phoneRect.Width * 0.02f));

        // Jitter/shake
        float time = jitterTime + phoneShakeSeed;
        float shakeMag = (0.8f + (float)Math.Sin(time * 6.3f) * 0.2f) * (0.5f + ease * 2.0f);
        Vector2 jitter = new Vector2(
            (float)Math.Sin(time * 9.2f) * shakeMag,
            (float)Math.Cos(time * 7.1f) * shakeMag * 0.6f
        );

        // Draw body
        var bodyCol = new Color(20, 24, 28) * fade;
        var screenCol = new Color(6, 9, 12) * fade;
        var accent = Color.Lerp(new Color(0, 200, 255), new Color(255, 30, 80), ease) * (0.7f * fade);

        // Outer rounded-ish rect via 3 layers
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(phoneRect.X + (int)jitter.X, phoneRect.Y + (int)jitter.Y, phoneRect.Width, phoneRect.Height), bodyCol);
        // Screen
        var screenRect = new Rectangle(phoneRect.X + (int)bezel + (int)jitter.X, phoneRect.Y + (int)bezel + (int)jitter.Y, (int)(phoneRect.Width - bezel * 2f), (int)(phoneRect.Height - bezel * 2f));
        spriteBatch.Draw(Hacknet.Utils.white, screenRect, screenCol);

        // Speaker notch and home indicator
        var notchW = Math.Max(18, (int)(screenRect.Width * 0.25f));
        var notchH = 3;
        var notchX = screenRect.X + (screenRect.Width - notchW) / 2;
        var notchY = screenRect.Y + 4;
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(notchX, notchY, notchW, notchH), accent * 0.6f);
        var homeW = Math.Max(20, (int)(screenRect.Width * 0.3f));
        var homeH = 2;
        var homeX = screenRect.X + (screenRect.Width - homeW) / 2;
        var homeY = screenRect.Bottom - 6;
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle(homeX, homeY, homeW, homeH), accent * 0.4f);

        // Words that corrupt over time
        var font = GuiData.smallfont ?? GuiData.font;
        float lineH = font.MeasureString("W").Y + 2f;
        int maxLines = Math.Min(6, (int)Math.Floor((screenRect.Height - 16f) / lineH));
        int total = Math.Max(1, maxLines);

        for (int i = 0; i < total; i++)
        {
            string baseStr = (i % 2 == 0) ? commonPins[i % commonPins.Length] : commonWords[i % commonWords.Length];
            float rowY = screenRect.Y + 12f + i * lineH;
            float th = (i + 1f) / (total + 1f);
            // Local blend for this row
            float a = MathHelper.Clamp((ease - th + 0.35f) / 0.25f, 0f, 1f);
            a = a * a * (3f - 2f * a);

            // Glitched string based on a
            string glitched = baseStr;
            if (a > 0f)
            {
                var sb = new System.Text.StringBuilder(baseStr.Length);
                for (int c = 0; c < baseStr.Length; c++)
                {
                    char ch = baseStr[c];
                    // replace with glitch char with probability a
                    if (rng.NextDouble() < a * (0.6 + 0.4 * Math.Sin((time + i + c) * 5.7)))
                        ch = glitchChars[rng.Next(glitchChars.Length)];
                    sb.Append(ch);
                }
                glitched = sb.ToString();
            }

            // Slight horizontal wobble per row
            float wob = (float)Math.Sin((time + i) * 3.8f) * 2f;
            Vector2 pos = new Vector2(screenRect.X + 10f + wob, rowY);

            // Draw shadow/plain then glitch overlay
            Color plainC = Color.LightGray * (fade * (1f - a) * 0.9f);
            Color glitchC = new Color(255, 60, 100) * (fade * a);
            TextItem.doFontLabel(pos, baseStr, font, plainC);
            TextItem.doFontLabel(pos, glitched, font, glitchC);
        }

        // Moving horizontal scanlines (clamped within the phone screen)
        if (screenRect.Height > 1)
        {
            for (int i = 0; i < scanlines.Length; i++)
            {
                var s = scanlines[i];
                int y = screenRect.Y + (int)(s.Y % screenRect.Height);
                if (y < screenRect.Y) y += screenRect.Height;
                var r = new Rectangle(screenRect.X, y, screenRect.Width, 2);
                var ac = new Color(0, 255, 200) * (s.Alpha * fade * (0.3f + 0.7f * ease));
                spriteBatch.Draw(Hacknet.Utils.white, r, ac);
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
            Programs.getComputer(os, targetIP).openPort("eos", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            jitterTime += t;
            glitchTimer += t;
            // advance scanlines using phone screen height (no jitter, avoids overflow)
            var sr = ComputeScreenRectNoJitter();
            float h = Math.Max(2f, sr.Height);
            for (int i = 0; i < scanlines.Length; i++)
            {
                var s = scanlines[i];
                s.Y += s.Speed * t;
                if (s.Y >= h) s.Y -= h;
                scanlines[i] = s;
            }
        }

    }

    private Rectangle ComputeScreenRectNoJitter()
    {
        float pad = 10f;
        var phoneRect = new Rectangle(
            (int)(Bounds.X + pad),
            (int)(Bounds.Y + pad + 8f),
            (int)(Bounds.Width - pad * 2f),
            (int)(Bounds.Height - pad * 2f - 16f)
        );
        float bezel = Math.Max(3f, Math.Min(8f, phoneRect.Width * 0.02f));
        var screenRect = new Rectangle(
            phoneRect.X + (int)bezel,
            phoneRect.Y + (int)bezel,
            (int)(phoneRect.Width - bezel * 2f),
            (int)(phoneRect.Height - bezel * 2f)
        );
        return screenRect;
    }
}
