using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class SplitCurClipVideo_DeleteRight : SplitCurClipVideoAbstract
    {
        public SplitCurClipVideo_DeleteRight(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "SplitCurClipVideo_DeleteRight";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.D);
        }

        public override void Run()
        {
            VidkaClipVideoAbstract clip;
            int clipIndex = 0;
            long frameOffsetStartOfVideo = 0;
            if (!DoVideoSplitCalculations(out clip, out clipIndex, out frameOffsetStartOfVideo))
                return;
            var clip_oldEnd = clip.FrameEnd;
            var clip_oldEaseRight = clip.EasingRight;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Undo = () =>
                {
                    Context.cxzxc("UNDO splitR: end=" + clip_oldEnd);
                    clip.FrameEnd = clip_oldEnd;
                    clip.EasingRight = clip_oldEaseRight;
                    Context.UpdateCanvasWidthFromProjAndDimdim();
                },
                Redo = () =>
                {
                    Context.cxzxc("splitR: end=" + frameOffsetStartOfVideo);
                    clip.FrameEnd = frameOffsetStartOfVideo;
                    clip.EasingRight = 0;
                    Context.UpdateCanvasWidthFromProjAndDimdim();
                },
                PostAction = () =>
                {
                    Context.SetFrameMarker_RightOfVClipJustBefore(clip, Context.Proj);
                } // marker stays where it is...
            });
        }
    }
}
