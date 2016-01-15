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
    public class SplitCurClipVideo_DeleteLeft : SplitCurClipVideoAbstract
    {
        public SplitCurClipVideo_DeleteLeft(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "SplitCurClipVideo_DeleteLeft";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.A && !e.Shift && !e.Control);
        }

        public override void Run()
        {
            VidkaClipVideoAbstract clip;
            int clipIndex = 0;
            long frameOffsetStartOfVideo = 0;
            if (!DoVideoSplitCalculations(out clip, out clipIndex, out frameOffsetStartOfVideo))
                return;
            var clip_oldStart = clip.FrameStart;
            var clip_oldEaseLeft = clip.EasingLeft;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Undo = () =>
                {
                    cxzxc("UNDO splitL: start=" + clip_oldStart);
                    clip.FrameStart = clip_oldStart;
                    clip.EasingLeft = clip_oldEaseLeft;
                    Context.UpdateCanvasWidthFromProjAndDimdim();
                },
                Redo = () =>
                {
                    cxzxc("splitL: start=" + frameOffsetStartOfVideo);
                    clip.FrameStart = frameOffsetStartOfVideo;
                    clip.EasingLeft = 0;
                    Context.UpdateCanvasWidthFromProjAndDimdim();
                },
                PostAction = () =>
                {
                    Context.UiObjects.SetActiveVideo(clip, Context.Proj); // to reset CurrentClipFrameAbsPos
                    Context.SetFrameMarker_LeftOfVClip(clip, Context.Proj);
                }
            });
        }
    }
}
