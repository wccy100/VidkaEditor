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
    public class DrawOriginalVideo : DrawOp
    {
        public DrawOriginalVideo(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var currentClip = uiObjects.CurrentVideoClip;
            if (currentClip == null)
                return;

            int y1 = dimdim.getY_original1(h);
            int y2 = dimdim.getY_original2(h);
            int yaudio = dimdim.getY_original_half(h);
            if (!currentClip.HasAudio)
                y2 = yaudio;

            // calculations for current (selected) clip to fill in the rect
            int xOrigEaseL = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart, currentClip.FileLengthFrames, w);
            int xOrigEaseR = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd, currentClip.FileLengthFrames, w);
            int xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart + currentClip.EasingLeft, currentClip.FileLengthFrames, w);
            int xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd - currentClip.EasingRight, currentClip.FileLengthFrames, w);

            // draw entire original clip (0 .. vclip.FileLength)
            g.FillRectangle(PPP.brushWhite, 0, y1, w, y2 - y1);
            g.FillRectangle(PPP.brushActiveEased, xOrigEaseL, y1, xOrigEaseR - xOrigEaseL, y2 - y1);
            g.FillRectangle(PPP.brushActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);
            CommonDrawOps.Ops.DrawClipBitmaps(
                g: g,
                context: context,
                imgCache: imgCache,
                vclip: currentClip,
                x1: 0,
                y1: y1,
                clipw: w,
                clipvh: yaudio - y1,
                secStart: 0,
                len: currentClip.FileLengthSec ?? 0);
            var waveFile = currentClip.HasCustomAudio ? currentClip.CustomAudioFilename : currentClip.FileName;
            var waveOffset = currentClip.HasCustomAudio ? currentClip.CustomAudioOffset : 0;
            var waveLength = currentClip.HasCustomAudio ? currentClip.CustomAudioLengthSec : currentClip.FileLengthSec;
            CommonDrawOps.Ops.DrawWaveform(
                g, context, imgCache,
                waveFile,
                waveLength ?? 0, currentClip.FileLengthSec ?? 0, waveOffset,
                0, yaudio, w, y2 - yaudio,
                0, currentClip.FileLengthSec ?? 0,
                currentClip.IsMuted, currentClip.HasCustomAudio);
            g.DrawLine(PPP.penGray, 0, yaudio, w, yaudio);
            g.DrawRectangle(PPP.penDefault, 0, y1, w, y2 - y1);

            // draw clip bounds and diagonals (where they are)
            foreach (var vclip in uiObjects.CurClipAllUsagesVideo)
            {
                xOrigEaseL = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart, vclip.FileLengthFrames, w);
                xOrigEaseR = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd, vclip.FileLengthFrames, w);
                xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart + vclip.EasingLeft, vclip.FileLengthFrames, w);
                xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd - vclip.EasingRight, vclip.FileLengthFrames, w);
                var xMain1 = dimdim.getScreenX1_video(vclip);
                var xMain2 = xMain1 + dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); //hacky, I know
                int yMainTop = dimdim.getY_main1(h);
                int xMainDelta = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta); //hacky, I know
                int xOrigDelta = dimdim.convert_Frame2ScreenX_OriginalTimeline(uiObjects.MouseDragFrameDelta, currentClip.FileLengthFrames, w); // hacky, I know
                var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
                var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);

                var type = (vclip == uiObjects.CurrentVideoClipHover)
                    ? OutlineClipType.Hover
                    : OutlineClipType.Active;
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain1, yMainTop, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain2, yMainTop, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig1, y1, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig2, y1, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penGray : PPP.penGray, xOrigEaseL, y1, xOrigEaseL, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penGray : PPP.penGray, xOrigEaseR, y1, xOrigEaseR, y2);
                if (type == OutlineClipType.Hover && !uiObjects.MouseDragFrameDeltaMTO)
                {
                    if (uiObjects.TrimHover == TrimDirection.Left)
                    {
                        if (uiObjects.ShowEasingHandles)
                            g.DrawLine(PPP.penActiveBoundary, xOrigEaseL, y1, xOrigEaseL, y2);
                        else
                        {
                            g.DrawLine(PPP.penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
                            CommonDrawOps.Ops.DrawTrimBracket(g, xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
                        }
                    }
                    if (uiObjects.TrimHover == TrimDirection.Right)
                    {
                        if (uiObjects.ShowEasingHandles)
                            g.DrawLine(PPP.penActiveBoundary, xOrigEaseR, y1, xOrigEaseR, y2);
                        else
                        {
                            g.DrawLine(PPP.penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
                            CommonDrawOps.Ops.DrawTrimBracket(g, xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
                        }
                    }
                }
            }

            // draw marker on 
            var frameOffset = uiObjects.OriginalTimelinePlaybackMode
                ? uiObjects.CurrentMarkerFrame
                : uiObjects.CurrentMarkerFrame - (uiObjects.CurrentClipFrameAbsPos ?? 0) + currentClip.FrameStartNoEase;
            int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, w);
            g.DrawLine(PPP.penMarker, xMarker, y1, xMarker, y2);
        }

    }
}
