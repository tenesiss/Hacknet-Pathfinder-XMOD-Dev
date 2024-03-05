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
    [HarmonyPatch(typeof(OS), nameof(OS.saveGame))]
    class SaveMissions
    {
        static void Postfix()
        {
            if (OS.currentInstance.SaveUserAccountName is null) return;
            Reader.WriteXMODSave(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODSave.xml");
        }
    }
}
