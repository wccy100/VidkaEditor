using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core;
using Vidka.Core.Model;

namespace Vidka.Components
{
    // here we have functions and extension methods concerned with drawing

    public static partial class Utils
	{
        public static void IterateOverVisibleVideoClips(
            this ProjectDimensions dimdim,
            VidkaProj proj,
            int Width,
            Action<VidkaClipVideoAbstract, VidkaClipVideoAbstract, int, int, long, int> callback)
        {
            long curFrame = 0;
            int index = 0;
            VidkaClipVideoAbstract vclipPrev = null;
            foreach (var vclip in proj.ClipsVideo)
            {
                if (dimdim.isEvenOnTheScreen(curFrame, curFrame + vclip.LengthFrameCalc, Width))
                {
                    int x1 = dimdim.convert_Frame2ScreenX(curFrame);
                    int x2 = dimdim.convert_Frame2ScreenX(curFrame + vclip.LengthFrameCalc);
                    callback(vclip, vclipPrev, x1, x2, curFrame, index);
                }
                curFrame += vclip.LengthFrameCalc;
                index++;
                vclipPrev = vclip;
            }
        }

        public static void IterateOverVisibleAudioClips(
            this ProjectDimensions dimdim,
            VidkaProj proj,
            int Width,
            Action<VidkaClipAudio, int, int, long, long> callback)
        {
            foreach (var aclip in proj.ClipsAudio)
            {
                var frame1 = aclip.FrameOffset;
                var frame2 = aclip.FrameOffset + aclip.LengthFrameCalc;
                if (dimdim.isEvenOnTheScreen(frame1, frame2, Width))
                {
                    int x1 = dimdim.convert_Frame2ScreenX(frame1);
                    int x2 = dimdim.convert_Frame2ScreenX(frame2);
                    callback(aclip, x1, x2, frame1, frame2);
                }
            }
        }
	}
}
