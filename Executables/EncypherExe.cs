using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Microsoft.Xna.Framework;
using Pathfinder.Util;
using Pathfinder.Port;
using Pathfinder;
using System;
using System.Collections.Generic;

public class EncypherExe : Pathfinder.Executable.BaseExecutable
{
    public override string GetIdentifier() => "EncypherExe";
    public int num;
    public bool exe = false;
    private string StringToEncrypt;
    private string StringEncrypted;
    private List<int> FolderEnc = new List<int>();
    private int i_file;
    private bool is_ok;
    private List<string> textArr = new List<string>();
    private string textFile;
    private int i;
    private string encFilename;
    public EncypherExe(Rectangle location, OS operatingSystem, string[] args) : base(location, operatingSystem, args)
    {
        this.ramCost = 250;
        this.IdentifierName = "Encypher";
    }
    private int x = 0;
    public override void LoadContent()
    {
        base.LoadContent();
            if(this.Args.Length < 1)
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
    private float lifetime = 0f;
    private float maxLifetime = 10f;
    public override void Draw(float t)
    {
        base.Draw(t);
        drawTarget();
        drawOutline();
        if (lifetime < (maxLifetime-5))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "ENCRYPTING", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime - 5) && lifetime < (maxLifetime-2))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "ENCRYPTING.", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime - 2) && lifetime < (maxLifetime))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "ENCRYPTING..", new Color(255, 0, 0));
        }
        else if (lifetime >= (maxLifetime))
        {
            Hacknet.Gui.TextItem.doLabel(new Vector2(Bounds.Center.X - 100, Bounds.Center.Y - 100), "ENCRYPTING...", new Color(255, 0, 0));
        }
    }

    
    public override void Completed()
    {

    }
    public override void Update(float t)
    {
        base.Update(t);
        if (lifetime >= 10f && isExiting == false && exe == false)
        {
            exe = true;
            isExiting = true;
            StringToEncrypt = Programs.getCurrentFolder(os).searchForFile(this.Args[1]).data;
            FolderEnc.Add(0);
            encFilename = HacknetPluginTemplate.XMOD.FileFilter(this.Args[1]+"_Encrypted.dec", os);
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