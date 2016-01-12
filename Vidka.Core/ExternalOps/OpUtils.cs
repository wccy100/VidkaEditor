using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidka.Core.ExternalOps
{
    public class OpUtils
    {
        public const string VirtualDubExecutable = "VirtualDub";
        public const string ExplorerExecutable = "explorer";

        public static void OpenVirtualDubAndRunScript(string scriptFilename)
        {
            Process process = new Process();
            process.StartInfo.FileName = VirtualDubExecutable;
            process.StartInfo.Arguments = String.Format("/s \"{0}\"", scriptFilename);
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
        }

        public static void OpenWinExplorerAndSelectThisFile(string filename)
        {
            string args = string.Format("/e, /select, \"{0}\"", filename);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = ExplorerExecutable;
            info.Arguments = args;
            Process.Start(info);
        }

        public static void OpenWinExplorerAndSelectNothing(string filename)
        {
            string args = string.Format("\"{0}\"", filename);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = ExplorerExecutable;
            info.Arguments = args;
            Process.Start(info);
        }
    }
}
