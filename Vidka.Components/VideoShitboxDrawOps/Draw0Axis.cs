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
    public class Draw0Axis : DrawOp
    {
        public Draw0Axis(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            // vars to use
            var proj = context.Proj;

            // compute how many segments/ticks/etc to draw
            var frameStart = dimdim.convert_ScreenX2Frame(0);
            var frameEnd = dimdim.convert_ScreenX2Frame(w);
            int nSegments = w / Settings.Default.MinnimumTimelineSegmentSizeInPixels;
            long framesPerSegment = (frameEnd - frameStart) / nSegments;
            int secondsPerSegment = (int)(framesPerSegment / proj.FrameRate);
            secondsPerSegment = Utils.GetClosestSnapToSecondsForTimeAxis(secondsPerSegment);
            if (secondsPerSegment == 0) // we are zoomed in so much, but still show seconds
                secondsPerSegment = 1;
            // now that everything is rounded, how many segments do we really have?
            var actualFramesPerSegment = secondsPerSegment * proj.FrameRate;
            var actualNSegments = (int)((frameEnd - frameStart) / actualFramesPerSegment);
            var startingSecond = (long)Math.Floor(proj.FrameToSec(frameStart));

            // compute dimensions...
            var scrollbarHeight = context.GetHorizontalScrollBarHeight();
            var y1 = h - dimdim.getY_timeAxisHeight(h) - scrollbarHeight;
            var y2 = h - scrollbarHeight;

            g.DrawLine(PPP.penGray, 0, y1, w, y1);
            for (var i = 0; i < actualNSegments + 1; i++)
            {
                var curSecond = startingSecond + i * secondsPerSegment;
                var scrX = dimdim.convert_Sec2ScreenX(curSecond);
                var ts = TimeSpan.FromSeconds(curSecond);
                g.DrawLine(PPP.penGray, scrX, y1, scrX, y2);
                g.DrawString(ts.ToTsString_MinuteOrHour(), PPP.fontDefault, PPP.brushDefault, scrX + 2, y1 + 4);
            }
            if (secondsPerSegment == 1 && framesPerSegment <= proj.FrameRate)
            {
                // draw frame ticks as well
                for (var i = frameStart; i < frameEnd; i++)
                {
                    var scrX = dimdim.convert_Frame2ScreenX(i);
                    g.DrawLine(PPP.penGray, scrX, y1, scrX, y1 + 3);
                }
            }
        }

    }
}
