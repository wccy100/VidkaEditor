using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class Draw0Canvas : DrawOp
    {
        public Draw0Canvas(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            int yMain1 = dimdim.getY_main1(h);
            int yMain2 = dimdim.getY_main2(h);
            int yMainHalf = dimdim.getY_main_half(h);
            int yAudio1 = dimdim.getY_audio1(h);
            int yAudio2 = dimdim.getY_audio2(h);
            var hover = uiObjects.TimelineHover;
            g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? PPP.brushLightGray2 : PPP.brushLightGray, 0, yMain1, w, yMainHalf - yMain1);
            g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? PPP.brushLightGray3 : PPP.brushLightGray2, 0, yMainHalf, w, yMain2 - yMainHalf);
            g.FillRectangle((hover == ProjectDimensionsTimelineType.Audios) ? PPP.brushLightGray3 : PPP.brushLightGray2, 0, yAudio1, w, yAudio2 - yAudio1);
        }

    }
}
