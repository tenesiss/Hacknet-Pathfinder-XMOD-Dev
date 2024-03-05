using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;
using Hacknet;
using System;
using System.Collections.Generic;
using Hacknet.Daemons.Helpers;
using Microsoft.Xna.Framework;
using System.Linq;
using Hacknet.Extensions;
using System.Runtime.Remoting.Messaging;
using XMOD;
using System.IO;

namespace XMOD
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class XMOD : HacknetPlugin
    {
        public const string ModGUID = "tenesiss.XMOD";
        public const string ModName = "XMOD";
        public const string ModVer = "1.0.0";
        private List<string> textArr = new List<string>();
        private string textFile;
        private int i;
        private string idLog;
        static public bool sendIRCEnabled = false;
        static public string sendIRCName;
        private string logFilename;
        private string logContent;
        private string final_filename;
        public static string FileFilter(string filename, OS os)
        {
            int i_file;
            bool is_ok = false;
            string filename_t = null;
            Folder actualFolder = Programs.getCurrentFolder(os);

            if (actualFolder.containsFile(filename))
            {
                if (!actualFolder.containsFile(filename + "(1)"))
                {
                    return filename + "(1)";
                }
                else
                {
                    i_file = 1;
                    is_ok = false;
                    while (actualFolder.containsFile(filename + "(" + i_file + ")") && is_ok == false)
                    {
                        if (!actualFolder.containsFile(filename + "(" + (i_file + 1) + ")"))
                        {
                            i_file++;
                            is_ok = true;
                            filename_t = filename + "(" + i_file + ")";
                        } else
                        {
                            i_file++;
                        }
                            
                    }
                    return filename_t;
                }
            } else
            {
                return filename;
            }
        }

        public static string FullPath(string dir)
        {
            return ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + dir;
        }

        public static bool ConvertToBool(string boolinstring)
        {
            return (boolinstring == "true" ? true : false);
        }

        private void LoadExe()
        {
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<TransferExe>("#TRANSFER_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<SocketExe>("#SOCKET_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<EnBreak>("#EN_BREAK_EXE#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<EncypherExe>("#ENCYPHER_PROGRAM_EXE#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<VersionCrack>("#VERSION_CONTROL_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<EosCrack>("#EOS_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<SMTPFastCrack>("#SMTP_FAST_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<ShadingCrack>("#SHADING_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<PoliceCrack>("#POLICE_CRACK#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<FirewallDefacer>("#FIREWALL_DEFACER_PROGRAM#");
            Pathfinder.Executable.ExecutableManager.RegisterExecutable<TXCrack>("#TX_CRACK#");
        }
        private void LoadPorts()
        {
            Pathfinder.Port.PortManager.RegisterPort("socket", "Socket Connect", 3500);
            Pathfinder.Port.PortManager.RegisterPort("shading", "Shading Controller", 591);
            Pathfinder.Port.PortManager.RegisterPort("police", "Police Security Protocols", 188);
            Pathfinder.Port.PortManager.RegisterPort("tx", "TX Port", 500); // TX = Tech Xeno - Like a company or smth
        }

        private void mkdirRun(OS os, string[] args)
        {
            
            if (!os.connectedComp.PlayerHasAdminPermissions())
            {
                os.write("Insufficient Permissions");
            } else
            {
                if(args.Length < 1)
                {
                    os.write("You must input a valid folder name");
                } else
                {
                    if (Programs.getCurrentFolder(os).searchForFolder(args[1]) == null)
                    {
                        idLog = "@" + (int)OS.currentElapsedTime;
                        // @[time]_FileCreated:_by_[ip]_-_file:[filename]
                        logFilename = idLog + "_FolderCreated:_by_" + os.thisComputer.ip + "_-_folder:" + args[1];
                        // @[time] FileCreated: by [ip] - file:[filename]
                        logContent = idLog + " FolderCreated: by " + os.thisComputer.ip + " - folder:" + args[1];
                        os.connectedComp.getFolderFromPath("/log").files.Add(new FileEntry(logContent, logFilename));
                        Programs.getCurrentFolder(os).folders.Add(new Folder(args[1]));
                    } else
                    {
                        os.write("Folder already exists!");
                    }
                }
            }
        }

        private void sendIRCRun(OS os, string[] args)
        {
            if (sendIRCEnabled)
            {
                if (!os.connectedComp.daemons.Exists(d => d is IRCDaemon || d is DLCHubServer))
                {
                    os.write("You are not connected to an IRC Server.");
                }
                else
                {
                    if (args[1] == null)
                    {
                        os.write("You must enter a message to send.");
                    } else
                    {
                        string message = string.Join(" ", args.Where(a => a != args[0]));
                        dynamic ircd = os.connectedComp.getDaemon(typeof(IRCDaemon)) ?? os.connectedComp.getDaemon(typeof(DLCHubServer));
                        IRCSystem ircsys = ircd.System;
                        DateTime dt = DateTime.Now;
                        string timestamp = dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00");

                        ircsys.AddLog(sendIRCName ?? os.username, message, timestamp);
                    }
                    
                }
            }
        }

        private void mkfileRun(OS os, string[] args)
        {
            if (!os.connectedComp.PlayerHasAdminPermissions())
            {
                os.write("Insufficient Permissions");
            }
            else
            {
                if (args.Length < 1)
                {
                    os.write("You must input the file name");
                    os.write("Aborted");
                }
                else
                {
                    if (args.Length < 2)
                    {
                        os.write("You must input a content for the file");
                        os.write("Aborted");
                    }
                    else
                    {
                        final_filename = FileFilter(args[1], os);
                                i = 2;
                                textArr.Add(args[2]);
                                while (i < args.Length - 1)
                                {
                                    textArr.Add(args[i + 1]);
                                    i++;
                                }
                                textFile = string.Join(" ", textArr.ToArray());
                                idLog = "@" + (int)OS.currentElapsedTime;
                                Programs.getCurrentFolder(os).files.Add(new FileEntry(textFile, final_filename));
                                // @[time]_FileCreated:_by_[ip]_-_file:[filename]
                                logFilename = idLog + "_FileCreated:_by_" + os.thisComputer.ip + "_-_file:" + final_filename;
                                // @[time] FileCreated: by [ip] - file:[filename]
                                logContent = idLog + " FileCreated: by " + os.thisComputer.ip + " - file:" + final_filename;
                                os.connectedComp.getFolderFromPath("/log").files.Add(new FileEntry(logContent, logFilename));
                                textArr.Clear();

                    }
                }
            }
        }

        public override bool Load()
        {
            HarmonyInstance.PatchAll();

            // Load/Register Executables
            LoadExe();
            // Load/Register Ports
            LoadPorts();


            

            Pathfinder.Mission.GoalManager.RegisterGoal<FileCreationGoal>("filecreation");
            
            // mkfile args: ["filename", '"hello', "i'm", "robert", '"'] - at least a prototype
            Pathfinder.Command.CommandManager.RegisterCommand("mkfile", mkfileRun);
            Pathfinder.Command.CommandManager.RegisterCommand("mkdir", mkdirRun);
            Pathfinder.Command.CommandManager.RegisterCommand("chat", sendIRCRun);

            Pathfinder.Action.ConditionManager.RegisterCondition<ConditionFileDeletion>("FileDeleted");
            Pathfinder.Action.ConditionManager.RegisterCondition<ConditionFileCreation>("FileCreated");
            Pathfinder.Action.ConditionManager.RegisterCondition<ConditionHasPoints>("HasPoints");
            Pathfinder.Action.ConditionManager.RegisterCondition<ConditionPlayerSentMessage>("PlayerSentMessage");
            Pathfinder.Action.ConditionManager.RegisterCondition<HasFlagsNew>("HasFlags");
            Pathfinder.Action.ConditionManager.RegisterCondition<DoesNotHaveFlagsNew>("DoesNotHaveFlags");

            Pathfinder.Action.ActionManager.RegisterAction<CmdRunAction>("CmdRun");
            Pathfinder.Action.ActionManager.RegisterAction<AddPoints>("AddPoints");
            Pathfinder.Action.ActionManager.RegisterAction<RemovePoints>("RemovePoints");
            Pathfinder.Action.ActionManager.RegisterAction<SendEmailAction>("SendMail");
            Pathfinder.Action.ActionManager.RegisterAction<EnableIRCMessaging>("EnableIRCMessaging");
            Pathfinder.Action.ActionManager.RegisterAction<LoadMissionX>("LoadMissionX");
            Pathfinder.Action.ActionManager.RegisterAction<CancelMissionX>("CancelMissionX");

            return true;
        }
    }
}
