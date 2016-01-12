using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidka.Components
{
    public class PPP
    {
        public static readonly Pen penDefault = new Pen(Color.Black, 1); // new Pen(Color.FromArgb(255, 30, 30, 30), 1);
        public static readonly Pen penBorder = new Pen(Color.Black, 1);
        public static readonly Pen penMarker = new Pen(Color.Black, 2);
        public static readonly Pen penBorderDrag = new Pen(Color.Blue, 5);
        public static readonly Pen penHover = new Pen(Color.Blue, 4);
        public static readonly Pen penHoverThin = new Pen(Color.Blue, 2);
        public static readonly Pen penActiveClip = new Pen(Color.LightBlue, 6);
        public static readonly Pen penActiveBoundary = new Pen(Color.Red, 6);
        public static readonly Pen penActiveBoundaryPrev = new Pen(Color.Purple, 6);
        public static readonly Pen penActiveBoundaryEasing = new Pen(Color.LawnGreen, 12);
        public static readonly Pen penActiveSealed = new Pen(Color.LawnGreen, 6);
        public static readonly Pen penSealed = new Pen(Color.LawnGreen, 4); // marks the split line when prev.frameEnd == cur.frameStart
        public static readonly Pen penGray = new Pen(Color.Gray, 1);
        public static readonly Pen penWhite = new Pen(Color.White, 1);
        public static readonly Brush brushDefault = new SolidBrush(Color.Black);
        public static readonly Brush brushLightGray = new SolidBrush(Color.FromArgb(unchecked((int)0xFFfbfbfb)));
        public static readonly Brush brushLightGray2 = new SolidBrush(Color.FromArgb(unchecked((int)0xFFf5f5f5)));
        public static readonly Brush brushLightGray3 = new SolidBrush(Color.FromArgb(unchecked((int)0xFFeeeeee)));
        public static readonly Brush brushGreenPixelTypeStandard = new SolidBrush(Color.Green);
        public static readonly Brush brushActive = new SolidBrush(Color.LightBlue);
        public static readonly Brush brushActiveEased = new SolidBrush(Color.FromArgb(90, 0xAD, 0xD8, 0xE6));
        public static readonly Brush brushLockedClip = new SolidBrush(Color.Beige);
        public static readonly Brush brushLockedActiveClip = new SolidBrush(Color.DarkKhaki);
        public static readonly Brush brushWhite = new SolidBrush(Color.White);
        public static readonly Brush brushHazy = new SolidBrush(Color.FromArgb(200, 230, 230, 230));
        public static readonly Brush brushHazyActive = new SolidBrush(Color.FromArgb(90, 0xAD, 0xD8, 0xE6));
        public static readonly Brush brushHazyCurtain = new SolidBrush(Color.FromArgb(200, 245, 245, 245)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
        public static readonly Brush brushHazyMute = new SolidBrush(Color.FromArgb(200, 200, 200, 200)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
        public static readonly Brush brushHazyCustomAudio = new SolidBrush(Color.FromArgb(70, 255, 200, 240)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
        public static readonly Font fontDefault = SystemFonts.DefaultFont;
    }
}
