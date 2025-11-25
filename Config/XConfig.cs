using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMOD.Config;

namespace XMOD
{
    public class XConfig {
        public List<XConnection> connections = new List<XConnection>();

        public XConfig(){}
        public XConfig(List<XConnection> connections)
        {
            this.connections = connections;
        }
    }
}
