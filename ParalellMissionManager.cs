using Hacknet;
using Hacknet.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMOD
{
    public class ParalellMissionManager
    {
        public static List<Mission> currentMissions = new List<Mission>();
        public static string extensionPath = ExtensionLoader.ActiveExtensionInfo.FolderPath;
        public static string[] missionFiles = Directory.GetFiles(extensionPath + "/Missions");
        public static List<Mission> missions = new List<Mission>();
        public static void ReadMissions()
        {
            List<Mission> newMissions = new List<Mission>();
            for(int i=0; i < missionFiles.Length; i++)
            {
                XDocument missionDoc = XDocument.Load(missionFiles[i]);
                Mission MissionObject = Reader.ReadMission(missionDoc);
                newMissions.Add(MissionObject);
            }
            missions = newMissions;
        }
        public static void _addMission(Mission mission)
        {
            if (currentMissions.Find(m => m.identifier == mission.identifier) != null)
            {
                ErrorHandler.CreateError("Mission already loaded", 1, false).Emit();
                return;
            }
            currentMissions.Add(mission);
            if (mission.missionStartFunctionName != null)
            {
                MissionFunctions.runCommand(int.Parse(mission.missionStartFunctionValue), mission.missionStartFunctionName);
            }
        }
        public static void _removeMission(Mission mission)
        {
            currentMissions.Remove(mission);
        }
        public static void _removeMission(string missionIdentifier)
        {
            for (int i=0;i < currentMissions.Count;i++)
            {
                if (currentMissions[i].identifier == missionIdentifier)
                {
                    currentMissions.RemoveAt(i);
                }
            }
        }
    }
}
