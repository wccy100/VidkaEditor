using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core;
using Vidka.Core.ExternalOps;

namespace Vidka.Components
{
	public partial class ShitboxAlignVideoAudio : UserControl
	{
		#region events
		public delegate void OffsetUpdated_H(float offset);
		public event OffsetUpdated_H OffsetUpdated;
		#endregion

		private ImageCacheManager imageMan;
		private EditorDrawOps drawOps;
        private VidkaClipVideoAbstract vclip, vclipFullToDraw;
        private VidkaClipAudio aclipToDraw;
		private ProjectDimensions dimdim;
		private VidkaFileMapping fileMapping;
		private VidkaProj sampleProj;
		private int mouseX1;
		private bool isDown;
		private float originalOffset;

		public ShitboxAlignVideoAudio()
		{
			sampleProj = new VidkaProj();
			dimdim = new ProjectDimensions(sampleProj);
			drawOps = new EditorDrawOps();

			InitializeComponent();
		}

		public void SetParticulars(
			VidkaClipVideoAbstract vclip,
			VidkaFileMapping fileMapping)
		{
			this.imageMan = new ImageCacheManager();
			this.fileMapping = fileMapping;
			this.vclip = vclip;
            // set up the vclip that we will draw
            vclipFullToDraw = vclip.MakeCopy_VideoClip();
            vclipFullToDraw.FrameStart = 0;
            vclipFullToDraw.FrameEnd = vclipFullToDraw.LengthFrameCalc;
            // set up the audio clip that we will draw
            aclipToDraw = new VidkaClipAudio()
            {
                FileName = vclip.CustomAudioFilename,
                FileLengthSec = vclip.CustomAudioLengthSec,
                FileLengthFrames = dimdim.SecToFrame(vclip.CustomAudioLengthSec ?? 0),
                FrameStart = 0,
                FrameEnd = dimdim.SecToFrame(vclip.CustomAudioLengthSec ?? 0),
            };
            imageMan.ImagesReady += imageMan_ImagesReady;
		}

        public void Update_audionDuration(double? lengthSec)
        {
            vclipFullToDraw.CustomAudioLengthSec = lengthSec;
            aclipToDraw.FileLengthSec = lengthSec;
            aclipToDraw.FileLengthFrames = dimdim.SecToFrame(vclip.CustomAudioLengthSec ?? 0);
            aclipToDraw.FrameStart = 0;
            aclipToDraw.FrameEnd = dimdim.SecToFrame(vclip.CustomAudioLengthSec ?? 0);
            Invalidate();
        }

		private void ShitboxAlignVideoAudio_Load(object sender, EventArgs e) { }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            imageMan.ImagesReady -= imageMan_ImagesReady;
        }

		private void imageMan_ImagesReady()
		{
			Invalidate();
		}

		#region ================================ paint! ================================

		private void ShitboxAlignVideoAudio_Paint(object sender, PaintEventArgs e)
		{
			if (imageMan == null)
				return;
            if (vclip == null)
                return;

			imageMan.___paintBegin();

			drawOps.setParameters2(e.Graphics, imageMan, dimdim, fileMapping, null, sampleProj, Width, Height);

            drawOps.AlignVideoAudio_drawVideo(vclipFullToDraw);
            drawOps.AlignVideoAudio_drawAudio(aclipToDraw, vclip.CustomAudioOffset);

			imageMan.___paintEnd();
		}

		#endregion

		#region ================================ mouse interaction ================================

		private void ShitboxAlignVideoAudio_MouseDown(object sender, MouseEventArgs e)
		{
			if (vclip == null)
				return;
			mouseX1 = e.X;
			isDown = true;
			originalOffset = vclip.CustomAudioOffset;
		}

		private void ShitboxAlignVideoAudio_MouseMove(object sender, MouseEventArgs e)
		{
			if (!isDown)
				return;
            if (vclip == null)
                return;
			var dX = e.X - mouseX1;
			var dTs = dimdim.convert_AbsX2Seconds(dX);
			vclip.CustomAudioOffset = originalOffset + dTs;
			if (OffsetUpdated != null)
				OffsetUpdated(vclip.CustomAudioOffset);
		}

		private void ShitboxAlignVideoAudio_MouseUp(object sender, MouseEventArgs e)
		{
			isDown = false;
		}

		#endregion

    }
}
