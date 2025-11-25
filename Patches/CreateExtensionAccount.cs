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
    [HarmonyPatch(typeof(ExtensionLoader), nameof(ExtensionLoader.LoadNewExtensionSession))]
    class CreateExtensionAccount
    {
        static void Postfix()
        {
            XConfig config = Reader.ReadXMOConfig(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODConfig.xml");
            if (config != null)
            {
                XMOD.config = config;
                XMOD.connections = config.connections;
            }
        }
    }
}
