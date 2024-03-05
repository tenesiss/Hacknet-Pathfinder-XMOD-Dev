using Hacknet;
using Hacknet.Extensions;
using Hacknet.Mission;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Hacknet.MailServer;

namespace XMOD
{
    public class Mission
    {
        public string identifier;
        public bool silent;
        public bool activeCheck;
        public OS os;
        public List<MisisonGoal> goals;
        public string missionStartFunctionName;
        public string missionStartFunctionValue;
        public string missionEndFunctionName;
        public string missionEndFunctionValue;
        public Mission nextMission;
        public EmailData missionEmailData;
        public List<Mission> branches = new List<Mission>();

        public Mission(string identifier, bool isSilent, bool activeCheck, OS os, List<MisisonGoal> goals, string startFunctionName, string startFunctionValue, string endFunctionName, string endFunctionValue,Mission nextMission, List<Mission> branches)
        {
            this.identifier = identifier;
            this.silent = isSilent;
            this.activeCheck = activeCheck;
            this.os = os;
            this.goals = goals;
            this.missionStartFunctionName = startFunctionName;
            this.missionStartFunctionValue = startFunctionValue;
            this.missionEndFunctionName = endFunctionName;
            this.missionEndFunctionValue = endFunctionValue;
            this.nextMission = nextMission;
            this.branches = branches;
        }
        public void Update()
        {
            if(activeCheck && this.isComplete())
            {
                finish();
            }
            if (!isComplete())
            {
                for(int v=0; v < branches.Count; v++)
                {
                    branches[v].Update();
                    if (branches[v].isComplete())
                    {
                        ParalellMissionManager._removeMission(this);
                        OS.currentInstance.saveGame();
                        break;
                    }
                }
            }
        }
        public void finish()
        {
            if (ParalellMissionManager.currentMissions.Contains(this))
            {
                ParalellMissionManager._removeMission(this);
            }
            if (missionEndFunctionName != null)
            {
                MissionFunctions.runCommand(int.Parse(missionEndFunctionValue), missionEndFunctionName);
            }
            if (nextMission != null)
            {
                ParalellMissionManager._addMission(nextMission);
            }
            if(OS.currentInstance.SaveUserAccountName != null) OS.currentInstance.saveGame();
        }
        public bool isComplete()
        {
            List<bool> iscomplt = new List<bool>();
            for(int i=0;i < goals.Count;i++)
            {
                if (!goals[i].isComplete())
                {
                    iscomplt.Add(false);
                } else
                {
                    iscomplt.Add(true);
                }
            }
            if(iscomplt.Any(p => p == false))
            {
                return false;
            } else
            {
                return true;
            }
        }
    }
}
