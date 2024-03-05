using Hacknet;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD.Patches
{
    [HarmonyPatch(typeof(OS), nameof(OS.Update))]
    class Update
    {
        static void Prefix()
        {
            for(int i=0;i < ParalellMissionManager.currentMissions.Count;i++)
            {
                ParalellMissionManager.currentMissions[i].Update();
            }
        }
    }
}
