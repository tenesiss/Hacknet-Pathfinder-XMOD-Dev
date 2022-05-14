using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hacknet;
using HarmonyLib;
using System.Threading.Tasks;

namespace XMOD.OverwriteActions
{
    internal static class ReplacementsUtils
    {
        private static StringBuilder builder = new StringBuilder();
        private static OS aOS = Pathfinder.Util.ComputerLookup.FindById("playerComp").os;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hacknet.ComputerLoader), nameof(Hacknet.ComputerLoader.filter))]
        public static void filter(string s, ref string ___result)
        {
            string nText = ___result;
            aOS.netMap.nodes.ForEach(nd =>
            {
                nText = nText
                .Replace(String.Format("#DISCONNECT_LOG:{0}#", nd.ip), formatLog("Disconnected", nd.ip))
                .Replace(String.Format("#CONNECT_LOG:{0}#", nd.ip), formatLog("Connected", nd.ip))
                .Replace(String.Format("#DISCONNECT_LOG_CONTENT:{0}#", nd.ip), formatLog("Disconnected", nd.ip).Replace("_", " "))
                .Replace(String.Format("#CONNECT_LOG_CONTENT:{0}#", nd.ip), formatLog("Connected", nd.ip).Replace("_", " "));
            });
            ___result = nText;
        }

        private static string filterip(string sp)
        {
            int startIn = sp.IndexOf(":") + 1;
            int endIn = (sp.LastIndexOf("#") - 1) - startIn;
            string ipDS = sp.Substring(startIn, endIn);
            return ipDS;
        }

        private static string formatLog(string msg, string ip)
        {
            return "@" + OS.currentElapsedTime + "_" + ip + "_" + msg; // @344_12.3.22.1_Disconnected (Example)
        }
    }
}
