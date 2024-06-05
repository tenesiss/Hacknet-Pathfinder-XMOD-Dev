using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class SaveData
    {
        public List<Mission> activeMissions;
        public bool CanSendIRCMessage;
        public string IRCMessageName;
        public List<DNSRecord> dnsRecords;

        public SaveData(List<Mission> activeMissions, bool CanSendIRCMessage, string IRCMessageName, List<DNSRecord> dnsRecords) 
        { 
            this.activeMissions = activeMissions;
            this.CanSendIRCMessage = CanSendIRCMessage;
            this.IRCMessageName = IRCMessageName;
            this.dnsRecords = dnsRecords;
        }
    }
}
