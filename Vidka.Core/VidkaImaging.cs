using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Vidka.Core.Model;

namespace Vidka.Core
{
	public class VidkaImaging
	{
		private Font fontDefault = SystemFonts.DefaultFont;

		public static Bitmap RenderSimpleTextVideoClip(VidkaClipTextSimple clip, VidkaProj proj)
		{
			Bitmap image = new Bitmap(proj.Width, proj.Height);
			Graphics ggg = Graphics.FromImage(image);
			ggg.FillRectangle(new SolidBrush(Color.FromArgb(clip.ArgbBackgroundColor)), 0, 0, proj.Width, proj.Height);
			var font = new Font(FontFamily.GenericSansSerif, clip.FontSize);
			var ssss = ggg.MeasureString(clip.Text, font);
			ggg.DrawString(clip.Text, font, new SolidBrush(Color.FromArgb(clip.ArgbFontColor)), (proj.Width - ssss.Width) / 2, (proj.Height - ssss.Height) / 2);
			ggg.Flush();
			return image;
		}

		public static void RenderSimpleTextVideoClipToFile(VidkaClipTextSimple clip, VidkaProj proj, string outFilename)
		{
			Bitmap image = RenderSimpleTextVideoClip(clip, proj);
			// save all to one jpg file
			image.Save(outFilename, ImageFormat.Jpeg);
			image.Dispose();
		}
	}
}
