using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;

namespace Vidka.Core.Ops
{
    public enum ExternalPlayerType
    {
        Mplayer = 1,
        VirtualDub = 2,
    }

	public class MPlayerPlaybackSegment : OpBaseClass
	{
		private const string TMP_FILENAME = "MPlayerPlaybackSegment-temp.avs";
        private VidkaProj proj;
        private long frameStart;
        private long framesLength;
        private bool onlyLockedClips;
        public bool doCrop;
        public ExternalPlayerType ExternalPlayer { get; set; }

		public MPlayerPlaybackSegment(VidkaProj proj)
		{
            this.proj = proj;
            ExternalPlayer = ExternalPlayerType.Mplayer;
            doCrop = false;
		}

        public void WhileYoureAtIt_cropProj(long frameStart, long framesLength, bool onlyLockedClips)
        {
            doCrop = true;
            this.frameStart = frameStart;
            this.framesLength = framesLength;
            this.onlyLockedClips = onlyLockedClips;
        }

        public void run()
        {
            var projCropped = proj;
            if (doCrop)
                projCropped = proj.Crop(
                    frameStart,
                    framesLength,
                    null, //proj.Width / 4,
                    null, //proj.Height / 4,
                    onlyLockedClips);
            if (projCropped.ClipsVideo.Count == 0)
            {
                ResultCode = OpResultCode.OtherError;
                ErrorMessage = "There are no locked clips from this point on!";
                return;
            }
            var tmpAvsPath = VidkaIO.GetFileFromThisAppDirectory(TMP_FILENAME);
            VidkaIO.ExportToAvs(projCropped, tmpAvsPath);
            RunMPlayer(tmpAvsPath, proj);
        }

		private void RunMPlayer(string filenameAvs, VidkaProj proj)
		{
			Process process = new Process();
            if (ExternalPlayer == ExternalPlayerType.Mplayer)
            {
                process.StartInfo.FileName = OpBaseClass.MplayerExecutable;
                process.StartInfo.Arguments = String.Format("\"{0}\" -vo gl -noautosub -geometry {1}x{2} -idle -fixed-vo -loop 1000",
				    filenameAvs,
				    proj.Width,
				    proj.Height);
            }
            else if (ExternalPlayer == ExternalPlayerType.VirtualDub)
            {
                process.StartInfo.FileName = OpBaseClass.VirtualDubExecutable;
                process.StartInfo.Arguments = String.Format("\"{0}\"", filenameAvs);
            }
			process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
			//process.StartInfo.CreateNoWindow = true;

			runProcessRememberError(process);
		}

    }
}
