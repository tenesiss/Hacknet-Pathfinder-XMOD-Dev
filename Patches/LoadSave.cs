using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Text;
using System.Threading.Tasks;
using Hacknet;
using Hacknet.Extensions;

namespace XMOD.Patches
{
    [HarmonyPatch(typeof(OS), nameof(OS.loadSaveFile))]
    class LoadSave
    {
        static void Postfix()
        {
            ParalellMissionManager.ReadMissions();
            SaveData save = Reader.ReadXMODSave(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODSave.xml");
            ParalellMissionManager.currentMissions = save.activeMissions;
            XMOD.sendIRCEnabled = save.CanSendIRCMessage;
            XMOD.sendIRCName = save.IRCMessageName;
        }
    }
}
