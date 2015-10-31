using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Components;

namespace Vidka.Components
{
	public partial class ClipPropertiesWindowAudio : Form
	{
		public ClipPropertiesWindowAudio()
		{
			InitializeComponent();
		}

        public CommonAudioClipProperties CommonPropertiesControl { get { return commonAudioClipProperties; } }

		public void AddImportantTab(string title, Control control)
		{
			TabPage myTabPage = new TabPage(title);
			myTabPage.Controls.Add(control);
			var oldTabs = tabControl.TabPages.Cast<TabPage>().ToList();
			tabControl.TabPages.Clear();
			tabControl.TabPages.Add(myTabPage);
			tabControl.TabPages.AddRange(oldTabs.ToArray());
		}

		public void AddNonImportantTab(string title, Control control)
		{
			TabPage myTabPage = new TabPage(title);
			myTabPage.Controls.Add(control);
			tabControl.TabPages.Add(myTabPage);
		}

		private void VideoClipPropertiesWindow_Load(object sender, EventArgs e) { }
	}
}
