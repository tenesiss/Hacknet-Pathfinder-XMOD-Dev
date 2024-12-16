using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class ExeSettings
    {
        public static Dictionary<string, float> completeTime = new Dictionary<string, float>()
        {
            {"SocketCrack", 22f},
            {"EnBreak", 80f},
            {"EncypherExe", 10f},
            {"EosCrack", 21f},
            {"FirewallDefacerExe", 19f},
            {"PoliceCrack", 18f},
            {"ShadingCrack", 18f},
            {"SMTPFastCrack", 7f},
            {"TransferCrack", 20f},
            {"TXCrack", 15f},
            {"VersionCrack", 19f}
        };
        public static Dictionary<string, int> ram = new Dictionary<string, int>() {
            {"SocketCrack", 230},
            {"EnBreak", 450},
            {"EncypherExe", 70},
            {"EosCrack", 275},
            {"FirewallDefacerExe", 190},
            {"PoliceCrack", 256},
            {"ShadingCrack", 252},
            {"SMTPFastCrack", 250},
            {"TransferCrack", 255},
            {"TXCrack", 247},
            {"VersionCrack", 244}
        };

    }
}
