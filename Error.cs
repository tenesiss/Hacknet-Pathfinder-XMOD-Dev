using Hacknet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    internal class Error
    {
        public string message;
        public bool passive;
        public int severity;
        public double delta;
        public double delta_emit;

        public Error(string message, bool passive, int severity)
        {
            this.message = message;
            this.passive = passive;
            this.severity = severity;
            delta = OS.currentElapsedTime;
        }

        public void Emit()
        {
            if (!passive)
            {
                ConsoleColor rcolor;
                switch (severity)
                {
                    case 0:
                        rcolor = ConsoleColor.White; break;
                    case 1:
                        rcolor = ConsoleColor.Yellow; break;
                    case 2:
                        rcolor = ConsoleColor.DarkYellow; break;
                    case 3:
                        rcolor = ConsoleColor.Red; break;
                    default:
                        rcolor = ConsoleColor.White; break;
                }
                Console.ForegroundColor = rcolor;
                Console.WriteLine("[XMOD}: "+message);
                Console.ResetColor();
            }
            delta_emit = OS.currentElapsedTime;
            ErrorHandler.recordedErrors.Add(this);
        }
    }
}
