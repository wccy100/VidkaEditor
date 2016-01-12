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
        public override string CommandName { get { return Name; } }
        public const string Name = "SplitCurClipVideo";

        protected VidkaClipVideoAbstract ClipNewOnTheLeft;
        protected VidkaClipVideoAbstract ClipOldOnTheRight;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.S);
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
                Context.cxzxc("Cannot, split text clips. Copy it instead!");
                return;
            }
            var clip_oldStart = clip.FrameStart;
            var clip_oldEaseLeft = clip.EasingLeft;
            ClipNewOnTheLeft = clip.MakeCopy_VideoClip();
            ClipNewOnTheLeft.FrameEnd = frameOffsetStartOfVideo; // remember, frameOffset is returned relative to start of the media file
            ClipNewOnTheLeft.EasingRight = 0;
            ClipOldOnTheRight = clip;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Undo = () =>
                {
                    Context.cxzxc("UNDO split");
                    Context.Proj.ClipsVideo.Remove(ClipNewOnTheLeft);
                    clip.FrameStart = clip_oldStart;
                    clip.EasingLeft = clip_oldEaseLeft;
                },
                Redo = () =>
                {
                    Context.cxzxc("split: location=" + frameOffsetStartOfVideo);
                    Context.Proj.ClipsVideo.Insert(clipIndex, ClipNewOnTheLeft);
                    clip.FrameStart = frameOffsetStartOfVideo;
                    clip.EasingLeft = 0;
                },
                PostAction = () =>
                {
                    Context.UiObjects.SetActiveVideo(clip, Context.Proj); // to reset CurrentClipFrameAbsPos
                }
            });
            if (Context.PreviewLauncher.IsPlaying)
                Context.PreviewLauncher.SplitPerformedIncrementClipIndex();
        }
    }
}
