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
    public class DrawAudioHover : DrawOp
    {
        public DrawAudioHover(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var proj = context.Proj;
            var aclip = uiObjects.CurrentAudioClipHover;
            if (aclip == null)
                return;

            int y1 = dimdim.getY_audio1(h);
            int y2 = dimdim.getY_audio2(h);
            //var secStart = dimdim.FrameToSec(aclip.FrameStart);
            //var secEnd = dimdim.FrameToSec(aclip.FrameEnd);
            int x1 = dimdim.convert_Frame2ScreenX(aclip.FrameOffset);
            int x2 = dimdim.convert_Frame2ScreenX(aclip.FrameOffset + aclip.LengthFrameCalc);

            //DrawOutlineOfAnyClip(x1, y1, x2 - x1, y2, y2, y2);
            // .... draw outline
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            g.DrawRectangle(PPP.penHover, x1, y1, x2 - x1, y2 - y1);
            if (uiObjects.TrimHover == TrimDirection.Left)
                CommonDrawOps.Ops.DrawTrimBracket(g, x1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
            if (uiObjects.TrimHover == TrimDirection.Right)
                CommonDrawOps.Ops.DrawTrimBracket(g, x2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
        }

    }
}
