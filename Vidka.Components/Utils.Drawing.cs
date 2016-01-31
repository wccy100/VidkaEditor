using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Components
{
    // here we have functions and extension methods concerned with drawing

    public static partial class Utils
	{
        public static void IterateOverVisibleVideoClips(
            this IVidkaOpContext context,
            int Width,
            Action<VidkaClipVideoAbstract, VidkaClipVideoAbstract, int, int, long, int> callback,
            Action<long, EditorDraggy> callbackDraggy)
        {
            var dimdim = context.Dimdim;
            var proj = context.Proj;
            var draggy = context.UiObjects.Draggy;

            long curFrame = 0;
            int index = 0;
            int draggyVideoShoveIndex = dimdim.GetVideoClipDraggyShoveIndex(draggy);

            VidkaClipVideoAbstract vclipPrev = null;
            foreach (var vclip in proj.ClipsVideo)
            {
                if (dimdim.isEvenOnTheScreen(curFrame, curFrame + vclip.LengthFrameCalc, Width))
                {
                    if (draggy.Mode == EditorDraggyMode.VideoTimeline && draggyVideoShoveIndex == index)
                    {
                        callbackDraggy(curFrame, draggy);
                        curFrame += draggy.FrameLength;
                    }
                    int x1 = dimdim.convert_Frame2ScreenX(curFrame);
                    int x2 = dimdim.convert_Frame2ScreenX(curFrame + vclip.LengthFrameCalc);
                    if (draggy.VideoClip != vclip)
                    {
                        callback(vclip, vclipPrev, x1, x2, curFrame, index);
                    }
                }
                //if (draggy != null && draggy.VideoClip != vclip)
                if (draggy.VideoClip != vclip)
                    curFrame += vclip.LengthFrameCalc;
                index++;
                vclipPrev = vclip;
            }
            if (draggy.Mode == EditorDraggyMode.VideoTimeline && draggyVideoShoveIndex == index)
                callbackDraggy(curFrame, draggy);
        }

        public static void IterateOverVisibleAudioClips(
            this IVidkaOpContext context,
            int Width,
            Action<VidkaClipAudio, int, int, long, long> callback)
        {
            var dimdim = context.Dimdim;
            var proj = context.Proj;

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

        public static void GetVClipScreenPosition(
            this IVidkaOpContext context,
            VidkaClipVideoAbstract vclip,
            int h,
            ref Rectangle rect)
        {
            var dimdim = context.Dimdim;
            var proj = context.Proj;
            var frameAbs = proj.GetVideoClipAbsFramePositionLeft(vclip);
            var y1 = dimdim.getY_main1(h);
            var y2 = dimdim.getY_main2(h);
            rect.X = dimdim.convert_Frame2ScreenX(frameAbs);
            rect.Y = y1;
            rect.Width = dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc);
            rect.Height = y2 - y1;
        }

	}
}
