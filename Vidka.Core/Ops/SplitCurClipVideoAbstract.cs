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
    public abstract class SplitCurClipVideoAbstract : _VidkaOp
    {
        public SplitCurClipVideoAbstract(IVidkaOpContext context) : base(context) { }

        /// <summary>
        /// Returns clip being split, its index within video timeline
        /// and how many frames from its FrameStart to cut
        /// </summary>
        protected bool DoVideoSplitCalculations(
            out VidkaClipVideoAbstract clip,
            out int clipIndex,
            out long frameOffsetStartOfVideo)
        {
            clip = null;
            clipIndex = Context.Proj.GetVideoClipIndexAtFrame(Context.UiObjects.CurrentMarkerFrame, out frameOffsetStartOfVideo);
            if (clipIndex == -1)
            {
                Context.cxzxc("No clip here... Cannot split!");
                return false;
            }
            clip = Context.Proj.GetVideoClipAtIndex(clipIndex);
            if (frameOffsetStartOfVideo == clip.FrameStartNoEase)
            {
                Context.cxzxc("On the seam... Cannot split!");
                return false;
            }
            if (clip.IsLocked)
            {
                Context.cxzxc("Clip locked... Cannot split!\nPress 'F' to unlock.");
                return false;
            }
            return true;
        }

    }
}
