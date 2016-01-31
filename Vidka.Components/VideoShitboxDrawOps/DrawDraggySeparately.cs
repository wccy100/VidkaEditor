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
    public class DrawDraggySeparately : DrawOp
    {
        public DrawDraggySeparately(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {}

        public override void Paint(Graphics g, int w, int h)
        {
            var draggy = uiObjects.Draggy;
            if (draggy.Mode != EditorDraggyMode.DraggingFolder)
                return;
            var coordX = draggy.MouseX;
            var coordY = h / 3 - 50;
            g.FillRectangle(PPP.brushHazy, coordX, coordY, 500, 100);
            g.DrawRectangle(PPP.penDefault, coordX, coordY, 500, 100);
            g.DrawString(draggy.Text, PPP.fontDefault, PPP.brushDefault, coordX, coordY + 30);
        }

    }
}
