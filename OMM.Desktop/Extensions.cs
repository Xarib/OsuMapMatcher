using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    public static class Extensions
    {
        private static int oldId;

        public static bool MapIdHasChanged(this OsuMemoryReader omr, out int currentId)
        {
            if ((currentId = omr.GetMapId()) == oldId)
                return false;

            oldId = currentId;

            return true;
        }

        public static bool TryReadLineStartingWith(this StreamReader sr, string s, out string line)
        {
            while ((line = sr.ReadLine()) is not null && !line.StartsWith(s)) { }

            if (line is null)
                return false;

            return true;
        }
    }
}
