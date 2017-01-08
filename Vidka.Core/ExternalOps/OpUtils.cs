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
    }
}
