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
        XElement ELbody = new XElement("body");
        XElement ELAttLink = new XElement("link", "comp");
        XElement ELAttAccount = new XElement("account", new object[] {"comp", "user", "pass"});
        XElement ElAttNote = new XElement("note", "title");
        object[] attachmentsAv = new object[] { ELAttLink, ELAttAccount, ElAttNote };
        XElement ELattachments = new XElement("attachments", attachmentsAv);
        element.Add(ELbody);
        element.Add(ELattachments);
        return element;
    }
}
