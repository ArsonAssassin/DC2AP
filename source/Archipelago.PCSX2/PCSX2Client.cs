using Archipelago.PCSX2.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.PCSX2
{
    public class PCSX2Client
    {
        public bool IsConnected { get; set; }
        public PCSX2Client()
        {

        }
        public bool Connect()
        {
            Console.WriteLine("Connecting to PCSX2");
            var pid = Memory.PCSX2_PROCESSID;
            if (pid == 0)
            {
                Console.WriteLine("PCSX2 not found.");
                Console.WriteLine("Press any key to exit.");
                Console.Read();
                System.Environment.Exit(0);
                return false;
            }
            return true;
        }
    }
}
