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

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawCurFrameMarker : DrawOp
    {
        public DrawCurFrameMarker(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            if (context.UiObjects.OriginalTimelinePlaybackMode)
            {
                int y1 = dimdim.getY_original2(h);
                int y2 = h;
                g.FillRectangle(PPP.brushHazyCurtain, 0, y1, w, y2 - y1);
            }
            else
            {
                var scrollbarHeight = context.GetHorizontalScrollBarHeight();
                var y2 = h - dimdim.getY_timeAxisHeight(h) - scrollbarHeight;
                var markerX = dimdim.convert_Frame2ScreenX(uiObjects.CurrentMarkerFrame);
                g.DrawLine(PPP.penMarker, markerX, 0, markerX, y2);
                //g.DrawString("" + uiObjects.CurrentMarkerFrame, PPP.fontDefault, PPP.brushDefault, markerX, 0);
            }
        }

    }
}
