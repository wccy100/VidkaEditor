using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class InsertSimpleTextClip : _VidkaOp
    {
        public InsertSimpleTextClip(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(InsertSimpleTextClip);
        public override string CommandName => Name;

        public override void Run()
        {
            var imgFilename = VidkaIO.MakeUniqueFilename_AuxSimpleText();
            var imgFilenameFull = VidkaIO.GetAuxillaryProjFile(Context.CurFileName, imgFilename);
            var newClip = new VidkaClipTextSimple()
            {
                Text = "Hello :)",
                ArgbBackgroundColor = Color.Black.ToArgb(),
                ArgbFontColor = Color.White.ToArgb(),
                FontSize = 20,
                FileName = imgFilenameFull,
                FileLengthSec = Settings.Default.ImageClipLengthSeconds,
                FileLengthFrames = Context.Proj.SecToFrame(Settings.Default.ImageClipLengthSeconds),
                FrameStart = 0,
                FrameEnd = Context.Proj.SecToFrame(Settings.Default.TextClipInitialLengthSeconds),
            };
            VidkaIO.RebuildAuxillaryFile_SimpleText(newClip, Context.Proj, Context.MetaGenerator);

            Context.AddUndoableAction_insertClipAtMarkerPosition(newClip);
        }

    }
}
