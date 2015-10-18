//#define ONE_LINE_TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace Vidka.Core.Ops
{
	public class ThumbnailExtractionSingle : OpBaseClass
	{
		private string filename;
		private string outFilename;
        private int w, h;
        private double secWhen;

        public ThumbnailExtractionSingle(string filename, string outFilename, int w, int h, double secWhen)
			: base()
		{
			this.filename = filename;
            this.outFilename = outFilename;
            this.w = w;
            this.h = h;
            this.secWhen = secWhen;
		}

		public void run()
		{
			RunFfMpegThumbnail(filename);
		}

		public delegate void PleaseUnlockThisFileH(string filename);
		public event PleaseUnlockThisFileH PleaseUnlockThisFile;

		private void RunFfMpegThumbnail(string filename)
		{
			Process process = new Process();
			process.StartInfo.FileName = FfmpegExecutable;
            // source: http://blog.roberthallam.org/2010/06/extract-a-single-image-from-a-video-using-ffmpeg/comment-page-1/
            process.StartInfo.Arguments = String.Format("-ss {0} -i {1} -t 1 -s {2}x{3} -f image2 {4}", secWhen, filename, w, h, outFilename);
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.CreateNoWindow = true;
			runProcessRememberError(process);
		}
	}
}
