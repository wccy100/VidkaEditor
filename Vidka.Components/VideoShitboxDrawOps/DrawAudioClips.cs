using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawAudioClips : DrawOp
    {
        public DrawAudioClips(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var draggy = uiObjects.Draggy;
            var proj = context.Proj;

            int y1 = dimdim.getY_audio1(h);
            int y2 = dimdim.getY_audio2(h);
            int cliph = y2 - y1;

            foreach (var aclip in proj.ClipsAudio)
            {
                var frame1 = aclip.FrameOffset;
                var frame2 = aclip.FrameOffset + aclip.LengthFrameCalc;
                if (dimdim.isEvenOnTheScreen(frame1, frame2, w))
                {
                    int x1 = dimdim.convert_Frame2ScreenX(frame1);
                    int x2 = dimdim.convert_Frame2ScreenX(frame2);
                    int clipw = x2 - x1;

                    CommonDrawOps.Ops.DrawWaveform(g, context, imgCache, aclip.FileName, aclip.FileLengthSec ?? 0, aclip.FileLengthSec ?? 0, 0, x1, y1, clipw, cliph,
                        proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd),
                        false, false);

                    //throw new NotImplementedException("DrawWaveform that takes Audio clip!!!");
                    //DrawWaveform(g, proj, aclip, x1, y1, clipw, cliph,
                    //	proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd));

                    // active video clip deserves a special outline
                    if (aclip == uiObjects.CurrentAudioClip)
                        g.FillRectangle(PPP.brushHazyActive, x1, y1, clipw, cliph);

                    // outline rect
                    g.DrawRectangle(PPP.penDefault, x1, y1, clipw, cliph);
                }
            }
            if (draggy.Mode == EditorDraggyMode.AudioTimeline)
            {
                var draggyX = draggy.MouseX - draggy.MouseXOffset;
                var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
                g.DrawRectangle(PPP.penBorderDrag, draggyX, y1, draggyW, cliph);
            }
        }

    }
}
