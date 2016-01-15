using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.ExternalOps;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public abstract class PreviewAvsSegmentInMplayerAbstract : _VidkaOp
    {
        public PreviewAvsSegmentInMplayerAbstract(IVidkaOpContext context) : base(context) { }

        protected void PreviewAvsSegmentInMplayer(double secMplayerPreview, bool onlyLockedClips, ExternalPlayerType playerType)
        {
            cxzxc("creating mplayer...");
            var mplayed = new MPlayerPlaybackSegment(Context.Proj);
            mplayed.ExternalPlayer = playerType;
            mplayed.WhileYoureAtIt_cropProj(Context.UiObjects.CurrentMarkerFrame, (long)(Context.Proj.FrameRate * secMplayerPreview), onlyLockedClips);
            mplayed.run();
            if (mplayed.ResultCode == OpResultCode.FileNotFound)
                Context.eeee("Error: please make sure mplayer is in your PATH!");
            else if (mplayed.ResultCode == OpResultCode.OtherError)
                Context.eeee("Error: " + mplayed.ErrorMessage);
        }
    }
}
