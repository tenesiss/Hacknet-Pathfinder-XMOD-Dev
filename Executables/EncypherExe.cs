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
                }
        }
    
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        if (lifetime < maxLifetime / 3)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 - 8f, bounds.Center.Y), "ENCRYPTING", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime / 3 && lifetime < maxLifetime / 2)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 - 8f, bounds.Center.Y), "ENCRYPTING.", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime / 2 && lifetime < maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 - 8f, bounds.Center.Y), "ENCRYPTING..", GuiData.font, Color.DarkGray * fade);
        }
        else if (lifetime >= maxLifetime)
        {
            TextItem.doFontLabel(new Vector2(Bounds.Center.X / 2 - 3f, bounds.Center.Y), "ENCRYPTED", GuiData.font, Color.DarkGray * fade);
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
}