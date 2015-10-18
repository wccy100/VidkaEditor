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
	public partial class SimpleTextSettings : UserControl
	{
		public SimpleTextSettings()
		{
			InitializeComponent();
		}
		private void SimpleTextSettings_Load(object sender, EventArgs e) {}

		public VidkaClipTextSimple VClip { get; private set; }

		public void SetVideoClip(VidkaClipTextSimple clip)
		{
			VClip = clip;
			// set all the UI
			txt.Text = VClip.Text;
			ddlFontSize.SelectedItem = VClip.FontSize.ToString();
			Color clrBckground = Color.FromArgb(VClip.ArgbBackgroundColor);
			Color clrFont = Color.FromArgb(VClip.ArgbFontColor);
			pnlColor.BackColor = clrBckground;
			pnlColor.Invalidate();
			colorDialog1.Color = clrBckground;
			pnlFontColor.BackColor = clrFont;
			pnlFontColor.Invalidate();
			colorDialog2.Color = clrFont;
		}

		private void txt_TextChanged(object sender, EventArgs e)
		{
			VClip.Text = txt.Text;
		}

		private void btnBackground_Click(object sender, EventArgs e)
		{
			var result = colorDialog1.ShowDialog();
			if (result == DialogResult.OK)
			{
				pnlColor.BackColor = colorDialog1.Color;
				VClip.ArgbBackgroundColor = colorDialog1.Color.ToArgb();
			}
		}

		private void btnFontColor_Click(object sender, EventArgs e)
		{
			var result = colorDialog2.ShowDialog();
			if (result == DialogResult.OK)
			{
				pnlFontColor.BackColor = colorDialog2.Color;
				VClip.ArgbFontColor = colorDialog2.Color.ToArgb();
			}
		}

		private void ddlFontSize_SelectedIndexChanged(object sender, EventArgs e)
		{
			var str = ddlFontSize.SelectedItem.ToString();
			int size = 20;
			int.TryParse(str, out size);
			VClip.FontSize = size;
		}

	}
}
