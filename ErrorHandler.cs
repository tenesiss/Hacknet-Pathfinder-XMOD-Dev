using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    internal class ErrorHandler
    {
        public static List<Error> recordedErrors = new List<Error>();

        public static Error CreateError(string message, int severity, bool isPassive)
        {
            return new Error(message, isPassive, severity);
        }
    }
}
