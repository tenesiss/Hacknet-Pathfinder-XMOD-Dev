using Hacknet.Extensions;
using Hacknet.PlatformAPI.Storage;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XMOD.Patches
{
    [HarmonyPatch(typeof(SaveFileManager), nameof(SaveFileManager.DeleteUser))]
    public class DeleteSave
    {
        static void Postfix(string username)
        {
            if (File.Exists(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODSave.xml"))
            {
                XDocument saveDoc = XDocument.Load(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODSave.xml");
                saveDoc.Element("Saves").Elements("AccountSave").Where(ac => ac.Attribute("accountName").Value == username).Remove();
                saveDoc.Save(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/XMODSave.xml");
            }
        }
    }
}
