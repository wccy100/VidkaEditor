using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core;
using Vidka.Core.Model;

namespace Vidka.Components
{
	public partial class CommonVideoClipProperties : UserControl
	{

		public CommonVideoClipProperties()
		{
			InitializeComponent();
		}

		public VidkaClipVideoAbstract VClip { get; private set; }
		
		public void SetParticulars(VidkaClipVideoAbstract clip)
		{
			VClip = clip;
			// set all the UI
            txtPostOp.Text = VClip.PostOp;
            txtLabel.Text = VClip.Label;
            chkIsPixelTypeStandard.Checked = VClip.IsPixelTypeStandard;
            chkIsRenderBreakupPoint.Checked = VClip.IsRenderBreakupPoint;
		}

		private void txtPostOp_TextChanged(object sender, EventArgs e)
		{
			VClip.PostOp = txtPostOp.Text;
		}

        private void txtLabel_TextChanged(object sender, EventArgs e)
        {
            VClip.Label = txtLabel.Text;
        }

        private void chkIsPixelTypeStandard_CheckedChanged(object sender, EventArgs e)
        {
            VClip.IsPixelTypeStandard = chkIsPixelTypeStandard.Checked;
        }

        private void chkIsRenderBreakupPoint_CheckedChanged(object sender, EventArgs e)
        {
            VClip.IsRenderBreakupPoint = chkIsRenderBreakupPoint.Checked;
        }

		private void btnRotate180_Click(object sender, EventArgs e)
		{
			txtPostOp.AppendText(".FlipHorizontal().FlipVertical()\n");
		}

		private void btnFlipH_Click(object sender, EventArgs e)
		{
			txtPostOp.AppendText(".FlipHorizontal()\n");
		}

		private void btnFlipV_Click(object sender, EventArgs e)
		{
			txtPostOp.AppendText(".FlipVertical()\n");
		}

		private void btnFill_Click(object sender, EventArgs e)
		{
            VideoShitbox.ConsoleSingleton.cxzxc("");
		}

        private void btnFadeIn5_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeIn(5)\n");
        }

        private void btnFadeOut5_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeOut(5)\n");
        }

        private void btnFadeoutAudio5_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeOutAudioOnly(5)\n");
        }

	}
}
