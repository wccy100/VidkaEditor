using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.UiObj;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawOriginalAudio : DrawOp
    {
        public DrawOriginalAudio(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var currentClip = uiObjects.CurrentAudioClip;
            if (currentClip == null)
                return;
            int y1 = dimdim.getY_original1(h);
            int y2 = dimdim.getY_original2(h);

            // calculations for current (selected) clip to fill in the rect
            int xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart, currentClip.FileLengthFrames, w);
            int xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd, currentClip.FileLengthFrames, w);

            // draw entire original clip (0 .. vclip.FileLength)
            CommonDrawOps.Ops.DrawWaveform(g, context, imgCache,
                currentClip.FileName, currentClip.FileLengthSec ?? 0, currentClip.FileLengthSec ?? 0, 0,
                0, y1, w, y2 - y1,
                0, currentClip.FileLengthSec ?? 0,
                false, false);
            // outline
            g.DrawRectangle(PPP.penDefault, 0, y1, w, y2 - y1);
            // current active
            g.FillRectangle(PPP.brushHazyActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);

            // draw clip bounds and diagonals (where they are)
            foreach (var vclip in uiObjects.CurClipAllUsagesAudio)
            {
                xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart, vclip.FileLengthFrames, w);
                xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd, vclip.FileLengthFrames, w);
                var xMain1 = dimdim.convert_Frame2ScreenX(vclip.FrameOffset);
                var xMain2 = xMain1 + dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); //hacky, I know
                int yMainTop = dimdim.getY_audio1(h);
                int xMainDelta = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta); //hacky, I know
                int xOrigDelta = dimdim.convert_Frame2ScreenX_OriginalTimeline(uiObjects.MouseDragFrameDelta, currentClip.FileLengthFrames, w); // hacky, I know

                var type = (vclip == uiObjects.CurrentAudioClipHover)
                    ? OutlineClipType.Hover
                    : OutlineClipType.Active;
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain1, yMainTop, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain2, yMainTop, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig1, y1, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig2, y1, xOrig2, y2);
                if (type == OutlineClipType.Hover)
                {
                    if (uiObjects.TrimHover == TrimDirection.Left)
                    {
                        g.DrawLine(PPP.penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
                        CommonDrawOps.Ops.DrawTrimBracket(g, xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                    if (uiObjects.TrimHover == TrimDirection.Right)
                    {
                        g.DrawLine(PPP.penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
                        CommonDrawOps.Ops.DrawTrimBracket(g, xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                }
            }

            // draw marker on 
            var frameOffset = uiObjects.OriginalTimelinePlaybackMode
                ? uiObjects.CurrentMarkerFrame
                : uiObjects.CurrentMarkerFrame - (currentClip.FrameOffset) + currentClip.FrameStart;
            int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, w);
            g.DrawLine(PPP.penMarker, xMarker, y1, xMarker, y2);
        }

    }
}
