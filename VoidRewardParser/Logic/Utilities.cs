using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidRewardParser.Logic
{
    internal class Utilities
    {
        public static bool IsWindows10OrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 10;
        }
    }
}
