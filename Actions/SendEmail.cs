using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using Pathfinder;
using Pathfinder.Util.XML;
using System.Xml.Linq;
using System.Collections.Generic;

public class SendEmailAction : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public string author;
    public string content;
    [XMLStorage]
    public string subject;
    public List<String> attachments;
    public List<string> attachmentsRaw = new List<string>();
    private List<ElementInfo> attachmentsX;
    override public void Trigger(OS os)
    {
        MailServer mailServer = (MailServer)os.netMap.mailServer.getDaemon(typeof(MailServer));
        mailServer.addMail(MailServer.generateEmail(subject, content, author, LoadAttachments()), os.defaultUser.name);
    }
    private List<string> LoadAttachments()
    {
        List<string> finalAttachments = new List<string>();
        OS osP = OS.currentInstance;
        if(attachmentsRaw != null && attachmentsRaw.Count > 0)
        {
            attachmentsRaw.ForEach(cld =>
            {
                int indxDots = cld.IndexOf(":");
                string firstName = cld.Substring(0, indxDots); // link:playerComp | comp | note
                string values = cld.Substring(indxDots + 1, cld.Length - (firstName.Length + 1));
                // [link]:(proComp)
                // []: firstName
                // (): values
                switch (firstName)
                {
                    case "link":
                        Computer comp = Pathfinder.Util.ComputerLookup.FindById(values);
                        finalAttachments.Add("link#%#" + values + "#%#" + comp.ip);
                        break;
                    case "account":
                        string[] accData = values.Split(",".ToCharArray());
                        Computer compAcc = Pathfinder.Util.ComputerLookup.FindById(accData[0]);
                        finalAttachments.Add("account#%#" + accData[0] + "#%#" + compAcc.ip + "#%#" + accData[1] + "#%#" + accData[2]);
                        break;
                    case "note":
                        string[] noteData = values.Split(",".ToCharArray());
                        finalAttachments.Add("note#%#" + noteData[0] + "#%#" + noteData[1]);
                        break;
                }
            });
            return finalAttachments;
        } else
        {
            return finalAttachments;
        }
    }
    public override void LoadFromXml(ElementInfo info)
    {
        base.LoadFromXml(info);
        ElementInfo attachmentsD = info.Children.Find(x => x.Name == "attachments");
        ElementInfo bodyD = info.Children.Find(x => x.Name == "body");
        content = bodyD.Content;
        attachmentsX = attachmentsD.Children;
        OS main_os = OS.currentInstance;
        attachmentsD.Children.ForEach(xc =>
        {
            switch (xc.Name)
            {
                case "link":
                    // attachmentsEmail.Add("link#%#" + compLink.name + "#%#" + compLink.ip);
                        attachmentsRaw.Add("link:" + xc.Attributes["comp"]);
                    break;
                case "account":
                    // attachmentsEmail.Add("account#%#" + compAccount.name + "#%#" + compAccount.ip + "#%#" + xc.Attributes["user"] + "#%#" + xc.Attributes["pass"]); 
                    string accData = "account:" + xc.Attributes["comp"] + "," + xc.Attributes["user"] + "," + xc.Attributes["pass"];
                        attachmentsRaw.Add(accData);
                    break;
                case "note":
                    string titleNote = xc.Attributes["title"];
                    string bodyNote = xc.Content;
                    // attachmentsEmail.Add("note#%#" + titleNote + "#%#" + bodyNote);
                    attachmentsRaw.Add("note:" + titleNote + "," + bodyNote);
                    break;
            }
        });
    }
    public override XElement GetSaveElement()
    {
        XElement element =  base.GetSaveElement();
        List<XElement> attachmentsLD = new List<XElement>();
        XElement ELbody = new XElement("body", content);
        attachmentsX.ForEach(eld =>
        {
            attachmentsLD.Add(ConvertElementInfoToXElement(eld));
        });
        XElement ELattachments = new XElement("attachments");
        if(attachmentsLD.Count > 0)
        {
            ELattachments.Add(attachmentsLD);
        }
        element.Add(ELbody);
        element.Add(ELattachments);
        return element;
    }
    private XElement ConvertElementInfoToXElement(ElementInfo el)
    {
        XElement Xel = new XElement(el.Name, el.Content);
        foreach(KeyValuePair<string, string> elAt in el.Attributes)
        {
            Xel.SetAttributeValue(elAt.Key, elAt.Value);
        }
        if(el.Children.Count > 0)
        {
            for (int i = 0; i >= el.Children.Count; i++)
            {

                Xel.Add(ConvertElementInfoToXElement(el.Children[i]));
            }
            return Xel;
        } else
        {
            return Xel;
        }
    }
}
