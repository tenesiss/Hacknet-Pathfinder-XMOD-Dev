using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Pathfinder.Port;
using Pathfinder;
using System;
using System.Collections.Generic;
using Hacknet.Gui;
using XMOD;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

public class EncypherExe : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EncypherExe";
    public int num;
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    private string StringToEncrypt;
    private string StringEncrypted;
    private List<int> FolderEnc = new List<int>();
    private string encFilename;
    // Preview/animation state
    // Noise grid (random text field) that converts into red nonsense
    private readonly char[] nonsenseChars = "0123456789ABCDEF@#$%&*+/?<>[]{}".ToCharArray();
    private readonly char[] plainChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_:/\\=()".ToCharArray();
    private readonly Random rng = new Random();
    private SpriteFont gridFont;
    private float cellW;
    private float cellH;
    private int gridCols;
    private int gridRows;
    private char[] gridPlain = Array.Empty<char>();
    private char[] gridCode = Array.Empty<char>();
    private float[] gridThreshold = Array.Empty<float>();
    private bool gridReady = false;
    public EncypherExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Encypher";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {
        base.LoadContent();
            if(this.Args.Length < 2)
            {
                needsRemoval = true;
                os.write("You must input a filename");
            } else
            {
                    if(!Programs.getCurrentFolder(os).containsFile(this.Args[1]))
                    {
                        needsRemoval = true;
                        os.write("Invalid file");
                    }
                    else
                    {
                        // Prepare the random noise grid for the animation
                        try { PrepareNoiseGrid(); gridReady = true; } catch { gridReady = false; }
                    }
                }
        }
    
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        // Smooth progress (ease in-out) from 0..1
        float p = Math.Max(0f, Math.Min(1f, lifetime / Math.Max(0.0001f, maxLifetime)));
        float smooth = p * p * (3f - 2f * p);

        // Ensure grid matches current bounds/font if needed
        EnsureNoiseGridUpToDate();
        DrawNoiseGrid(smooth);

        // Top-left status with percentage inside bounds
        string status = p >= 1f ? "ENCRYPTED" : "ENCRYPTING";
        int percent = (int)Math.Round(p * 100f);
        string label = status + " " + percent + "%";
        var labelPos = new Vector2(Bounds.X + 8f, Bounds.Y + 12f);
        TextItem.doFontLabel(labelPos, label, GuiData.smallfont ?? GuiData.font, Color.LightGray * fade);
    }

    
    public override void Completed()
    {

    }
    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            StringToEncrypt = Programs.getCurrentFolder(os).searchForFile(this.Args[1]).data;
            FolderEnc.Add(0);
            encFilename = XMOD.XMOD.FileFilter(this.Args[1]+"_Encrypted.dec", os);
            if (this.Args.Length >= 3)
            {
                StringEncrypted = Hacknet.FileEncrypter.EncryptString(StringToEncrypt, "Encrypted Source", "localhost", this.Args[2]);
                Programs.getCurrentFolder(os).files.Add(new FileEntry(StringEncrypted, encFilename));
            } else
            {
                StringEncrypted = Hacknet.FileEncrypter.EncryptString(StringToEncrypt, "Encrypted Source", "localhost");
                Programs.getCurrentFolder(os).files.Add(new FileEntry(StringEncrypted, encFilename));
            }
        }
        else
        {
            lifetime += t;
        }

    }

    private void PrepareNoiseGrid()
    {
        // Font selection
        gridFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font;

        const float pad = 8f;
        // Reserve space for header label
        var headerFont = GuiData.smallfont ?? GuiData.font;
        float headerH = headerFont.MeasureString("ENCRYPTING 100%").Y + 6f;

        // Measure cell size
        Vector2 m = gridFont.MeasureString("W");
        cellW = Math.Max(4f, m.X);
        cellH = Math.Max(8f, m.Y + 1f);

        float availW = Math.Max(0f, Bounds.Width - pad * 2f);
        float availH = Math.Max(0f, Bounds.Height - pad - headerH - pad);

        gridCols = (int)Math.Floor(availW / cellW);
        gridRows = (int)Math.Floor(availH / cellH);
        if (gridCols < 1 || gridRows < 1)
        {
            gridPlain = Array.Empty<char>();
            gridCode = Array.Empty<char>();
            gridThreshold = Array.Empty<float>();
            return;
        }

        int total = gridCols * gridRows;
        // Cap total to avoid excessive draw cost
        if (total > 5000)
        {
            // Increase cell size proportionally to cap density
            float scale = (float)Math.Sqrt(total / 5000.0);
            cellW *= scale;
            cellH *= scale;
            gridCols = Math.Max(1, (int)Math.Floor(availW / cellW));
            gridRows = Math.Max(1, (int)Math.Floor(availH / cellH));
            total = gridCols * gridRows;
        }

        gridPlain = new char[total];
        gridCode = new char[total];
        gridThreshold = new float[total];

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                int i = r * gridCols + c;
                gridPlain[i] = plainChars[rng.Next(plainChars.Length)];
                gridCode[i] = nonsenseChars[rng.Next(nonsenseChars.Length)];
                // Threshold with slight diagonal bias for pleasing sweep
                float baseTh = (float)rng.NextDouble();
                float bias = ((float)r / Math.Max(1, gridRows - 1) + (float)c / Math.Max(1, gridCols - 1)) * 0.5f;
                gridThreshold[i] = MathHelper.Clamp(baseTh * 0.6f + bias * 0.4f, 0f, 1f);
            }
        }
    }

    private void EnsureNoiseGridUpToDate()
    {
        var currentFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font;
        if (!gridReady || currentFont != gridFont)
        {
            PrepareNoiseGrid();
            gridReady = true;
            return;
        }

        const float pad = 8f;
        var headerFont = GuiData.smallfont ?? GuiData.font;
        float headerH = headerFont.MeasureString("ENCRYPTING 100%").Y + 6f;

        float availW = Math.Max(0f, Bounds.Width - pad * 2f);
        float availH = Math.Max(0f, Bounds.Height - pad - headerH - pad);
        int cols = (int)Math.Floor(availW / Math.Max(1f, cellW));
        int rows = (int)Math.Floor(availH / Math.Max(1f, cellH));
        if (cols != gridCols || rows != gridRows)
        {
            PrepareNoiseGrid();
            gridReady = true;
        }
    }

    private void DrawNoiseGrid(float progress)
    {
        if (gridPlain == null || gridPlain.Length == 0) return;

        const float pad = 8f;
        var headerFont = GuiData.smallfont ?? GuiData.font;
        float headerH = headerFont.MeasureString("ENCRYPTING 100%").Y + 6f;

        float startX = Bounds.X + pad;
        float startY = Bounds.Y + pad + headerH;

        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        for (int r = 0; r < gridRows; r++)
        {
            float y = startY + r * cellH;
            for (int c = 0; c < gridCols; c++)
            {
                int i = r * gridCols + c;
                float x = startX + c * cellW;

                float th = gridThreshold[i];
                // Smooth local blend around threshold width
                const float transWidth = 0.18f;
                float a = MathHelper.Clamp((progress - th) / transWidth + 0.5f, 0f, 1f);
                a = a * a * (3f - 2f * a);

                char pc = gridPlain[i];
                char ec = gridCode[i];

                // Optional subtle pulse on encoded
                float pulse = 0.85f + 0.15f * (float)Math.Sin((i * 0.15f) + time * 7f);
                Color redCol = new Color((int)(220 * pulse), (int)(40 * pulse), (int)(40 * pulse));

                // Cross-fade plain -> encoded for smoothness
                if (a < 1f)
                {
                    Color plainCol = Color.White * (fade * (1f - a));
                    GuiData.spriteBatch.DrawString(gridFont, pc.ToString(), new Vector2(x, y), plainCol);
                }
                if (a > 0f)
                {
                    Color encCol = redCol * (fade * a);
                    GuiData.spriteBatch.DrawString(gridFont, ec.ToString(), new Vector2(x, y), encCol);
                }
            }
        }
    }
}
