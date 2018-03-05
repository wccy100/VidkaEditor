using Miktemk.Editor;
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
    public class SplitCurClipVideo : SplitCurClipVideoAbstract
    {
        public SplitCurClipVideo(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(SplitCurClipVideo);
        public override string CommandName => Name;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.S && !e.Shift && !e.Control);
        }

        public override void Run()
        {
            VidkaClipVideoAbstract clip;
            int clipIndex = 0;
            long frameOffsetStartOfVideo = 0;
            if (!DoVideoSplitCalculations(out clip, out clipIndex, out frameOffsetStartOfVideo))
                return;
            if (clip is VidkaClipTextSimple)
            {
                cxzxc("Cannot, split text clips. Copy it instead!");
                return;
            }
            var clip_oldStart = clip.FrameStart;
            var clip_oldEaseLeft = clip.EasingLeft;
            var clipNewOnTheLeft = clip.MakeCopy_VideoClip();
            clipNewOnTheLeft.FrameEnd = frameOffsetStartOfVideo; // remember, frameOffset is returned relative to start of the media file
            clipNewOnTheLeft.EasingRight = 0;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Undo = () =>
                {
                    cxzxc("UNDO split");
                    Context.Proj.ClipsVideo.Remove(clipNewOnTheLeft);
                    clip.FrameStart = clip_oldStart;
                    clip.EasingLeft = clip_oldEaseLeft;
                    AdditionalActionsOnUndo(clip, clipNewOnTheLeft);
                },
                Redo = () =>
                {
                    cxzxc("split: location=" + frameOffsetStartOfVideo);
                    Context.Proj.ClipsVideo.Insert(clipIndex, clipNewOnTheLeft);
                    clip.FrameStart = frameOffsetStartOfVideo;
                    clip.EasingLeft = 0;
                    AdditionalActionsOnRedo(clip, clipNewOnTheLeft);
                },
                PostAction = () =>
                {
                    Context.UiObjects.SetActiveVideo(clip, Context.Proj); // to reset CurrentClipFrameAbsPos
                    AdditionalActionsOnPostAction(clip, clipNewOnTheLeft);
                }
            });
            if (Context.PreviewLauncher.IsPlaying)
                Context.PreviewLauncher.SplitPerformedIncrementClipIndex();
        }

        protected virtual void AdditionalActionsOnUndo(VidkaClipVideoAbstract clip, VidkaClipVideoAbstract clipNewOnTheLeft) { }
        protected virtual void AdditionalActionsOnRedo(VidkaClipVideoAbstract clip, VidkaClipVideoAbstract clipNewOnTheLeft) { }
        protected virtual void AdditionalActionsOnPostAction(VidkaClipVideoAbstract clip, VidkaClipVideoAbstract clipNewOnTheLeft) { }
    }
}
