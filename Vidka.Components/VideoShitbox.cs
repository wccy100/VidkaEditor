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

namespace Vidka.Components {

	public partial class VideoShitbox : UserControl, IVideoShitbox
	{
		#region events
		public delegate void TogglePreviewMode_Handler();
		public event TogglePreviewMode_Handler PleaseTogglePreviewMode;

		public delegate void PleaseToggleConsoleVisibility_Handler();
		public event PleaseToggleConsoleVisibility_Handler PleaseToggleConsoleVisibility;


		public delegate void PleaseShowClipUsages_Handler();
		public event PleaseShowClipUsages_Handler PleaseShowClipUsages;

		public delegate void PleaseSetPlayerAbsPosition_Handler(PreviewPlayerAbsoluteLocation location);
		public event PleaseSetPlayerAbsPosition_Handler PleaseSetPlayerAbsPosition;

		public delegate void PleaseSetFormTitle_H(string title);
		public event PleaseSetFormTitle_H PleaseSetFormTitle;

        public delegate void ProjectUpdated_H();
        public event ProjectUpdated_H ProjectUpdated;
		#endregion

		// state
		private RichTextBox txtConsole;
		private bool isControlLoaded = false;
		public EditorLogic Logic { get; private set; }
		private ImageCacheManager imageMan;
		private EditorDrawOps drawOps;
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
			drawOps = new EditorDrawOps();
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
			if (e.Control && e.KeyCode.IsLRShiftKey())
				Logic.ControlPressed();
			if (e.Shift && e.KeyCode.IsLRShiftKey())
				Logic.ShiftPressed();
			//------- commented out because we have created menu shortcuts
			//if (e.Control && e.KeyCode == Keys.S) // these are controlled from MainForm now
			//	Logic.SaveTriggered();
			//else if (e.Control && e.KeyCode == Keys.O)
			//	Logic.OpenTriggered();
			//else if (e.Control && e.Shift && e.KeyCode == Keys.E)
			//	Logic.ExportToAvs();
			else if (e.Control && e.KeyCode == Keys.Oemplus)
				Logic.ZoomIn(Width);
			else if (e.Control && e.KeyCode == Keys.OemMinus)
				Logic.ZoomOut(Width);
			//else if (e.Alt && e.KeyCode == Keys.Enter)
			else if (e.KeyCode == Keys.F4)
				OpenClipProperties(Logic.UiObjects.CurrentClip);
			else if (e.Control && e.KeyCode == Keys.Space)
				Logic.PlayPause(onlyLockedClips: true);
			else if (e.KeyCode == Keys.Space)
				Logic.PlayPause(false);
			else if (e.Control && e.Shift && e.KeyCode == Keys.B)
				Logic.PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview, true);
			else if (e.Control && e.KeyCode == Keys.B)
                Logic.PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview, false);
            else if (e.Control && e.Shift && e.KeyCode == Keys.G)
                Logic.PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview2, true);
            else if (e.Control && e.KeyCode == Keys.G)
                Logic.PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview2, false);
			else if (e.KeyCode == Keys.Home)
				Logic.SetFrameMarker_0_ForceRepaint();
			else if (e.KeyCode == Keys.End)
				Logic.SetFrameMarker_End_ForceRepaint();
			else if (e.KeyCode == Keys.Enter)
				Logic.EnterPressed();
			else if (e.KeyCode == Keys.Escape)
				Logic.EscapePressed();
			else if (e.Control && e.KeyCode == Keys.Z)
				Logic.Undo();
			else if (e.Control && e.KeyCode == Keys.Y)
				Logic.Redo();
			else if (e.Control && e.KeyCode == Keys.C)
				Logic.CopyCurClipToClipboard();
			else if (e.Control && e.KeyCode == Keys.X)
				Logic.CutCurClipToClipboard();
			else if (e.Control && e.KeyCode == Keys.V)
				Logic.PasteClipFromClipboard();
			else if (e.Control && e.Shift && e.KeyCode == Keys.D)
				Logic.DuplicateCurClip();
			else if (e.KeyCode == Keys.S)
				Logic.SplitCurClipVideo(false);
			else if (e.KeyCode == Keys.L)
				Logic.SplitCurClipVideo(true);
			else if (e.KeyCode == Keys.A)
				Logic.SplitCurClipVideo_DeleteLeft();
			else if (e.KeyCode == Keys.D)
				Logic.SplitCurClipVideo_DeleteRight();
			else if (e.KeyCode == Keys.F)
				Logic.ToggleCurSelectedClip_IsLocked();
			else if (e.KeyCode == Keys.M)
				Logic.ToggleCurSelectedClip_IsMuted();
			else if (e.KeyCode == Keys.Delete)
				Logic.DeleteCurSelectedClip();
			else if (e.KeyCode == Keys.P) {
				if (PleaseTogglePreviewMode != null)
					PleaseTogglePreviewMode();
			}
			else if (e.KeyCode == Keys.O) {
				if (PleaseToggleConsoleVisibility != null)
					PleaseToggleConsoleVisibility();
			}
			else if (e.KeyCode == Keys.W) {
				if (PleaseShowClipUsages != null)
					PleaseShowClipUsages();
			}
            Logic.KeyPressed(e.KeyCode);
		}

		private void OpenClipProperties(VidkaClip clip)
		{
			if (clip == null)
				return;
			VidkaClip newClip = null;
            Form windowDialog = null;
            if (clip is VidkaClipVideoAbstract)
            {
                var window = new ClipPropertiesWindowVideo {
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
                var window = new ClipPropertiesWindowAudio {
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
			InvokeOrNot_IDontGiveAShit_JustDoIt(() => {
				this.AutoScrollMinSize = new Size(w, 50); // +100???
			});
		}

		public void UpdateCanvasHorizontalScroll(int scrollX) {
			InvokeOrNot_IDontGiveAShit_JustDoIt(() => {
				this.AutoScrollPosition = new Point(scrollX, 0);
			});
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
			InvokeOrNot_IDontGiveAShit_JustDoIt(() =>
			{
				if (txtConsole == null)
					return;
				if (txtConsole.IsDisposed)
					return;
				// TODO: implement logging filters in UI
				txtConsole.AppendText(text + "\n");
			});
		}

		public void AskTo_PleaseSetPlayerAbsPosition(PreviewPlayerAbsoluteLocation location) {
			if (PleaseSetPlayerAbsPosition != null)
				PleaseSetPlayerAbsPosition(location);
		}

		public void AskTo_PleaseSetFormTitle(string title) {
			if (PleaseSetFormTitle != null)
				PleaseSetFormTitle(title);
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

        public void ProjectLoaded()
        {
            if (ProjectUpdated != null)
                ProjectUpdated();
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

			drawOps.setParameters(e.Graphics, Logic, imageMan, Width, Height);

			//prepare canvas: paint strips for timelines, etc
			drawOps.PrepareCanvas();

			drawOps.DrawTimeAxis();

			// TODO: buffer an off-screen image of the entire project
			drawOps.DrawProjectVideoTimeline();

			drawOps.DrawProjectAudioTimeline();

			// draw hover clip outline
			if (Logic.UiObjects.CurrentVideoClipHover != null)
				drawOps.OutlineClipVideoHover();
			if (Logic.UiObjects.CurrentAudioClipHover != null)
				drawOps.OutlineClipAudioHover();

			if (Logic.UiObjects.OriginalTimelinePlaybackMode) {
				drawOps.DrawCurtainForOriginalPlayback();
			}
			else {
				drawOps.DrawCurrentFrameMarker();
			}

			if (Logic.UiObjects.CurrentVideoClip != null) {
				drawOps.DrawCurrentClipVideoOnOriginalTimeline();
			}
			if (Logic.UiObjects.CurrentAudioClip != null) {
				drawOps.DrawCurrentClipAudioOnOriginalTimeline();
			}

			drawOps.DrawDraggySeparately();
			imageMan.___paintEnd();
		}

		private void imageMan_ImagesReady()
		{
			Invalidate();
		}

		#endregion


		#region ---------------- helpers -------------------

		private void InvokeOrNot_IDontGiveAShit_JustDoIt(Action func) {
			if (InvokeRequired) {
				if (IsDisposed)
					return;
				Invoke(new MethodInvoker(func));
				return;
			}
			func();
		}

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
