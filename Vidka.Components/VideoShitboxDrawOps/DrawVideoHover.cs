using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Components.Properties;
using Vidka.Core;
using Vidka.Core.Model;
using Miktemk;
using Vidka.Core.UiObj;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawVideoHover : DrawOp
    {
        public DrawVideoHover(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var proj = context.Proj;
            var vclip = uiObjects.CurrentVideoClipHover;
            if (vclip == null)
                return;

            int y1 = dimdim.getY_main1(h);
            //int y2 = vclip.HasAudio
            //    ? dimdim.getY_main2(h)
            //    : dimdim.getY_main_half(h);
            int y2 = dimdim.getY_main2(h); // I finally decided against 1/2 blue hover rect on no-audio clips, it confuses the easing collision -Oct 25, 2015
            int yAudio = dimdim.getY_main_half(h);
            int yEase1 = dimdim.getY_main_easing1(h);
            int yEase2 = dimdim.getY_main_easing2(h);
            //int yaudio = dimdim.getY_main_half(h);
            int x1 = dimdim.getScreenX1_video(vclip);
            int clipW = dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); // hacky, I know
            var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
            var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);
            //DrawOutlineOfAnyClip(x1, y1, clipW, y2, yAudio, yEase);

            CommonDrawOps.Ops.DrawVideoClipBorder(g, PPP.penHover, x1, x1 + clipW, y1, y2, yEase2, xEaseLeft, xEaseRight, vclip.EasingLeft, vclip.EasingRight);

            // .... draw outline
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            //g.DrawRectangle(PPP.penHover, x1, y1, clipW, y2 - y1);
            g.DrawLine(PPP.penHover, x1, y1, x1 + clipW, y1);
            g.DrawLine(PPP.penHover, x1, y1, x1, y2);
            g.DrawLine(PPP.penHover, x1 + clipW, y1, x1 + clipW, y2);
            if (uiObjects.TrimHover == TrimDirection.Left)
            {
                if (uiObjects.ShowEasingHandles && vclip.EasingLeft > 0)
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1 - xEaseLeft, yEase1, yEase2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else if (uiObjects.ShowEasingHandles)
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1, yAudio, yEase2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
            }
            if (uiObjects.TrimHover == TrimDirection.Right)
            {
                if (uiObjects.ShowEasingHandles && vclip.EasingRight > 0)
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1 + clipW + xEaseRight, yEase1, yEase2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else if (uiObjects.ShowEasingHandles)
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1 + clipW, yAudio, yEase2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else
                    CommonDrawOps.Ops.DrawTrimBracket(g, x1 + clipW, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
            }

            // .... draw seal when prev/next clip match
            if (uiObjects.TrimHover != TrimDirection.None)
            {
                var index = proj.ClipsVideo.IndexOf(vclip);
                if (uiObjects.TrimHover == TrimDirection.Left && index > 0)
                {
                    var prevClip = proj.ClipsVideo[index - 1];
                    if (prevClip.FileName == vclip.FileName && prevClip.FrameEnd == vclip.FrameStart + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(PPP.penActiveSealed, x1 + mouseTrimPixels, y1, x1 + mouseTrimPixels, y2);
                }
                if (uiObjects.TrimHover == TrimDirection.Right && index < proj.ClipsVideo.Count - 1)
                {
                    var nextClip = proj.ClipsVideo[index + 1];
                    if (nextClip.FileName == vclip.FileName && nextClip.FrameStart == vclip.FrameEnd + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(PPP.penActiveSealed, x1 + clipW + mouseTrimPixels, y1, x1 + clipW + mouseTrimPixels, y2);
                }
            }
        }

    }
}
