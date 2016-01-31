using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.Model;
using Vidka.Core.UiObj;
using Miktemk;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawVideoClips : DrawOp
    {
        private const int NonTrimIndicatorSizeL = 5;
        private const int NonTrimIndicatorSizeR = 10;

        public DrawVideoClips(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var proj = context.Proj;
            int y1 = dimdim.getY_main1(h);
            int y2 = dimdim.getY_main2(h);
            int yaudio = dimdim.getY_main_half(h);
            int yEase = dimdim.getY_main_easing2(h);
            int cliph = y2 - y1; // clip height (video and audio)
            int clipvh = yaudio - y1; // clip (only video) height (just the video part, no audio!)
            int yEase1 = y1 + cliph;
            int yEase2 = yEase;

            context.IterateOverVisibleVideoClips(w, (vclip, vclipPrev, x1, x2, curFrame, index) =>
            {
                var brushClip = whatBrushForThisClip(vclip);
                
                int clipw = x2 - x1;
                var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
                var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);

                // .... active video clip deserves a special outline, fill white otherwise to hide gray background
                g.FillRectangle(brushClip, x1, y1, clipw, clipvh);
                CommonDrawOps.Ops.DrawClipBitmaps(
                    g: g,
                    context: context,
                    imgCache: imgCache,
                    vclip: vclip,
                    x1: x1,
                    y1: y1,
                    clipw: clipw,
                    clipvh: clipvh,
                    secStart: proj.FrameToSec(vclip.FrameStartNoEase),
                    len: proj.FrameToSec(vclip.LengthFrameCalc));
                if (vclip.HasAudio || vclip.HasCustomAudio)
                {
                    var waveFile = vclip.HasCustomAudio ? vclip.CustomAudioFilename : vclip.FileName;
                    var waveOffset = vclip.HasCustomAudio ? vclip.CustomAudioOffset : 0;
                    var waveLength = vclip.HasCustomAudio ? vclip.CustomAudioLengthSec : vclip.FileLengthSec;
                    CommonDrawOps.Ops.DrawWaveform(g, context, imgCache,
                        waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset, x1, y1 + clipvh, clipw, cliph - clipvh,
                        proj.FrameToSec(vclip.FrameStart + vclip.EasingLeft), proj.FrameToSec(vclip.FrameEnd - vclip.EasingRight),
                        vclip.IsMuted, vclip.HasCustomAudio);
                    // waveform separator
                    g.DrawLine(PPP.penGray, x1, y1 + clipvh, x2, y1 + clipvh);
                    //g.DrawRectangle(PPP.penGray, x1, y1 + clipvh, x2 - x1, cliph - clipvh);
                }

                // .... still analyzing...
                if (vclip.IsNotYetAnalyzed)
                    g.DrawString("Still analyzing...", PPP.fontDefault, PPP.brushDefault, x1 + 5, y1 + 5);
                // .... easings
                if (vclip.HasAudio || vclip.HasCustomAudio)
                {
                    var waveFile = vclip.HasCustomAudio ? vclip.CustomAudioFilename : vclip.FileName;
                    var waveOffset = vclip.HasCustomAudio ? vclip.CustomAudioOffset : 0;
                    var waveLength = vclip.HasCustomAudio ? vclip.CustomAudioLengthSec : vclip.FileLengthSec;
                    if (vclip.EasingLeft > 0)
                    {
                        CommonDrawOps.Ops.DrawWaveform(g, context, imgCache,
                            waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset,
                            x1 - xEaseLeft, yEase1, xEaseLeft, yEase2 - yEase1,
                            proj.FrameToSec(vclip.FrameStart), proj.FrameToSec(vclip.FrameStart + vclip.EasingLeft),
                            vclip.IsMuted, vclip.HasCustomAudio);
                    }
                    if (vclip.EasingRight > 0)
                    {
                        CommonDrawOps.Ops.DrawWaveform(g, context, imgCache,
                            waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset,
                            x2, yEase1, xEaseRight, yEase2 - yEase1,
                            proj.FrameToSec(vclip.FrameEnd - vclip.EasingRight), proj.FrameToSec(vclip.FrameEnd),
                            vclip.IsMuted, vclip.HasCustomAudio);
                    }
                }

                // .... has this clip not been trimmed?
                if (vclip.FrameStart == 0)
                    g.DrawLine(PPP.penDefault, x1, y1 - 1, x1 + Math.Min(clipw / 2, NonTrimIndicatorSizeL), y1 - 1);
                if (vclip.FrameEnd == vclip.FileLengthFrames)
                    g.DrawLine(PPP.penDefault, x2, y1 - 1, x2 - Math.Min(clipw / 2, NonTrimIndicatorSizeR), y1 - 1);

                // .... pixel type standard
                if (vclip.IsPixelTypeStandard)
                {
                    g.FillEllipse(PPP.brushGreenPixelTypeStandard, x1 + 5, y1 + 5, 10, 10);
                }

                // .... outline this clip
                CommonDrawOps.Ops.DrawVideoClipBorder(g, PPP.penDefault, x1, x1 + clipw, y1, y1 + (vclip.HasAudio ? cliph : clipvh), yEase2, xEaseLeft, xEaseRight, vclip.EasingLeft, vclip.EasingRight);

                // .... if vclipPrev.end == vclip.start and they are same file, mark green indicator
                if (vclipPrev != null && vclipPrev.FileName == vclip.FileName && vclipPrev.FrameEnd == vclip.FrameStart)
                    g.DrawLine(PPP.penSealed, x1, y1 + 10, x1, y1 + clipvh);

                // .... label
                if (!String.IsNullOrEmpty(vclip.Label))
                    drawClipLabel(g, vclip.Label, x1, y1, clipw, cliph);
            },
            
            //------------------------------------ draw draggy video ---------------------------------------
            
            (frame, draggy) => {
                var draggyX = dimdim.convert_Frame2ScreenX(frame);
                var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
                var draggyH = (draggy.HasAudio) ? cliph : clipvh;
                if (draggy.VideoClip != null)
                {
                    g.FillRectangle(PPP.brushWhite, draggyX, y1, draggyW, draggyH);
                    g.FillRectangle(PPP.brushActive, draggyX, y1, draggyW, clipvh);
                }
                g.DrawRectangle(PPP.penBorderDrag, draggyX, y1, draggyW, draggyH);
                g.DrawString(draggy.Text, PPP.fontDefault, PPP.brushDefault, draggyX + 5, y1 + 5);

                // debug rect
                //g.DrawRectangle(PPP.penDefault, draggy.MouseX-draggy.MouseXOffset, y1-2, draggyW, cliph+5);
            });
        }

        private void drawClipLabel(Graphics g, string label, int x1, int y1, int clipw, int cliph)
        {
            var size = g.MeasureString(label, PPP.fontDefault);
            var maxTry = 3;
            while (size.Width > clipw)
            {
                if (maxTry == 0)
                    return;
                label = label.BreakInThreeWithEllipsis();
                size = g.MeasureString(label, PPP.fontDefault);
                maxTry--;
            }
            g.DrawString(label, PPP.fontDefault, PPP.brushDefault, x1, y1 - size.Height);
        }

        private Brush whatBrushForThisClip(VidkaClipVideoAbstract vclip)
        {
            var currentVideoClip = uiObjects.CurrentVideoClip;
            if (vclip == currentVideoClip && vclip.IsLocked)
                return PPP.brushLockedActiveClip;
            else if (vclip == currentVideoClip)
                return PPP.brushActive;
            else if (vclip.IsLocked)
                return PPP.brushLockedClip;
            return PPP.brushWhite;
        }
    }
}
