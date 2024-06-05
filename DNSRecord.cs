using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class DNSRecord
    {
        public string domain;
        public string ip;
        public string registeredBy;

        public DNSRecord(string domain, string ip, string registeredBy)
        {
            this.domain = domain;
            this.ip = ip;
            this.registeredBy = registeredBy;
        }
    }
}
