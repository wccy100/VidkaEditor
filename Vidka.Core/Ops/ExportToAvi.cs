#define RUN_MENCODER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.ExternalOps;
using Vidka.Core.Properties;
using Vidka.Core.Model;
using System.IO;
using Miktemk;

namespace Vidka.Core.Ops
{
    public class ExportToAvi : _VidkaOp
    {
        public ExportToAvi(IVidkaOpContext context) : base(context) {}

        public const string Name = nameof(ExportToAvi);
        public override string CommandName => Name;
       
        private string rawDumpFolder;

        //... commented out because we have created menu shortcuts
        //public override bool TriggerByKeyPress(KeyEventArgs e)
        //{
        //    return (e.Control && e.Shift && e.KeyCode == Keys.E);
        //}

        public override void Run()
        {
            if (String.IsNullOrEmpty(Context.CurFileName))
            {
                Context.iiii("Context.CurFileName is null!");
                return;
            }
            var nBreakups = Context.Proj.RenderBreakupsCount();
            var fileOutVideo = Context.CurFileName + Settings.Default.ExportVideoExtension;
            if (nBreakups <= 1)
            {
                RenderToAvi(Context.CurFileName, Context.Proj, fileOutVideo, Settings.Default.mencoderArguments);
            }
            else
            {
                rawDumpFolder = Context.CheckRawDumpFolderIsOkAndGiveItToMe();
                var subProjs = Context.Proj.RenderBreakupsSplitIntoSubProjects();
                var aviFilenames = new string[subProjs.Length];
                for (int i = 0; i < subProjs.Length; i++)
                    aviFilenames[i] = GetRawVideoSegmentOutputPath(i);
                UtilsOp.OpenWinExplorerAndSelectNothing(Path.GetDirectoryName(aviFilenames[0]));
                var anyExist = aviFilenames.FirstOrDefault(x => File.Exists(x));
                if (anyExist != null)
                {
                    if (!Context.DialogConfirm("Files exist! Proceed?", "Some segment files here exist, e.g. " + Path.GetFileName(anyExist) + @".
They will not be overwritten, because I assume you want to save long rendering time.
(If you wish to rerender those files you must delete them manually)
So, anyways, proceed, do I?"))
                        return;
                }
                for (int i = 0; i < subProjs.Length; i++)
                {
                    if (!File.Exists(aviFilenames[i]))
                        RenderToAvi(Context.CurFileName, subProjs[i], aviFilenames[i], Settings.Default.mencoderArgumentsRaw);
                }
                var vdubScript = GetVdubScriptOutputPathForConcatRawSegments();
                VidkaIO.MakeVDubScriptForOpenTheseVideosAndStartRender(aviFilenames, fileOutVideo, vdubScript);
                OpUtils.OpenVirtualDubAndRunScript(vdubScript);
            }
        }

        private string GetRawVideoSegmentOutputPath(int index)
        {
            var filenameOut = String.Format("{0}-{1}{2}", Path.GetFileName(Context.CurFileName), index + 1, Settings.Default.ExportVideoExtension);
            return Path.Combine(rawDumpFolder, filenameOut);
        }

        private string GetVdubScriptOutputPathForConcatRawSegments()
        {
            var filenameOut = Path.GetFileName(Context.CurFileName) + ".vdscript";
            return Path.Combine(rawDumpFolder, filenameOut);
        }

        private void RenderToAvi(string xmlFilename, VidkaProj proj, string fileOutVideo, string mencoderArgs)
        {
            var fileOutAvs = VidkaIO.GetGeneratedAvsTmpFilename();
            VidkaIO.ExportToAvs(proj, fileOutAvs);
            Context.iiii("------ export to " + Settings.Default.ExportVideoExtension + "------");
            Context.iiii("Exported to " + fileOutAvs);
#if RUN_MENCODER
            Context.InvokeOpByName("RebuildProject");
            Context.iiii("Exporting to " + fileOutVideo);
            var mencoding = new MEncoderSaveVideoFile(fileOutAvs, fileOutVideo, mencoderArgs);
            Context.iiii("------ executing: ------");
            Context.iiii(mencoding.FullCommand);
            Context.iiii("------");
            mencoding.RunMEncoder();
            Context.iiii("Exported to " + fileOutVideo);
            Context.iiii("Done export.");
#endif
        }
    }
}
