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

public class PoliceCrack : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "PoliceCrack";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Visuals: scrolling terminal-like random characters
    private readonly Random rng = new Random();
    private SpriteFont termFont;
    private float cellW;
    private float cellH;
    private int rows;
    private int cols;
    private string[] rowBuffers = Array.Empty<string>();
    private float[] rowOffsets = Array.Empty<float>();
    private float[] rowSpeeds = Array.Empty<float>();
    private char[] charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-=/_+<>[]{}()!@#$%^&*~".ToCharArray();
    private char[] glitchset = "0123456789ABCDEF@#$%&*+/?<>[]{}".ToCharArray();
    private float pad = 6f;
    private float headerTop = 20f;
    private Rectangle lastBounds;
    private float glitchTime = 0f;
    public PoliceCrack(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Police Protocol Crusher";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();
        num = Pathfinder.Util.ComputerLookup.FindByIp(targetIP).GetDisplayPortNumberFromCodePort(188);

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
        RecalcGrid(lastBounds);
    }

    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();

        float p = Math.Max(0f, Math.Min(1f, maxLifetime > 0 ? lifetime / maxLifetime : 1f));

        float xMin = Bounds.X + pad;
        float yMin = Bounds.Y + pad + headerTop;
        float xMax = Bounds.Right - pad;
        // subtle background
        int bgH = Math.Max(0, (int)(Bounds.Height - (pad * 2f + headerTop)));
        spriteBatch.Draw(Hacknet.Utils.white, new Rectangle((int)xMin - 1, (int)yMin - 1, (int)(xMax - xMin) + 2, bgH + 2), new Color(8, 10, 12) * (0.35f * fade));

        bool done = p >= 1f || isExiting;
        for (int r = 0; r < rows; r++)
        {
            float y = yMin + r * cellH;
            string buf = rowBuffers[r];
            float startX = xMin - rowOffsets[r];
            for (int c = 0; c < buf.Length; c++)
            {
                float cx = startX + c * cellW;
                if (cx + cellW < xMin) continue;
                if (cx > xMax) break;
                if (c == 0 && r % 3 == 0)
                {
                    Color promptCol = done ? new Color(220, 40, 60) : Color.Cyan;
                    GuiData.spriteBatch.DrawString(termFont, ">", new Vector2(cx, y), promptCol * (0.7f * fade));
                }
                char ch = buf[c];
                if (done && rng.NextDouble() < 0.18 + 0.18 * Math.Sin(glitchTime * 7.0 + (r * 0.37 + c * 0.21)))
                {
                    ch = glitchset[rng.Next(glitchset.Length)];
                }
                Color glyphCol = done ? new Color(220, 40, 60) : new Color(0, 255, 220);
                GuiData.spriteBatch.DrawString(termFont, ch.ToString(), new Vector2(cx + (r % 3 == 0 ? cellW * 0.8f : 0f), y), glyphCol * (0.8f * fade));
            }
        }

        // Status
        string status = p >= 1f ? "CRACKED" : "CRACKING";
        TextItem.doFontLabel(new Vector2(Bounds.X + 8, Bounds.Bottom - 22), status, GuiData.smallfont, Color.DarkGray * fade);
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (this.Bounds.X != lastBounds.X || this.Bounds.Y != lastBounds.Y || this.Bounds.Width != lastBounds.Width || this.Bounds.Height != lastBounds.Height)
        {
            lastBounds = this.Bounds;
            RecalcGrid(lastBounds);
        }
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).openPort("police", os.thisComputer.ip);
        }
        else
        {
            lifetime += t;
            glitchTime += t;
            for (int r = 0; r < rows; r++)
            {
                rowOffsets[r] += rowSpeeds[r] * t;
                while (rowOffsets[r] >= cellW)
                {
                    rowOffsets[r] -= cellW;
                    if (!string.IsNullOrEmpty(rowBuffers[r]))
                        rowBuffers[r] = rowBuffers[r].Substring(1) + RandomChar();
                }
            }
        }

    }

    private void RecalcGrid(Rectangle rect)
    {
        termFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font;
        Vector2 m = termFont.MeasureString("W");
        cellW = Math.Max(6f, m.X);
        cellH = Math.Max(9f, m.Y + 1f);
        float availW = Math.Max(0f, rect.Width - pad * 2f);
        float availH = Math.Max(0f, rect.Height - pad * 2f - headerTop);
        cols = Math.Max(8, (int)Math.Floor(availW / cellW) + 6);
        int newRows = Math.Max(3, (int)Math.Floor(availH / cellH));
        rows = newRows;
        rowBuffers = new string[rows];
        rowOffsets = new float[rows];
        rowSpeeds = new float[rows];
        for (int r = 0; r < rows; r++)
        {
            rowBuffers[r] = RandomRow(cols);
            rowOffsets[r] = (float)rng.NextDouble() * cellW;
            rowSpeeds[r] = 60f + (float)rng.NextDouble() * 140f; // px/sec
        }
    }

    private string RandomRow(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++) sb.Append(RandomChar());
        return sb.ToString();
    }
    private char RandomChar() => charset[rng.Next(charset.Length)];
}
