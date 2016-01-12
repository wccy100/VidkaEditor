using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawVideoAudioAligns : DrawOp
    {
        public static readonly Pen penRenderBreakupLine = new Pen(Color.LightGray, 5);

        public DrawVideoAudioAligns(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {
            penRenderBreakupLine.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        }
        public override void Paint(Graphics g, int w, int h)
        {
            //int y1Video = dimdim.getY_main1(h);
            int y2Video = dimdim.getY_main2(h);
            //int yVideoAudio = dimdim.getY_main_half(h);
            int y1Audio = dimdim.getY_audio1(h);
            //int y2Audio = dimdim.getY_audio2(h);

            if (context.Proj.ClipsAudio.Count == 0)
                return;
            var draggy = context.UiObjects.Draggy;
            var isTrimming = context.UiObjects.CurrentAudioClipHover != null
                && context.UiObjects.TrimHover != TrimDirection.None
                && context.UiObjects.MouseDragFrameDelta != 0;
            dimdim.IterateOverVisibleVideoClips(context.Proj, w, (vclip, vclipPrev, x1, x2, curFrameVideo, index) =>
            {
                var curFrameVideoRight = curFrameVideo + vclip.LengthFrameCalc;
                dimdim.IterateOverVisibleAudioClips(context.Proj, w, (aclip, ax1, ax2, frameAudio1, frameAudio2) =>
                {
                    if (curFrameVideo == frameAudio1 || curFrameVideo == frameAudio2)
                        g.DrawLine(penRenderBreakupLine, x1, y2Video, x1, y1Audio);
                    if (curFrameVideoRight == frameAudio1 || curFrameVideoRight == frameAudio2)
                        g.DrawLine(penRenderBreakupLine, x2, y2Video, x2, y1Audio);
                    if (isTrimming && context.UiObjects.CurrentAudioClipHover == aclip)
                    {
                        var delta = context.UiObjects.MouseDragFrameDelta;
                        if (context.UiObjects.TrimHover == TrimDirection.Left)
                        {
                            if (curFrameVideo == frameAudio1 + delta)
                                g.DrawLine(penRenderBreakupLine, x1, y2Video, x1, y1Audio);
                            if (curFrameVideoRight == frameAudio1 + delta)
                                g.DrawLine(penRenderBreakupLine, x2, y2Video, x2, y1Audio);
                        }
                        if (context.UiObjects.TrimHover == TrimDirection.Right)
                        {
                            if (curFrameVideo == frameAudio2 + delta)
                                g.DrawLine(penRenderBreakupLine, x1, y2Video, x1, y1Audio);
                            if (curFrameVideoRight == frameAudio2 + delta)
                                g.DrawLine(penRenderBreakupLine, x2, y2Video, x2, y1Audio);
                        }
                    }
                });
                if (draggy.Mode == EditorDraggyMode.AudioTimeline)
                {
                    var frameAudio1 = draggy.FrameAbsLeft;
                    var frameAudio2 = draggy.FrameAbsLeft + draggy.FrameLength;
                    if (curFrameVideo == frameAudio1 || curFrameVideo == frameAudio2)
                        g.DrawLine(penRenderBreakupLine, x1, y2Video, x1, y1Audio);
                    if (curFrameVideoRight == frameAudio1 || curFrameVideoRight == frameAudio2)
                        g.DrawLine(penRenderBreakupLine, x2, y2Video, x2, y1Audio);
                }
            });
        }

    }
}
