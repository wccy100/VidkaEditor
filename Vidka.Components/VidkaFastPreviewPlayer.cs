using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Vidka.Core;
using Vidka.Core.ExternalOps;

namespace Vidka.Components
{
	public partial class VidkaFastPreviewPlayer : UserControl
	{
		private Font fontDefault = SystemFonts.DefaultFont;
		private Brush brushDefault = new SolidBrush(Color.Black);
		private Brush brushWhite = new SolidBrush(Color.White);

		private VidkaFileMapping fileMapping;
		private double offsetSeconds;
		private string filenameVideo;
		private Bitmap bmpThumbs;
		private Rectangle rectCrop, rectMe;
		private int bmpThumbs_nRow;
		private int bmpThumbs_nCol;

		public VidkaFastPreviewPlayer()
		{
			InitializeComponent();
			rectCrop = new Rectangle();
			rectMe = new Rectangle() { X = 0, Y = 0 };
		}

		private void VidkaFastPreviewPlayer_Load(object sender, EventArgs e)
		{
			this.DoubleBuffered = true;
		}

		public void SetFileMapping(VidkaFileMapping fileMapping)
		{
			this.fileMapping = fileMapping;
			Invalidate();
		}
		public void SetStillFrameNone()
		{
			disposeOfOldBmpThumbs();
			filenameVideo = null;
			Invalidate();
		}
		public void SetStillFrame(string filename, double offsetSeconds)
		{
			if (this.filenameVideo != filename && fileMapping != null)
			{
				disposeOfOldBmpThumbs();
				this.filenameVideo = filename;
				var filenameThumbs = fileMapping.AddGetThumbnailFilename(filename);
				if (File.Exists(filenameThumbs)) {
					bmpThumbs = System.Drawing.Image.FromFile(filenameThumbs, true) as Bitmap;
					bmpThumbs_nRow = bmpThumbs.Width / ThumbnailExtraction.ThumbW;
					bmpThumbs_nCol = bmpThumbs.Height / ThumbnailExtraction.ThumbH;
				}
			}
			this.offsetSeconds = offsetSeconds;
			Invalidate();
		}

		private void disposeOfOldBmpThumbs()
		{
			if (bmpThumbs == null)
				return;
			bmpThumbs.Dispose();
			bmpThumbs = null;
		}

		private void VidkaFastPreviewPlayer_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;
			if (bmpThumbs != null) {
				var imageIndex = (int)(offsetSeconds / ThumbnailExtraction.ThumbIntervalSec);
				if (bmpThumbs.Width == ThumbnailExtraction.ThumbW && bmpThumbs.Height == ThumbnailExtraction.ThumbH)
					imageIndex = 0; // index will overflow on clips from images or text
				rectMe.Width = Width;
				rectMe.Height = Height;
				rectCrop.X = ThumbnailExtraction.ThumbW * (imageIndex % bmpThumbs_nCol);
				rectCrop.Y = ThumbnailExtraction.ThumbH * (imageIndex / bmpThumbs_nRow);
				rectCrop.Width = ThumbnailExtraction.ThumbW;
				rectCrop.Height = ThumbnailExtraction.ThumbH;
				g.DrawImage(bmpThumbs, rectMe, rectCrop, GraphicsUnit.Pixel);
				var strFilename = Path.GetFileName(filenameVideo);
				var sizeF = g.MeasureString(strFilename, fontDefault);
				var strX = Width - sizeF.Width;
				var strY = Height - sizeF.Height;
				g.FillRectangle(brushWhite, strX, strY, sizeF.Width, sizeF.Height);
				g.DrawString(strFilename, fontDefault, brushDefault, strX, strY);
			}
		}

		public void PleaseUnlockThisFile(string filename)
		{
			disposeOfOldBmpThumbs();
		}
	}
}
