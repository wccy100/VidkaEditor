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

namespace Vidka.Components
{
	public partial class CommonAudioClipProperties : UserControl
	{

        public CommonAudioClipProperties()
		{
			InitializeComponent();
		}

		public VidkaClipAudio VClip { get; private set; }

        public void SetParticulars(VidkaClipAudio clip)
		{
			VClip = clip;
			// set all the UI
			txtPostOp.Text = VClip.PostOp;
		}

		private void txtPostOp_TextChanged(object sender, EventArgs e)
		{
			VClip.PostOp = txtPostOp.Text;
		}

        private void btnFadeOut5_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeOut(5)\n");
        }

        private void btnFadeOut10_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeOut(10)\n");
        }

        private void btnFadeIn5_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeIn(5)\n");
        }

        private void btnFadeIn10_Click(object sender, EventArgs e)
        {
            txtPostOp.AppendText(".FadeIn(10)\n");
        }

        private void CommonAudioClipProperties_Load(object sender, EventArgs e)
        {

        }

	}
}
