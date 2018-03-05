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
    public class ExportToAvi_Segment : _VidkaOp
    {
        public ExportToAvi_Segment(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(ExportToAvi_Segment);
        public override string CommandName => Name;

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
            if (nBreakups == 1)
            {
                RenderToAvi(Context.CurFileName, Context.Proj, fileOutVideo, Settings.Default.mencoderArguments);
            }
            else
            {
                var subProjs = Context.Proj.RenderBreakupsSplitIntoSubProjects();
                var aviFilenames = new string[subProjs.Length];
                for (int i = 0; i < subProjs.Length; i++)
                {
                    aviFilenames[i] = GetSegmentVideoSegmentOutputPath(i);
                    if (i == 0)
                        UtilsOp.OpenWinExplorerAndSelectNothing(Path.GetDirectoryName(aviFilenames[i]));
                    RenderToAvi(Context.CurFileName, subProjs[i], aviFilenames[i], Settings.Default.mencoderArguments);
                }
            }
        }

        public void RenderOneSegment(int index)
        {
            var subProjs = Context.Proj.RenderBreakupsSplitIntoSubProjects();
            if (index < 0 || index >= subProjs.Length)
                return;
            var subProj = subProjs[index];
            var videoFilename = GetSegmentVideoSegmentOutputPath(index);
            RenderToAvi(Context.CurFileName, subProj, videoFilename, Settings.Default.mencoderArguments);
        }

        private string GetSegmentVideoSegmentOutputPath(int index)
        {
            return String.Format("{0}-{1}{2}", Context.CurFileName, index + 1, Settings.Default.ExportVideoExtension);
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
