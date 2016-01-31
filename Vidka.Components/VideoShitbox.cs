using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Vidka.Core;
using Vidka.Core.VideoMeta;
using System.Runtime.InteropServices;
using Vidka.Core.Model;
using System.Windows;
using Vidka.Components.Properties;
using Vidka.Components.VideoShitboxDrawOps;
using Miktemk.Winforms;

namespace Vidka.Components {

	public partial class VideoShitbox : UserControl, IVideoShitbox
	{
		// state
		private RichTextBox txtConsole;
		private bool isControlLoaded = false;
		public EditorLogic Logic { get; private set; }
		private ImageCacheManager imageMan;
        private DrawOpsCollection vidkaDrawOps;
        private bool mouseDown;
		private int prevDragX = 0; // for drag/drop files
		private int mouseDownX, mouseDownY;
		private long repaintCount = 0;

		// singletonhack
		public static IVidkaConsole ConsoleSingleton;

		public VideoShitbox() {
			InitializeComponent();
			imageMan = new ImageCacheManager();
			imageMan.ImagesReady += imageMan_ImagesReady;
			mouseDown = false;
			ConsoleSingleton = this;
		}

		private void VideoShitbox_Load(object sender, EventArgs e) {
			isControlLoaded = true;
			this.AutoScroll = true;
			this.MouseWheel += VideoShitbox_MouseWheel;
			this.DoubleBuffered = true;
			if (Logic != null)
				Logic.UiInitialized();
		}

		#region ================================ scrolling bullshit ================================

		private void VideoShitbox_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll) {
				Logic.setScrollX(e.NewValue);
				Invalidate();
			}
		}

		private void VideoShitbox_MouseWheel(object sender, MouseEventArgs e)
		{
			if (Control.ModifierKeys == Keys.Control)
			{
				// don't perform the scroll thingie
				var hme = (HandledMouseEventArgs)e;
				hme.Handled = true;
				Invalidate();
				if (e.Delta > 0)
					Logic.ZoomIn(Width);
				else
					Logic.ZoomOut(Width);
				return;
			}

			Logic.setScrollX(this.HorizontalScroll.Value);
			Invalidate();
		}

		// the following mumbo jumbo of code redirects scrolls to control the horizontal
		// taken from: http://stackoverflow.com/questions/13034909/mouse-wheel-scroll-horizontally
		// but apparently we don't need this bullshit

		//private const int WM_SCROLL = 276; // Horizontal scroll 
		//private const int SB_LINELEFT = 0; // Scrolls one cell left 
		//private const int SB_LINERIGHT = 1; // Scrolls one line right
		//[DllImport("user32.dll", CharSet = CharSet.Auto)]
		//private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
		//private void panelInner_MouseWheel(object sender, MouseEventArgs e)
		//{
		//	if (ModifierKeys == Keys.Shift)
		//	{
		//		var direction = e.Delta > 0 ? SB_LINELEFT : SB_LINERIGHT;

		//		SendMessage(this.Handle, WM_SCROLL, (IntPtr)direction, IntPtr.Zero);
		//	}
		//}

		#endregion scrolling

		#region ================================ drag drop ================================

		private void VideoShitbox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.Copy;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Logic.MediaFileDragEnter(files, Width);
                //foreach (string file in files)
                //	Trace.WriteLine(file);
            }
		}

		private void VideoShitbox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Logic.MediaFileDragDrop(files);
                Invalidate();
                prevDragX = -1;
            }
		}

		private void VideoShitbox_DragOver(object sender, DragEventArgs e)
		{
			if (prevDragX == e.X)
				return;
			prevDragX = e.X;
			var point = this.PointToClient(new Point(e.X, e.Y));
			Logic.MediaFileDragMove(point.X);
        }

		private void VideoShitbox_DragLeave(object sender, EventArgs e)
		{
			Logic.CancelDragDrop();
        }

		#endregion

		#region ================================ interface events (key, mouse, touch) ================================

		public void VideoShitbox_KeyDown(object sender, KeyEventArgs e)
		{
            Logic.KeyPressed(e);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData) {
				case Keys.Left:
				case Keys.Right:
				case Keys.Control | Keys.Left:
				case Keys.Control | Keys.Right:
				case Keys.Shift | Keys.Left:
				case Keys.Shift | Keys.Right:
					Logic.LeftRightArrowKeys(keyData);
					break;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void VideoShitbox_MouseClick(object sender, MouseEventArgs e)
		{
			//Logic.MouseClicked(e.X, e.Y, Width, Height);
		}

		private void VideoShitbox_MouseDown(object sender, MouseEventArgs e)
		{
			mouseDown = true;
			mouseDownX = e.X;
			mouseDownY = e.Y;
			Logic.MouseDragStart(e.Button, e.X, e.Y, Width, Height);
		}

		private void VideoShitbox_MouseUp(object sender, MouseEventArgs e)
		{
			mouseDown = false;
			Logic.MouseDragEnd(e.X, e.Y, e.X - mouseDownX, e.Y - mouseDownY, Width, Height);
		}

		private void VideoShitbox_MouseMove(object sender, MouseEventArgs e)
		{
			if (mouseDown) {
				Logic.MouseDragged(e.X, e.Y, e.X - mouseDownX, e.Y - mouseDownY, Width, Height);
			}
			else
				Logic.MouseMoved(e.X, e.Y, Width, Height);
		}

		private void VideoShitbox_MouseLeave(object sender, EventArgs e)
		{
			Logic.MouseLeave();
		}

		private void VideoShitbox_Resize(object sender, EventArgs e)
		{
			Invalidate();
		}

		#endregion

		#region ================================ IVideoShitbox interface shit ================================

		//public void SetDraggy(VideoMetadataUseful meta) {
		//	State.IsDraggy = true;
		//	Invalidate();
		//	Trace.WriteLine("filename: " + meta.Filename);
		//	Trace.WriteLine("video length: " + meta.VideoDurationSec);
		//	Trace.WriteLine("audio length: " + meta.AudioDurationSec);
		//}

		public void PleaseRepaint()
		{
			Invalidate();
		}

		public void UpdateCanvasWidth(int w) {
			this.InvokeOrNot_IDontGiveAShit_JustDoIt(() => {
				this.AutoScrollMinSize = new Size(w, 50); // +100???
			});
		}

		public void UpdateCanvasHorizontalScroll(int scrollX) {
            this.InvokeOrNot_IDontGiveAShit_JustDoIt(() =>
            {
				this.AutoScrollPosition = new Point(scrollX, 0);
			});
		}

        public int GetHorizontalScrollBarHeight()
        {
            return this.HorizontalScroll.Visible
                ? SystemInformation.HorizontalScrollBarHeight
                : 0;
        }

		public string OpenProjectSaveDialog()
		{
			var dialog = new SaveFileDialog();
			dialog.Filter = "Vidka project (*.vidka)|*.vidka|All files (*.*)|*.*";
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
				return dialog.FileName;
			return null;
		}

		public string OpenProjectOpenDialog()
		{
			var dialog = new OpenFileDialog();
			dialog.Filter = "Vidka project (*.vidka)|*.vidka|All files (*.*)|*.*";
			var result = dialog.ShowDialog();
			if (result == DialogResult.OK)
				return dialog.FileName;
			return null;
		}

		public void AppendToConsole(VidkaConsoleLogLevel level, string text)
		{
            this.InvokeOrNot_IDontGiveAShit_JustDoIt(() =>
			{
				if (txtConsole == null)
					return;
				if (txtConsole.IsDisposed)
					return;
				// TODO: implement logging filters in UI
				txtConsole.AppendText(text + "\n");
			});
		}

		public void ShowErrorMessage(string title, string message)
		{
			MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public bool ShowConfirmMessage(string title, string message)
		{
			var result = MessageBox.Show(message, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			return result == DialogResult.OK;
		}

        public bool ShowInputMessage(string title, string message, string oldValue, out string newValue)
        {
            var result = DialogInput.ShowInputDialog(title, message, oldValue, out newValue);
            return result == DialogResult.OK;
        }

		public void PleaseUnlockThisFile(string filename)
		{
			imageMan.UnlockFileIfInUse(filename);
		}

        public bool ShouldIProceedIfProjectChanged()
        {
            if (Logic.IsFileChanged && !Settings.Default.SuppressChangedFilePromptOnClose)
            {
                var wantToSave = MessageBox.Show("Save changes to " + Logic.CurFileNameShort, "Save?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (wantToSave == DialogResult.Yes)
                    Logic.SaveTriggered();
                else if (wantToSave == DialogResult.Cancel)
                    return false;
            }
            return true;
        }

        public void OpenClipProperties(VidkaClip clip)
        {
            if (clip == null)
                return;
            VidkaClip newClip = null;
            Form windowDialog = null;
            if (clip is VidkaClipVideoAbstract)
            {
                var window = new ClipPropertiesWindowVideo
                {
                    Text = "Advanced clip properties",
                };
                windowDialog = window;
                if (clip is VidkaClipVideo)
                {
                    var vclip = (VidkaClipVideo)clip;
                    var vclip2 = vclip.MakeCopy_VideoClip();
                    newClip = vclip2;
                    window.CommonPropertiesControl.SetParticulars(vclip2);
                    window.CommonCustomAudioControl.SetParticulars(vclip2, Logic.MetaGenerator, Logic.FileMapping, Logic.Proj);
                }
                else if (clip is VidkaClipImage)
                {
                    var vclip = (VidkaClipImage)clip;
                    var vclip2 = vclip.MakeCopy_VideoClip();
                    newClip = vclip2;
                    window.CommonPropertiesControl.SetParticulars(vclip2);
                    window.CommonCustomAudioControl.SetParticulars(vclip2, Logic.MetaGenerator, Logic.FileMapping, Logic.Proj);
                    //window.AddImportantTab("");
                }
                else if (clip is VidkaClipTextSimple)
                {
                    var vclip = (VidkaClipTextSimple)clip;
                    var vclip2 = (VidkaClipTextSimple)vclip.MakeCopy_VideoClip();
                    newClip = vclip2;
                    window.CommonPropertiesControl.SetParticulars(vclip2);
                    window.CommonCustomAudioControl.SetParticulars(vclip2, Logic.MetaGenerator, Logic.FileMapping, Logic.Proj);
                    var textCreationControl = new SimpleTextSettings();
                    textCreationControl.SetVideoClip(vclip2);
                    window.AddImportantTab("Text", textCreationControl);
                }
            }
            else if (clip is VidkaClipAudio)
            {
                var window = new ClipPropertiesWindowAudio
                {
                    Text = "Advanced clip properties",
                };
                windowDialog = window;
                var aclip = (VidkaClipAudio)clip;
                var aclip2 = aclip.MakeCopy_AudioClip();
                newClip = aclip2;
                window.CommonPropertiesControl.SetParticulars(aclip2);
            }

            // use this dialog window to edit this clip
            if (newClip != null && windowDialog != null)
            {
                var result = windowDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    Logic.ReplaceClip(clip, newClip);
            }
        }

		#endregion

		#region ================================ object exchange ================================

		public void GuessWhoIsConsole(RichTextBox txtConsole)
		{
			this.txtConsole = txtConsole;
			AppendToConsole(VidkaConsoleLogLevel.Info, "Ciao Amore! Ready to rock and roll...");
		}

		public void setLogic(EditorLogic logic) {
			Logic = logic;
			if (isControlLoaded)
				Logic.UiInitialized();
            // .... these are all the classes that draw shit on in the VideoShitbox control
            vidkaDrawOps = new DrawOpsCollection(new DrawOp[] {
                new Draw0Canvas(Logic, imageMan),
                new Draw0Axis(Logic, imageMan),
                new DrawVideoClips(Logic, imageMan),
                new DrawAudioClips(Logic, imageMan),
                new DrawRenderBreakups(Logic, imageMan),
                new DrawVideoAudioAligns(Logic, imageMan),
                new DrawVideoAudioLinkage(Logic, imageMan),
                new DrawVideoHover(Logic, imageMan),
                new DrawAudioHover(Logic, imageMan),
                new DrawCurFrameMarker(Logic, imageMan),
                new DrawOriginalVideo(Logic, imageMan),
                new DrawOriginalAudio(Logic, imageMan),
                new DrawDraggySeparately(Logic, imageMan),
            });
            Invalidate();
		}

		#endregion

		#region ================================ paint! ================================

		private void VideoShitbox_Paint(object sender, PaintEventArgs e)
		{
			if (Logic == null)
				return;

			imageMan.___paintBegin();

			// debug why do we repaint 2 times when scrolling???
			repaintCount++;
			//cxzxc("y repaint 2x:" + repaintCount);

            if (vidkaDrawOps != null)
                vidkaDrawOps.Paint(e.Graphics, Width, Height);

			imageMan.___paintEnd();
		}

		private void imageMan_ImagesReady()
		{
			Invalidate();
		}

		#endregion


		#region ---------------- helpers -------------------

		/// <summary>
		/// Debug print to UI console
		/// </summary>
		private void cxzxc(string text)
		{
			AppendToConsole(VidkaConsoleLogLevel.Debug, text);
		}

		#endregion

    }
}
