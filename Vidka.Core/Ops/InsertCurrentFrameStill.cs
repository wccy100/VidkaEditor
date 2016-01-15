using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.ExternalOps;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class InsertCurrentFrameStill : _VidkaOp
    {
        public InsertCurrentFrameStill(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "InsertCurrentFrameStill";

        public override void Run()
        {
            long frameOffset = 0;
            var clipIndex = Context.Proj.GetVideoClipIndexAtFrame(Context.UiObjects.CurrentMarkerFrame, out frameOffset);
            if (clipIndex == -1)
                return;

            var clip = Context.Proj.ClipsVideo[clipIndex];
            var secOffset = Context.Proj.FrameToSec(frameOffset);
            var imgFilename = VidkaIO.MakeUniqueFilename_Frame();
            var imgFilenameFull = VidkaIO.GetAuxillaryProjFile(Context.CurFileName, imgFilename);
            var newClip = new VidkaClipImage()
            {
                FileName = imgFilenameFull,
                FileLengthSec = Settings.Default.ImageClipLengthSeconds,
                FileLengthFrames = Context.Proj.SecToFrame(Settings.Default.ImageClipLengthSeconds),
                FrameStart = 0,
                FrameEnd = Context.Proj.SecToFrame(Settings.Default.ImageClipLengthSeconds),
            };

            // run all the shit
            Context.iiii("Extracting thumbnail from " + Path.GetFileName(clip.FileName) + " at sec=" + secOffset);
            VidkaIO.MakeSureFolderExistsForFile(imgFilenameFull);
            var op = new ThumbnailExtractionSingle(clip.FileName, imgFilenameFull, Context.Proj.Width, Context.Proj.Height, secOffset);
            Context.iiii("Done.");
            op.run();
            Context.MetaGenerator.RequestThumbsOnly(imgFilenameFull, true);

            Context.AddUndoableAction_insertClipAtMarkerPosition(newClip);
        }

    }
}
