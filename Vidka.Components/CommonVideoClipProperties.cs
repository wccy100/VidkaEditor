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
		}

		private void txtPostOp_TextChanged(object sender, EventArgs e)
		{
			VClip.PostOp = txtPostOp.Text;
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

		}
	}
}
