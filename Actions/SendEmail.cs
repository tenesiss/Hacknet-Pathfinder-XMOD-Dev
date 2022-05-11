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
    private List<ElementInfo> attachmentsX;
    override public void Trigger(OS os)
    {
        MailServer mailServer = (MailServer)os.netMap.mailServer.getDaemon(typeof(MailServer));
        mailServer.addMail(MailServer.generateEmail(subject, content, author, attachments), os.defaultUser.name);
    }
    public override void LoadFromXml(ElementInfo info)
    {
        base.LoadFromXml(info);
        ElementInfo attachmentsD = info.Children.Find(x => x.Name == "attachments");
        ElementInfo bodyD = info.Children.Find(x => x.Name == "body");
        content = bodyD.Content;
        attachmentsX = attachmentsD.Children;
        List<String> attachmentsEmail = new List<String>();
        OS main_os = Pathfinder.Util.ComputerLookup.FindById("playerComp").os;
        attachmentsD.Children.ForEach(xc =>
        {
            switch (xc.Name)
            {
                case "link":
                    Computer compLink = Pathfinder.Util.ComputerLookup.FindById(xc.Attributes["comp"]);
                    if(compLink != null)
                    {
                        attachmentsEmail.Add("link#%#" + compLink.name + "#%#" + compLink.ip);
                    }
                    break;
                case "account":
                    Computer compAccount = Pathfinder.Util.ComputerLookup.FindById(xc.Attributes["comp"]);
                    if(compAccount != null)
                    {
                        attachmentsEmail.Add("account#%#" + compAccount.name + "#%#" + compAccount.ip + "#%#" + xc.Attributes["user"] + "#%#" + xc.Attributes["pass"]);
                    }
                    break;
                case "note":
                    string titleNote = xc.Attributes["title"];
                    string bodyNote = xc.Content;
                    attachmentsEmail.Add("note#%#" + titleNote + "#%#" + bodyNote);
                    break;
            }
        });
        attachments = attachmentsEmail;
    }
    public override XElement GetSaveElement()
    {
        XElement element =  base.GetSaveElement();
        List<XElement> attachmentsLD = new List<XElement>();
        XElement ELbody = new XElement("body", content);
        //  XElement ELAttLink = new XElement("link", attachments.all);
        // XElement ELAttAccount = new XElement("account", new object[] {"comp", "user", "pass"});
        // XElement ElAttNote = new XElement("note", "title");
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
