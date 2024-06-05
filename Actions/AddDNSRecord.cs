using System;
using System.Xml;
using Hacknet;
using BepInEx;
using BepInEx.Hacknet;
using Pathfinder.Util;
using XMOD;
using Pathfinder;
using Pathfinder.Util.XML;

public class AddDNSRecord : Pathfinder.Action.DelayablePathfinderAction
{
    [XMLStorage]
    public string ip;
    [XMLStorage]
    public string domain;
    [XMLStorage]
    public string registeredBy;
    override public void Trigger(OS os)
    {
        XMOD.XMOD.DNSData.Add(new DNSRecord(domain, ip, registeredBy));
    }
}
