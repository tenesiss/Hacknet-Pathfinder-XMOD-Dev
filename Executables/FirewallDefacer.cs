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

public class FirewallDefacer : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "FirewallDefacer";
    public bool exe = false;
    private float lifetime = 0f;
    private float maxLifetime;
    // Decode animation state
    private readonly Random rng = new Random();
    private SpriteFont gridFont;
    private float cellW;
    private float cellH;
    private int gridCols;
    private int gridRows;
    private char[] gridFrozenChars = Array.Empty<char>();
    private float[] gridThreshold = Array.Empty<float>();
    private bool gridReady = false;
    private float flickerTimer = 0f;
    private const float FlickerInterval = 0.045f; // how fast the green noise flickers
    private readonly char[] cipherChars = "01lI![]{}<>/\\|=-+_*%#@$&?^;:~".ToCharArray();
    public FirewallDefacer(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        ramCost = ExeSettings.ram[GetIdentifier()];
        IdentifierName = "Firewall Defacer";
        maxLifetime = ExeSettings.completeTime[GetIdentifier()];
    }
    private int x = 0;
    public override void LoadContent()
    {

        base.LoadContent();

        if (os.connectedComp.firewall == null)
        {
            os.write("No firewall detected!");
            os.write("Execution failed");
            needsRemoval = true;
        }
        Programs.getComputer(os, targetIP).hostileActionTaken();
    }
    
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        // Progress (0..1) with smoothstep
        float p = Math.Max(0f, Math.Min(1f, lifetime / Math.Max(0.0001f, maxLifetime)));
        float smooth = p * p * (3f - 2f * p);

        // Ensure/prepare decode grid and draw it filling most of the rect
        // with a top margin and a gap above the bottom-left labels
        EnsureDecodeGridUpToDate();
        DrawDecodeGrid(smooth);

        // Bottom-left status label (with dots during progress)
        string status;
        if (lifetime < maxLifetime / 3f) status = "DEFACING";
        else if (lifetime < maxLifetime / 2f) status = "DEFACING.";
        else if (lifetime < maxLifetime) status = "DEFACING..";
        else status = "DEFACED";

        var labelFont = GuiData.smallfont ?? GuiData.font; // make label smaller like other EXEs
        var smallFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font; // use even smaller for password
        const float pad = 8f;
        const float lineGap = 4f;
        Vector2 statusSize = labelFont.MeasureString(status);
        float statusY = Bounds.Bottom - pad - statusSize.Y;
        var labelPos = new Vector2(Bounds.X + pad, statusY);
        TextItem.doFontLabel(labelPos, status, labelFont, Color.DarkGray * fade);

        // When complete, show password just above the status line
        if (lifetime >= maxLifetime && os.connectedComp != null && os.connectedComp.firewall != null)
        {
            string pw = "Password: " + os.connectedComp.firewall.solution;
            Vector2 pwSize = smallFont.MeasureString(pw);
            var pwPos = new Vector2(Bounds.X + pad, statusY - lineGap - pwSize.Y);
            TextItem.doFontLabel(pwPos, pw, smallFont, Color.DarkGray * fade);
        }
    }

    

    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= maxLifetime && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            Programs.getComputer(os, targetIP).firewall.solved = true;
        }
        else
        {
            lifetime += t;
        }
        // advance flicker timer for green noise
        flickerTimer += t;
    }

    private void PrepareDecodeGrid()
    {
        // Choose a compact font
        gridFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font;

        const float pad = 8f;            // outer margin inside the window
        const float topGap = 8f;         // gap between top edge and grid
        float reservedBottom = GetLabelReservedHeight(lifetime >= maxLifetime);

        // Compute an area that sits below the top with margin and above the label block
        float top = Bounds.Y + pad + topGap;
        float bottomLimit = Bounds.Bottom - pad - reservedBottom;
        float height = Math.Max(0f, bottomLimit - top);

        // Cell size from font metrics
        Vector2 m = gridFont.MeasureString("W");
        cellW = Math.Max(4f, m.X);
        cellH = Math.Max(8f, m.Y + 1f);

        float availW = Math.Max(0f, Bounds.Width - pad * 2f);
        float availH = Math.Max(0f, height);

        gridCols = (int)Math.Floor(availW / cellW);
        gridRows = (int)Math.Floor(availH / cellH);

        if (gridCols < 1 || gridRows < 1)
        {
            gridFrozenChars = Array.Empty<char>();
            gridThreshold = Array.Empty<float>();
            return;
        }

        int total = gridCols * gridRows;
        // Limit density to keep draw cost reasonable
        if (total > 5000)
        {
            float scale = (float)Math.Sqrt(total / 5000.0);
            cellW *= scale;
            cellH *= scale;
            gridCols = Math.Max(1, (int)Math.Floor(availW / cellW));
            gridRows = Math.Max(1, (int)Math.Floor(availH / cellH));
            total = gridCols * gridRows;
        }

        gridFrozenChars = new char[total];
        gridThreshold = new float[total];

        for (int r = 0; r < gridRows; r++)
        {
            for (int c = 0; c < gridCols; c++)
            {
                int i = r * gridCols + c;
                // Set a final frozen char which will be shown in white when decoded
                gridFrozenChars[i] = cipherChars[rng.Next(cipherChars.Length)];
                // Threshold with a slight diagonal bias for a nice sweep
                float baseTh = (float)rng.NextDouble();
                float bias = ((float)r / Math.Max(1, gridRows - 1) + (float)c / Math.Max(1, gridCols - 1)) * 0.5f;
                gridThreshold[i] = MathHelper.Clamp(baseTh * 0.55f + bias * 0.45f, 0f, 1f);
            }
        }
    }

    private void EnsureDecodeGridUpToDate()
    {
        var currentFont = GuiData.tinyfont ?? GuiData.smallfont ?? GuiData.font;
        if (!gridReady || currentFont != gridFont)
        {
            PrepareDecodeGrid();
            gridReady = true;
            return;
        }

        const float pad = 8f;
        const float topGap = 8f;
        float reservedBottom = GetLabelReservedHeight(lifetime >= maxLifetime);
        float top = Bounds.Y + pad + topGap;
        float bottomLimit = Bounds.Bottom - pad - reservedBottom;
        float height = Math.Max(0f, bottomLimit - top);

        float availW = Math.Max(0f, Bounds.Width - pad * 2f);
        float availH = Math.Max(0f, height);
        int cols = (int)Math.Floor(availW / Math.Max(1f, cellW));
        int rows = (int)Math.Floor(availH / Math.Max(1f, cellH));
        if (cols != gridCols || rows != gridRows)
        {
            PrepareDecodeGrid();
            gridReady = true;
        }
    }

    private void DrawDecodeGrid(float progress)
    {
        if (gridFrozenChars == null || gridFrozenChars.Length == 0) return;

        const float pad = 8f;
        const float topGap = 8f;
        float reservedBottom = GetLabelReservedHeight(lifetime >= maxLifetime);

        float startX = Bounds.X + pad;
        float top = Bounds.Y + pad + topGap;
        float bottomLimit = Bounds.Bottom - pad - reservedBottom;
        float startY = top;

        // Update rate for flicker on encoded cells
        bool doFlickerUpdate = false;
        if (flickerTimer >= FlickerInterval)
        {
            flickerTimer = 0f;
            doFlickerUpdate = true;
        }

        float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;

        for (int r = 0; r < gridRows; r++)
        {
            float y = startY + r * cellH;
            if (y + cellH > bottomLimit) break; // stay within content area
            for (int c = 0; c < gridCols; c++)
            {
                int i = r * gridCols + c;
                float x = startX + c * cellW;

                float th = gridThreshold[i];
                // Local transition width
                const float transWidth = 0.18f;
                float a = MathHelper.Clamp((progress - th) / transWidth + 0.5f, 0f, 1f);
                a = a * a * (3f - 2f * a); // smoothstep

                // Before decode: flickering dark-green glyphs
                // After decode: stable white glyphs
                char ch;
                if (a < 1f)
                {
                    // still encoded; flicker
                    if (doFlickerUpdate)
                    {
                        // occasionally change frozen preview too so decode result isn't too predictable
                        if (rng.NextDouble() < 0.02) gridFrozenChars[i] = cipherChars[rng.Next(cipherChars.Length)];
                    }
                    ch = cipherChars[rng.Next(cipherChars.Length)];
                    float pulse = 0.85f + 0.15f * (float)Math.Sin((i * 0.11f) + time * 9f);
                    var encCol = new Color((int)(20 * pulse), (int)(160 * pulse), (int)(20 * pulse)); // dark green
                    GuiData.spriteBatch.DrawString(gridFont, ch.ToString(), new Vector2(x, y), encCol * (fade * (1f - a)));
                }
                if (a > 0f)
                {
                    // decoded glyph stays stable, in white
                    ch = gridFrozenChars[i];
                    GuiData.spriteBatch.DrawString(gridFont, ch.ToString(), new Vector2(x, y), Color.White * (fade * a));
                }
            }
        }
    }

    private float GetLabelReservedHeight(bool includePassword)
    {
        var labelFont = GuiData.smallfont ?? gridFont ?? GuiData.font;
        var smallFont = GuiData.tinyfont ?? GuiData.smallfont ?? labelFont;
        float hLabel = labelFont.MeasureString("DEFACING..").Y;
        float reserved = hLabel;
        if (includePassword)
        {
            float hPass = smallFont.MeasureString("Password: XXXX").Y;
            const float lineGap = 4f;  // spacing between password and status lines
            reserved += lineGap + hPass;
        }
        const float contentGap = 8f; // gap between grid content and label block
        return reserved + contentGap;
    }
}
