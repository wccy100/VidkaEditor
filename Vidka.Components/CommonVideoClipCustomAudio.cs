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
using Vidka.Core.Model;
using Vidka.Core.ExternalOps;
using Vidka.Core.VideoMeta;
using Vidka.Core;

namespace Vidka.Components
{
    public partial class CommonVideoClipCustomAudio : UserControl
    {
        private VidkaClipVideoAbstract vclip, vclipFullToDraw;
        private MetaGeneratorInOtherThread metaGenerator;
        private VidkaFileMapping fileMapping;
        private VidkaProj proj;

        public CommonVideoClipCustomAudio()
        {
            InitializeComponent();
        }


        #region ------------------- ui events ---------------------

        private void shitboxAlignVideoAudioControl_Load(object sender, EventArgs e) { }
        private void CommonVideoClipCustomAudio_Load(object sender, EventArgs e)
        {
            //shitboxAlignVideoAudioControl.OffsetUpdated += shitboxAlignVideoAudioControl_OffsetUpdated;
        }

        private void panelDragFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string file1 = files.FirstOrDefault();
                if (DragAndDropManager.IsFilenameAudio(file1))
                {
                    e.Effect = DragDropEffects.Copy;
                    panelDragFile.BackColor = Color.Azure;
                }
            }
        }

        private void panelDragFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file1 = files.FirstOrDefault();
            if (DragAndDropManager.IsFilenameAudio(file1))
                SetFilename(file1);
            panelDragFile.BackColor = Color.LightGray;
        }

        private void panelDragFile_DragLeave(object sender, EventArgs e)
        {
            panelDragFile.BackColor = Color.LightGray;
        }

        private void chkHasCustomAudio_CheckedChanged(object sender, EventArgs e)
        {
            if (vclip == null)
                return;
            vclip.HasCustomAudio = chkHasCustomAudio.Checked;
            updateDisabilityOfControlBasedOnCheckbox();
        }

        //private void shitboxAlignVideoAudioControl_OffsetUpdated(float offset)
        //{
        //    txtOffset.Text = "" + offset;
        //}

        private void btnDown1_Click(object sender, EventArgs e) { IncrementOffset(-1f); }
        private void btnDown01_Click(object sender, EventArgs e) { IncrementOffset(-0.1f); }
        private void btnUp01_Click(object sender, EventArgs e) { IncrementOffset(0.1f); }
        private void btnUp1_Click(object sender, EventArgs e) { IncrementOffset(1f); }

        private void txtOffset_TextChanged(object sender, EventArgs e)
        {
            if (vclip == null)
                return;
            float tryParseVal;
            if (!float.TryParse(txtOffset.Text, out tryParseVal))
                txtOffset.Text = "" + vclip.CustomAudioOffset;
            else
                vclip.CustomAudioOffset = tryParseVal;
            //shitboxAlignVideoAudioControl.Invalidate();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var sampleProj = new VidkaProj
            {
                Width = proj.Width,
                Height = proj.Height,
                FrameRate = proj.FrameRate,
            };
            vclipFullToDraw.HasCustomAudio = vclip.HasCustomAudio;
            vclipFullToDraw.CustomAudioFilename = vclip.CustomAudioFilename;
            vclipFullToDraw.CustomAudioLengthSec = vclip.CustomAudioLengthSec;
            vclipFullToDraw.CustomAudioOffset = vclip.CustomAudioOffset;
            sampleProj.ClipsVideo.Add(vclipFullToDraw);

            var mplayed = new MPlayerPlaybackSegment(sampleProj);
            mplayed.ExternalPlayer = ExternalPlayerType.VirtualDub;
            mplayed.run();
            if (mplayed.ResultCode == OpResultCode.FileNotFound)
                MessageBox.Show("Please make sure " + mplayed.ExternalPlayer + " in your PATH", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (mplayed.ResultCode == OpResultCode.OtherError)
                MessageBox.Show("Error: " + mplayed.ErrorMessage, "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region ------------------- functions ---------------------

        public void SetParticulars(
            VidkaClipVideoAbstract vclip,
            MetaGeneratorInOtherThread metaGenerator,
            VidkaFileMapping fileMapping,
            VidkaProj proj)
        {
            this.vclip = vclip;
            this.metaGenerator = metaGenerator;
            this.fileMapping = fileMapping;
            this.proj = proj;
            // ..... set up the vclip that we will draw
            vclipFullToDraw = vclip.MakeCopy_VideoClip();
            vclipFullToDraw.FrameStart = 0;
            vclipFullToDraw.FrameEnd = vclipFullToDraw.LengthFrameCalc;
            // ..... set up UI
            chkHasCustomAudio.Checked = vclip.HasCustomAudio;
            txtOffset.Text = "" + vclip.CustomAudioOffset;
            //shitboxAlignVideoAudioControl.SetParticulars(vclip, fileMapping);
            SetFilenameLabel(vclip.CustomAudioFilename);
            updateAudioInfo(vclip);
            updateDisabilityOfControlBasedOnCheckbox();
        }

        private void SetFilenameLabel(string filename)
        {
            lblFilename.Text = (filename != null)
                ? Path.GetFileName(filename)
                : "Drag file please...";
        }

        private void SetFilename(string filename)
        {
            if (vclip == null)
            {
                MessageBox.Show("For some strange reason the video clip did not get set to this control...", "Unexpected error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            vclip.CustomAudioFilename = filename;
            vclip.HasCustomAudio = (filename != null);
            lblAudioProperties.Text = "Audio info: ...processing, please wait...";
            metaGenerator.RequestMeta(
                filename: filename,
                customCallback: callbackMetaReady);
            metaGenerator.RequestWaveOnly(
                filename: filename,
                customCallback: callbackWaveReady);
            SetFilenameLabel(filename);
            chkHasCustomAudio.Checked = true;
            updateDisabilityOfControlBasedOnCheckbox();
        }

        private void updateDisabilityOfControlBasedOnCheckbox()
        {
            btnUp1.Enabled = chkHasCustomAudio.Checked;
            btnUp01.Enabled = chkHasCustomAudio.Checked;
            btnDown1.Enabled = chkHasCustomAudio.Checked;
            btnDown01.Enabled = chkHasCustomAudio.Checked;
            txtOffset.Enabled = chkHasCustomAudio.Checked;
            btnPreview.Enabled = chkHasCustomAudio.Checked;
            //label1.Enabled = chkHasCustomAudio.Checked;
            //label2.Enabled = chkHasCustomAudio.Checked;
        }

        private void updateAudioInfo(VidkaClipVideoAbstract vclip)
        {
            if (vclip == null) {
                lblAudioProperties.Text = "Audio info: ---";
                waveImageBox.ImageLocation = null;
                return;
            }
            lblAudioProperties.Text = (!vclip.CustomAudioLengthSec.HasValue)
                ? "Audio info: ---"
                : lblAudioProperties.Text = String.Format("Audio info: {0}", TimeSpan.FromSeconds(vclip.CustomAudioLengthSec ?? 0).ToString_MinuteOrHour());
            var waveFilename = fileMapping.AddGetWaveFilenameJpg(vclip.CustomAudioFilename);
            waveImageBox.ImageLocation = (vclip.HasCustomAudio && File.Exists(waveFilename))
                ? waveFilename
                : null;
        }

        private void callbackWaveReady(string filename, string filenameWave, string filenameWaveJpg)
        {
            InvokeOrNot_IDontGiveAShit_JustDoIt(() => updateAudioInfo(vclip));
            //shitboxAlignVideoAudioControl.Invalidate();
        }

        private void callbackMetaReady(string filename, VideoMetadataUseful metaXml)
        {
            if (vclip == null)
                return;
            vclip.CustomAudioLengthSec = metaXml.AudioDurationSec;
            InvokeOrNot_IDontGiveAShit_JustDoIt(() => updateAudioInfo(vclip));
            //shitboxAlignVideoAudioControl.Update_audionDuration(VClip.CustomAudioLengthSec);
        }

        private void IncrementOffset(float delta)
        {
            if (vclip == null)
                return;
            vclip.CustomAudioOffset += delta;
            txtOffset.Text = "" + vclip.CustomAudioOffset;
            //shitboxAlignVideoAudioControl.Invalidate();
        }

        private void InvokeOrNot_IDontGiveAShit_JustDoIt(Action func) {
			if (InvokeRequired) {
				if (IsDisposed)
					return;
				Invoke(new MethodInvoker(func));
				return;
			}
			func();
		}

        #endregion
        
    }
}
