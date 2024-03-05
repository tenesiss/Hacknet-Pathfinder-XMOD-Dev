using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using Hacknet;
using System;
using Hacknet.Daemons.Helpers;
using System.Linq;

public class ConditionPlayerSentMessage : Pathfinder.Action.PathfinderCondition
{
    [XMLStorage]
    public string target;
    [XMLStorage]
    public string content;
    [XMLStorage]
    public string requiredFlags = "";
    [XMLStorage]
    public string checkOnce = "false";
    [XMLStorage]
    public string considerPrevious = "true";
    private int previousMsgsSize = -1;
    private int count = 0;

    public override bool Check(object os_obj)
    {
        
        Computer compTarget = Pathfinder.Util.ComputerLookup.FindById(target);
        OS finalOS = (OS)os_obj;
        dynamic deam = null;

        if (compTarget.getDaemon(typeof(IRCDaemon)) != null)
        {
            deam = compTarget.getDaemon(typeof(IRCDaemon));
        } else if(compTarget.getDaemon(typeof(DLCHubServer)) != null)
        {
            deam = compTarget.getDaemon(typeof(DLCHubServer));
        }
        if(deam != null)
        {
            IRCSystem ircsys = deam.System;
            string[] msgs = ircsys.ActiveLogFile.data.Split(new string[] { "\n#" }, StringSplitOptions.None);
            if(previousMsgsSize >= 0)
            {
                msgs = msgs.Skip(previousMsgsSize).ToArray();
            }
            if (count == 0 && considerPrevious == "false")
            {
                previousMsgsSize = msgs.Length;
                finalOS.write("a");
                if (checkOnce == "true")
                {
                    finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                }
                count++;
                return false;
            }
            for (int i=0;i < msgs.Length;i++)
            {
                IRCSystem.IRCLogEntry log = IRCSystem.IRCLogEntry.Deserialize(msgs[i]);
                if(log.Message == content && log.Author == (XMOD.XMOD.sendIRCName ?? finalOS.username))
                {
                    if (requiredFlags != "")
                    {
                        string[] requiredFlagsToWork = requiredFlags.Split(new string[] { "," }, StringSplitOptions.None);
                        for (int v = 0; v < requiredFlagsToWork.Length; v++)
                        {
                            if (!finalOS.Flags.HasFlag(requiredFlagsToWork[v]))
                            {
                                if (checkOnce == "true")
                                {
                                    finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
                                }
                                else
                                {
                                    count++;
                                }
                                return false;
                            }
                        }
                    }
                     return true;
                }
            }
            if (checkOnce == "true")
            {
                finalOS.delayer.Post(ActionDelayer.NextTick(), () => { finalOS.ConditionalActions.Actions.RemoveAll(ca => ca.Condition == this); });
            } else
            {
                count++;
            }
            return false;
        } else
        {
            return false;
        }
    }
}
