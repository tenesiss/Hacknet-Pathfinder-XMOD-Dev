using Hacknet.Mission;
using Hacknet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hacknet.MailServer;
using System.Xml.Linq;
using Pathfinder.Util;
using Hacknet.Extensions;
using System.IO;

namespace XMOD
{
    public static class Reader
    {
        public static string ad = "#%#";
        
        public static SaveData ReadXMODSave(string path)
        {
            if (!File.Exists(path)) return null;
            XDocument saveDocument = XDocument.Load(path);

            XElement userSaveElement = saveDocument.Element("Saves").Elements("AccountSave").ToList().Find(el => el.Attribute("accountName").Value == OS.currentInstance.SaveUserAccountName);
            if (userSaveElement is null) return null;
           
            List<Mission> activeMissions = new List<Mission>();
            IEnumerable<XElement> missionsEls = userSaveElement.Element("Missions").Elements("Mission");
            for(int i=0;i < missionsEls.Count(); i++)
            {
                Mission theMission = ParalellMissionManager.missions.Find(m => m.identifier == missionsEls.ElementAt(i).Value);
                if(theMission is null) continue;
                activeMissions.Add(theMission);
            }
            List<DNSRecord> dnsRecords = new List<DNSRecord>();
            IEnumerable<XElement> recordsEls = userSaveElement.Element("DNS").Elements("Record");
            for(int i=0;i < recordsEls.Count(); i++)
            {
                dnsRecords.Add(new DNSRecord(recordsEls.ElementAt(i).Value, recordsEls.ElementAt(i).Attribute("ip").Value, recordsEls.ElementAt(i).Attribute("registeredBy").Value));
            }
            XElement IRCMessagingConfig = userSaveElement.Element("IRCMessagingConfig");
            bool canSendIRCMessage = IRCMessagingConfig.Attribute("enabled").Value == "true";
            string IRCMessageName = IRCMessagingConfig.Attribute("name").Value == "NONE" ? null : IRCMessagingConfig.Attribute("name").Value;

            return new SaveData(activeMissions, canSendIRCMessage, IRCMessageName, dnsRecords);
        }
        public static void WriteXMODSave(string path)
        {
            XDocument SaveDocument;
            XElement SavesElement = new XElement("Saves");
            XElement userSaveElement;
            if (!File.Exists(path))
            {
                SaveDocument = new XDocument();
                userSaveElement = new XElement("AccountSave", new XAttribute("accountName", OS.currentInstance.SaveUserAccountName));
                SavesElement.Add(userSaveElement);
                SaveDocument.Add(SavesElement);
            } else
            {
                SaveDocument = XDocument.Load(path);
                userSaveElement = SaveDocument.Element("Saves").Elements("AccountSave").ToList().Find(sv => sv.Attribute("accountName").Value == OS.currentInstance.SaveUserAccountName);
                if(userSaveElement is null)
                {
                    userSaveElement = new XElement("AccountSave", new XAttribute("accountName", OS.currentInstance.SaveUserAccountName));
                    SaveDocument.Element("Saves").Add(userSaveElement);
                }
            }

            userSaveElement.Elements().Remove();

            XElement missionsEl = new XElement("Missions");
            for(int i=0;i < ParalellMissionManager.currentMissions.Count; i++)
            {
                XElement pmissionEl = new XElement("Mission", ParalellMissionManager.currentMissions[i].identifier);
                missionsEl.Add(pmissionEl);
            }
            userSaveElement.Add(missionsEl);

            XElement dnsRecords = new XElement("DNS");
            for (int i = 0; i < XMOD.DNSData.Count; i++)
            {
                XElement record = new XElement("Record", XMOD.DNSData[i].domain);
                record.Add(new XAttribute("ip", XMOD.DNSData[i].ip));
                record.Add(new XAttribute("registeredBy", XMOD.DNSData[i].registeredBy));
                dnsRecords.Add(record);
            }
            XElement IRCConfig = new XElement("IRCMessagingConfig");
            XAttribute canSendIRCMessage = new XAttribute("enabled", XMOD.sendIRCEnabled);
            XAttribute IRCMessageName = new XAttribute("name", XMOD.sendIRCName ?? "NONE");
            IRCConfig.Add(canSendIRCMessage);
            IRCConfig.Add(IRCMessageName);
            userSaveElement.Add(IRCConfig);
            userSaveElement.Add(dnsRecords);

            SaveDocument.Save(path);
        }


        public static Mission ReadMission(XDocument missionDoc)
        {
            XElement missionElement = missionDoc.Element("mission");
            string missionId = missionElement.Attribute("id").Value;
      
            XElement goalsElement = missionElement.Element("goals");
            List<XElement> goalsSub = goalsElement.Elements().ToList();
            List<MisisonGoal> goals = new List<MisisonGoal>();
            for (int i = 0; i < goalsSub.Count; i++)
            {
                goals.Add(ReadGoal(goalsSub[i]));
            }

            XElement missionStartElement = missionElement.Element("missionStart");
            string missionStartFunctionName = missionStartElement.Value != "NONE" ? missionStartElement.Value : null;
            string missionStartFunctionValue = missionStartElement.Attribute("val").Value;

            XElement missionEndElement = missionElement.Element("missionEnd");
            string missionEndFunctionName = missionEndElement.Value != "NONE" ? missionEndElement.Value : null;
            string missionEndFunctionValue = missionEndElement.Attribute("val").Value;

            XElement nextMissionElement = missionElement.Element("nextMission");
            Mission nextMission;
            if (nextMissionElement.Value != "NONE")
            {
                nextMission = ReadMission(XDocument.Load(XMOD.FullPath(nextMissionElement.Value)));
            } else
            {
                nextMission = null;
            }

            XElement branchesElement = missionElement.Element("branchMissions");
            List<Mission> branchesf = new List<Mission>();
            if (branchesElement != null)
            {
                IEnumerable<XElement> branches = branchesElement.Elements();
                foreach (XElement branch in branches)
                {
                    branchesf.Add(ReadMission(XDocument.Load(ExtensionLoader.ActiveExtensionInfo.FolderPath + "/" + branch.Value)));
                }
            }
            return new Mission(missionId, true, true, OS.currentInstance, goals, missionStartFunctionName, missionStartFunctionValue, missionEndFunctionName, missionEndFunctionValue, nextMission, branchesf);
        }
        public static MisisonGoal ReadGoal(XElement goalEl)
        {
            string goalType = goalEl.Attribute("type").Value;
            switch (goalType)
            {
                case "filedeletion":
                    return new FileDeletionMission(goalEl.Attribute("path").Value, goalEl.Attribute("filename").Value, goalEl.Attribute("target").Value, OS.currentInstance);
                case "clearfolder":
                    return new FileDeleteAllMission(goalEl.Attribute("path").Value, goalEl.Attribute("target").Value, OS.currentInstance);
                case "filedownload":
                    return new FileDownloadMission(goalEl.Attribute("path").Value, goalEl.Attribute("filename").Value, goalEl.Attribute("target").Value, OS.currentInstance);
                case "filechange":
                    if (goalEl.Attribute("removal") == null)
                    {
                        return new FileChangeMission(goalEl.Attribute("path").Value, goalEl.Attribute("filename").Value, goalEl.Attribute("target").Value, goalEl.Attribute("keyword").Value, OS.currentInstance);
                    }
                    else
                    {
                        return new FileChangeMission(goalEl.Attribute("path").Value, goalEl.Attribute("filename").Value, goalEl.Attribute("target").Value, goalEl.Attribute("keyword").Value, OS.currentInstance, XMOD.ConvertToBool(goalEl.Attribute("removal").Value));
                    }
                case "getadmin":
                    return new GetAdminMission(goalEl.Attribute("target").Value, OS.currentInstance);
                case "getstring":
                    return new GetStringMission(goalEl.Attribute("target").Value);
                case "delay":
                    return new DelayMission(float.Parse(goalEl.Attribute("time").Value));
                case "hasflag":
                    return new CheckFlagSetMission(goalEl.Attribute("target").Value, OS.currentInstance);
                case "fileupload":
                    if (goalEl.Attribute("decrypt") != null)
                    {
                        if (goalEl.Attribute("decryptPass") != null)
                        {
                            return new FileUploadMission(goalEl.Attribute("path").Value, goalEl.Attribute("file").Value, goalEl.Attribute("target").Value, goalEl.Attribute("destTarget").Value, goalEl.Attribute("destPath").Value, OS.currentInstance, XMOD.ConvertToBool(goalEl.Attribute("decrypt").Value), goalEl.Attribute("decryptPass").Value);
                        }
                        else
                        {
                            return new FileUploadMission(goalEl.Attribute("path").Value, goalEl.Attribute("file").Value, goalEl.Attribute("target").Value, goalEl.Attribute("destTarget").Value, goalEl.Attribute("destPath").Value, OS.currentInstance, XMOD.ConvertToBool(goalEl.Attribute("decrypt").Value));
                        }
                    }
                    else
                    {
                        return new FileUploadMission(goalEl.Attribute("path").Value, goalEl.Attribute("file").Value, goalEl.Attribute("target").Value, goalEl.Attribute("destTarget").Value, goalEl.Attribute("destPath").Value, OS.currentInstance);
                    }
                case "AddDegree":
                    return new AddDegreeMission(goalEl.Attribute("owner").Value, goalEl.Attribute("degree").Value, goalEl.Attribute("uni").Value, float.Parse(goalEl.Attribute("gpa").Value), OS.currentInstance);
                case "wipedegrees":
                    return new WipeDegreesMission(goalEl.Attribute("owner").Value, OS.currentInstance);
                case "sendemail":
                    return new SendEmailMission(goalEl.Attribute("mailServer").Value, goalEl.Attribute("recipient").Value, goalEl.Attribute("subject").Value, OS.currentInstance);
                case "getadminpasswordstring":
                    return new GetAdminPasswordStringMission(goalEl.Attribute("target").Value, OS.currentInstance);
                default:
                    return new DelayMission(1.0f);
            }
        }
       /* public static EmailData ReadEmail(XElement email)
        {
            string sender = email.Element("sender").Value;
            string subject = email.Element("subject").Value;
            string body = email.Element("body").Value;
            List<string> attachments = ReadAttachments(email.Element("attachments").Elements());

            return new EmailData(sender, subject, body, attachments);
        }
        public static List<string> ReadAttachments(IEnumerable<XElement> attachments)
        {
            List<string> at = new List<string>();
            for(int i=0;i < attachments.Count(); i++)
            {
                switch (attachments.ElementAt(i).Name.ToString())
                {
                    case "account":
                        Computer comp = ComputerLookup.FindById(attachments.ElementAt(i).Attribute("comp").Value);
                        string user = attachments.ElementAt(i).Attribute("user").Value;
                        string pass = attachments.ElementAt(i).Attribute("pass").Value;

                        at.Add("account" + ad + comp.idName + ad + comp.ip + ad + user + ad + pass);
                        break;
                    case "note":
                        string title = attachments.ElementAt(i).Attribute("title").Value;
                        string content = attachments.ElementAt(i).Value;

                        at.Add("note" + ad + title + ad + content);
                        break;
                    case "link":
                        Computer comp2 = ComputerLookup.FindById(attachments.ElementAt(i).Attribute("comp").Value);

                        at.Add("link" + ad + comp2.idName + ad + comp2.ip);
                        break;
                }
            }
            return at;
        } */
    }
}
