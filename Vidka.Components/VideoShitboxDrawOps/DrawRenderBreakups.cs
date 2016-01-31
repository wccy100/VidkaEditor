using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawRenderBreakups : DrawOp
    {
        public static readonly Pen penRenderBreakupLine = new Pen(Color.Gray, 5);

        public DrawRenderBreakups(IVidkaOpContext context, ImageCacheManager imageMan) : base(context, imageMan)
        {
            penRenderBreakupLine.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
        }
        public override void Paint(Graphics g, int w, int h)
        {
            context.IterateOverVisibleVideoClips(w, (vclip, vclipPrev, x1, x2, curFrame, index) =>
            {
                if (vclip.IsRenderBreakupPoint)
                {
                    g.DrawLine(penRenderBreakupLine, x1, 0, x1, h);
                }
            }, (frame, draggy) => { });
        }

    }
}
