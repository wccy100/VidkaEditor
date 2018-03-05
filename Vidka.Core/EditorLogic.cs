﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Vidka.Core.Model;
using System.Threading;
using System.Xml.Serialization;
using System.Windows.Forms;
using Vidka.Core.VideoMeta;
using Vidka.Core.ExternalOps;
using Vidka.Core.Properties;
using Vidka.Core.Ops;
using Vidka.Core.OpsMouse;
using Vidka.Core.UiObj;
using Miktemk;
using Miktemk.Editor;

namespace Vidka.Core
{

    public class EditorLogic : IVidkaOpContext
	{
        private bool Debug_outputEditOpLifecycle = true;

		// constants

		/// <summary>
		/// Big step Alt+left/Alt+right
		/// </summary>
		private const int MANY_FRAMES_STEP = 50;
		/// <summary>
		/// Scroll adjustment when marker gets out of bounds and triggers scroll change
		/// </summary>
		private const int SCREEN_MARKER_JUMP_LEEWAY = 10;
		/// <summary>
		/// Pixels to clip border within which bound drag is enabled
		/// </summary>
		private const int BOUND_THRESH_MAX = 30;
        /// <summary>
        /// Pixels to clip border within which bound drag is enabled
        /// </summary>
        private const int BOUND_EASING_BOTTOM_PIXELS = 20;
		/// <summary>
		/// Once zoomed out and clips are too small
		/// </summary>
		private const int BOUND_THRESH_MIN = 5;

		private const string VidkaClipboardHolderFormat = "VidkaClipboardData";

        #region events

        public delegate void VoidHandler();
        public delegate void PleaseSetPlayerAbsPosition_Handler(PreviewPlayerAbsoluteLocation location);
        public delegate void StringParamHandler(string title);
        private void FireHandler(VoidHandler x)
        {
            if (x != null)
                x();
        }

        public event VoidHandler PleaseTogglePreviewMode;
        public event VoidHandler PleaseToggleConsoleVisibility;
        public event VoidHandler ProjectUpdated_AsFarAsMenusAreConcerned;
        public event PleaseSetPlayerAbsPosition_Handler PleaseSetPlayerAbsPosition;
        public event StringParamHandler PleaseSetFormTitle;

        #endregion

		// what we are working with
		private IVideoShitbox shitbox;
		private IVideoPlayer videoPlayer;

        // helper and logic classes
        private UndoRedoManager editActionsManager;
        private PreviewThreadLauncher previewLauncher;
		private MouseOpAbstract CurEditOp;
		private VidkaIO ioOps;
		private DragAndDropManager dragAndDropMan;
		private MouseOpAbstract[] EditOpsAll;
        private _VidkaOp[] Ops;
        private VidkaProj Proj_forOriginalPlayback; // fake proj used to playback on the original timeline (when the curtain/OriginalTimelinePlaybackMode is on)

		// ... for other helper classes see the "object exchange" region

		// my own state shit
		private string curFilename;
		private int mouseX;
		private int? needToChangeCanvasWidth;
		private int? needToChangeScrollX;
        private ExportToAvi_Segment exportToSegment;

        public EditorLogic(IVideoShitbox editor, IVideoPlayer videoPlayer, IAudioPlayer playerAudio)
		{
			this.shitbox = editor;
			this.videoPlayer = videoPlayer;
			Proj = new VidkaProj();
			Dimdim = new ProjectDimensions(Proj);
            editActionsManager = new UndoRedoManager();
            UiObjects = new VidkaUiStateObjects();
            previewLauncher = new PreviewThreadLauncher(videoPlayer, playerAudio, this);
			ioOps = new VidkaIO();
			Proj_forOriginalPlayback = new VidkaProj();
			IsFileChanged = false;

			FileMapping = Settings.Default.DataNearProject
				? (VidkaFileMapping)new VidkaFileMapping_proj()
				: (VidkaFileMapping)new VidkaFileMapping_resource();
			dragAndDropMan = new DragAndDropManager(editor, Proj, FileMapping);
			dragAndDropMan.MetaReadyForDraggy += dragAndDropMan_MetaReadyForDraggy;
			dragAndDropMan.MetaReadyForOutstandingVideo += dragAndDropMan_MetaReadyForOutstandingVideo;
			dragAndDropMan.MetaReadyForOutstandingAudio += dragAndDropMan_MetaReadyForOutstandingAudio;
			dragAndDropMan.ThumbOrWaveReady += dragAndDropMan_ThumbOrWaveReady;
			dragAndDropMan.PleaseUnlockThisFile += dragAndDropMan_PleaseUnlockThisFile;

			EditOpsAll = new MouseOpAbstract[] {
				new MouseOpTrimVideo(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Left, ProjectDimensionsTimelineType.Main),
				new MouseOpTrimVideo(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Right, ProjectDimensionsTimelineType.Main),
				new MouseOpTrimVideo(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Left, ProjectDimensionsTimelineType.Original),
				new MouseOpTrimVideo(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Right, ProjectDimensionsTimelineType.Original),
				new MouseOpTrimAudio(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Left, ProjectDimensionsTimelineType.Audios),
				new MouseOpTrimAudio(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Right, ProjectDimensionsTimelineType.Audios),
                new MouseOpTrimAudio(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Left, ProjectDimensionsTimelineType.Original),
				new MouseOpTrimAudio(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Right, ProjectDimensionsTimelineType.Original),
				new MouseOpMoveVideo(this, UiObjects, Dimdim, editor, videoPlayer, MetaGenerator),
				new MouseOpMoveAudio(this, UiObjects, Dimdim, editor, videoPlayer, MetaGenerator),
				new MouseOpSelectOriginalSegment(this, UiObjects, Dimdim, editor, videoPlayer),
				new MouseOpVideoEasings(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Left),
				new MouseOpVideoEasings(this, UiObjects, Dimdim, editor, videoPlayer, TrimDirection.Right),
				new MouseOpLinkVideoAudio(this, UiObjects, Dimdim, editor, videoPlayer),
			};

            Ops = new _VidkaOp[]
            {
                new ExportToAvi(this),
                exportToSegment = new ExportToAvi_Segment(this),
                new PreviewAvsSegmentInMplayer_Mp5s(this),
                new PreviewAvsSegmentInMplayer_Mp5sfff(this),
                new PreviewAvsSegmentInMplayer_Mp15s(this),
                new PreviewAvsSegmentInMplayer_Mp15sfff(this),
                new PreviewAvsSegmentInMplayer_Vd15s(this),
                new RebuildProj(this),
                new SplitCurClipVideo(this),
                new SplitCurClipVideo_DeleteLeft(this),
                new SplitCurClipVideo_DeleteRight(this),
                new SplitCurClipVideo_FinalizeLeft(this),
                new ToggleMuted(this),
                new ToggleLocked(this),
                new ToggleRenderSplitPoint(this),
                new TogglePreviewMode(this),
                new ToggleConsoleVisibility(this),
                new ShowClipUsage(this),
                new DuplicateCurClip(this),
                new DeleteCurSelectedClip(this),
                new InsertSimpleTextClip(this),
                new InsertCurrentFrameStill(this),
                new LinearShuffleByFilename(this),
                new RemoveLinkVideoAudio(this),
            };
			setProjToAllEditOps(Proj);
		}

        #region ============================= op management =============================

        public void InvokeOpByName(string name)
        {
            var op = Ops.FirstOrDefault(x => x.CommandName == name);
            if (op != null)
                op.Run();
        }

        /// <summary>
        /// Called on ANY key press
        /// </summary>
        public void KeyPressed(KeyEventArgs e)
        {
            // TODO: refactor these maybe
            if (e.Control && e.KeyCode.IsLRShiftKey())
                ControlPressed();
            if (e.Shift && e.KeyCode.IsLRShiftKey())
                ShiftPressed();
            //------- commented out because we have created menu shortcuts
            //if (e.Control && e.KeyCode == Keys.S) // these are controlled from MainForm now
            //	Logic.SaveTriggered();
            //else if (e.Control && e.KeyCode == Keys.O)
            //	Logic.OpenTriggered();
            //else if (e.Control && e.Shift && e.KeyCode == Keys.E)
            //	Logic.ExportToAvs();
            else if (e.Control && e.KeyCode == Keys.Oemplus)
                ZoomIn(shitbox.Width);
            else if (e.Control && e.KeyCode == Keys.OemMinus)
                ZoomOut(shitbox.Width);
            //else if (e.Alt && e.KeyCode == Keys.Enter)
            else if (e.KeyCode == Keys.F4)
                shitbox.OpenClipProperties(UiObjects.CurrentClip);
            else if (e.Control && e.KeyCode == Keys.Space)
                PlayPause(onlyLockedClips: true);
            else if (e.KeyCode == Keys.Space)
                PlayPause(false);
            else if (e.KeyCode == Keys.Home)
                SetFrameMarker_0_ForceRepaint();
            else if (e.KeyCode == Keys.End)
                SetFrameMarker_End_ForceRepaint();
            else if (e.KeyCode == Keys.Enter)
                EnterPressed();
            else if (e.KeyCode == Keys.Escape)
                EscapePressed();
            else if (e.Control && e.KeyCode == Keys.Z)
                Undo();
            else if (e.Control && e.KeyCode == Keys.Y)
                Redo();
            else if (e.Control && e.KeyCode == Keys.C)
                CopyCurClipToClipboard();
            else if (e.Control && e.KeyCode == Keys.X)
                CutCurClipToClipboard();
            else if (e.Control && e.KeyCode == Keys.V)
                PasteClipFromClipboard();

            // ... to fix that bug when you do split on the "outside" of the original clip and it actually splits somewhere in your project
            if (UiObjects.OriginalTimelinePlaybackMode)
                return;

            ___UiTransactionBegin();
            if (CurEditOp == null)
            {
                ActivateCorrectOp((opp) =>
                {
                    return opp.TriggerBy_KeyPress(e.KeyCode);
                });
                if (CurEditOp != null)
                    CurEditOp.KeyPressedOther(e.KeyCode);
            }
            var op = Ops.FirstOrDefault(x => x.TriggerByKeyPress(e));
            if (op != null)
                op.Run();
            ___UiTransactionEnd();
        }

        #endregion

        #region ============================= IVidkaOpContext =============================

        //TODO: move the shit here maybe?
        //CurFileName
        public PreviewThreadLauncher PreviewLauncher { get { return previewLauncher; } }

        /// <summary>
        /// should be called 1 time, otherwise we might be seeing a lot of that dialog!
        /// </summary>
        public string CheckRawDumpFolderIsOkAndGiveItToMe()
        {
            if (Directory.Exists(Settings.Default.RawVideoDumpFolder))
                return Settings.Default.RawVideoDumpFolder;
            string newDumpFolder = null;
            var yesNewValue = shitbox.ShowInputMessage(
                "Raw output dump folder",
                "Current dump folder is invalid. Hit cancel if you don't care.",
                Settings.Default.RawVideoDumpFolder,
                out newDumpFolder);
            if (yesNewValue)
            {
                Settings.Default.RawVideoDumpFolder = newDumpFolder;
                Settings.Default.Save();
            }
            return (Directory.Exists(Settings.Default.RawVideoDumpFolder))
                ? Settings.Default.RawVideoDumpFolder
                : Path.GetDirectoryName(CurFileName);
        }

        public void AppendToConsole(VidkaConsoleLogLevel level, string s)
        {
            shitbox.AppendToConsole(level, s);
        }
        public bool DialogConfirm(string title, string question)
        {
            return shitbox.ShowConfirmMessage(title, question);
        }
        public void DialogError(string title, string text)
        {
            shitbox.ShowErrorMessage(title, text);
        }

        // ............. firing simple events that affect the UI (done also by ops) ...............
        public void Fire_ProjectUpdated_AsFarAsMenusAreConcerned() {
            FireHandler(ProjectUpdated_AsFarAsMenusAreConcerned);
        }
        public void Fire_PleaseTogglePreviewMode() {
            FireHandler(PleaseTogglePreviewMode);
        }
        public void Fire_PleaseToggleConsoleVisibility() {
            FireHandler(PleaseToggleConsoleVisibility);
        }
        public void Fire_PleaseSetFormTitle(string title) {
            if (PleaseSetFormTitle != null)
                PleaseSetFormTitle(title);
        }
        public void Fire_PleaseSetPlayerAbsPosition(PreviewPlayerAbsoluteLocation location) {
            if (PleaseSetPlayerAbsPosition != null)
                PleaseSetPlayerAbsPosition(location);
        }
        public int GetHorizontalScrollBarHeight()
        {
            return shitbox.GetHorizontalScrollBarHeight();
        }

        #endregion

        #region ============================= drag-drop =============================

        // TODO: do not use global varialbles
		//private VideoMeta.VideoMetadataUseful dragMeta;

		public void MediaFileDragEnter(string[] filenames, int w)
		{
			var framesSampleQuarterScreen = (int)Dimdim.convert_AbsX2Frame(w / 4);
			dragAndDropMan.NewFilesDragged(filenames, framesSampleQuarterScreen);
			___UiTransactionBegin();
			UiObjects.SetDraggyCoordinates(
				mode: dragAndDropMan.ModeForUiDraggy,
				mouseX: mouseX,
				mouseXOffset: framesSampleQuarterScreen / 2,
				text: dragAndDropMan.DraggyText,
				frameLength: framesSampleQuarterScreen,
				hasAudio: true);
			___UiTransactionEnd();
		}

		public void MediaFileDragMove(int x)
		{
			___UiTransactionBegin();
			UiObjects.SetDraggyCoordinates(mouseX: x);
			___UiTransactionEnd();
		}

		public void MediaFileDragDrop(string[] filenames)
		{
			___UiTransactionBegin();
			// TODO: wqrfsawq: this is wrong... dragAndDropMan.Mode should eb Image/Video/Audio, whereas draggy.Mode should be just the timeline
			if (dragAndDropMan.Mode == DragAndDropManagerMode.Video ||
				dragAndDropMan.Mode == DragAndDropManagerMode.Image)
			{
				var vclips = dragAndDropMan.FinalizeDragAndMakeVideoClips();
				int draggyVideoShoveIndex = Dimdim.GetVideoClipDraggyShoveIndex(UiObjects.Draggy);
				var lastActiveVideo = UiObjects.CurrentVideoClip;
				AddUndableAction_andFireRedo(new UndoableAction
				{
					Redo = () =>
					{
						Proj.ClipsVideo.InsertRange(draggyVideoShoveIndex, vclips);
						UiObjects.SetActiveVideo(vclips.FirstOrDefault(), Proj);
					},
					Undo = () =>
					{
						Proj.ClipsVideo.RemoveRange(draggyVideoShoveIndex, vclips.Count());
						UiObjects.SetActiveVideo(lastActiveVideo, Proj);
					},
					PostAction = () =>
					{
						Proj.Compile();
						UpdateCanvasWidthFromProjAndDimdim();
					}
				});
			}
			else if (dragAndDropMan.Mode == DragAndDropManagerMode.Audio)
			{
                var frameDraggyFirst = Dimdim.convert_ScreenX2Frame(UiObjects.Draggy.MouseX - UiObjects.Draggy.MouseXOffset);
                var aclips = dragAndDropMan.FinalizeDragAndMakeAudioClips(frameDraggyFirst);
                var lastActiveVideo = UiObjects.CurrentVideoClip;
                AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        Proj.ClipsAudio.AddRange(aclips);
                        UiObjects.SetActiveAudio(aclips.FirstOrDefault());
                    },
                    Undo = () =>
                    {
                        foreach (var aclip in aclips)
                            Proj.ClipsAudio.Remove(aclip);
                        UiObjects.SetActiveVideo(lastActiveVideo, Proj);
                    },
                    PostAction = () =>
                    {
                        Proj.Compile();
                        UpdateCanvasWidthFromProjAndDimdim();
                    }
                });
			}
			else if (dragAndDropMan.Mode == DragAndDropManagerMode.Folder)
			{
				if (shitbox.ShowConfirmMessage("Recursive generation", "Generate thumbs and meta for this folder and all its subfolders? This will take a long ass while..."))
				{
					dragAndDropMan.QueueUpTheWholeFolder(dragAndDropMan.OriginalFile);
				}
                dragAndDropMan.FinalizeThisDragDropOp();
			}
            else if (dragAndDropMan.Mode == DragAndDropManagerMode.VidkaProject)
            {
                var proceed = shitbox.ShouldIProceedIfProjectChanged();
                if (!proceed)
                    return;
                LoadProjFromFile(filenames.FirstOrDefault());
                dragAndDropMan.FinalizeThisDragDropOp();
            }
			UiObjects.ClearDraggy();
			___UiTransactionEnd();
		}

		public void CancelDragDrop()
		{
			___UiTransactionBegin();
			dragAndDropMan.CancelDragDrop();
			UiObjects.ClearDraggy();
			___UiTransactionEnd();
		}

		private void dragAndDropMan_MetaReadyForDraggy(string filename, VideoMetadataUseful meta)
		{
			___UiTransactionBegin();
			var newLengthFrames = dragAndDropMan.Draggies.FirstOrDefault().LengthInFrames;
			UiObjects.SetDraggyCoordinates(
				text: "" + newLengthFrames + "\nframes",
				frameLength: newLengthFrames
			);
			___UiTransactionEnd();
		}

		private void dragAndDropMan_MetaReadyForOutstandingVideo(VidkaClipVideoAbstract vclip, VideoMetadataUseful meta)
		{
			___UiTransactionBegin();
			UpdateCanvasWidthFromProjAndDimdim();
			___UiTransactionEnd();
		}

		private void dragAndDropMan_MetaReadyForOutstandingAudio(VidkaClipAudio aclip, VideoMetadataUseful meta)
		{
			shitbox.PleaseRepaint();
		}

		private void dragAndDropMan_ThumbOrWaveReady()
		{
			shitbox.PleaseRepaint();
		}

		private void dragAndDropMan_PleaseUnlockThisFile(string filename)
		{
			shitbox.PleaseUnlockThisFile(filename);
			videoPlayer.PleaseUnlockThisFile(filename);
		}
		
		#endregion

		#region ============================= file save =============================

		public void NewProjectPlease()
		{
            var proceed = shitbox.ShouldIProceedIfProjectChanged();
            if (!proceed)
                return;
			SetProj(new VidkaProj());
			curFilename = null;
			SetFileChanged(false);
			___UiTransactionBegin();
            editActionsManager.Clear();
			UiObjects.ClearAll();
			videoPlayer.SetStillFrameNone();
            Fire_PleaseSetPlayerAbsPosition(PreviewPlayerAbsoluteLocation.TopRight);
			___UiTransactionEnd();
		}

		public void OpenTriggered()
		{
            var proceed = shitbox.ShouldIProceedIfProjectChanged();
            if (!proceed)
                return;
			var filename = shitbox.OpenProjectOpenDialog();
			if (String.IsNullOrEmpty(filename)) // still null? => user cancelled
				return;
			LoadProjFromFile(filename);
		}

		public void LoadProjFromFile(string filename)
		{
			if (!File.Exists(filename))
			{
				shitbox.ShowErrorMessage("Too much vodka?", "Do you realize the file " + Path.GetFileName(filename) + " does nota exista?");
				return;
			}

			curFilename = filename;
            Fire_PleaseSetFormTitle(curFilename);
			SetFileChanged(false);

			// load...
			var proj = ioOps.LoadProjFromFile(curFilename);
			SetProj(proj);

            Fire_ProjectUpdated_AsFarAsMenusAreConcerned();

			// update UI...
			___UiTransactionBegin();
			SetFrameMarker_0_ForceRepaint();
			UpdateCanvasWidthFromProjAndDimdim();
			___UiTransactionEnd();
		}

		public void SaveTriggered()
		{
			SaveProject(curFilename);
		}
		public void SaveAsTriggered()
		{
			SaveProject(null);
		}

		private void SetProj(VidkaProj proj)
		{
			Proj = proj;

			// init...
			Proj.Compile(); // set up filenames, etc, dunno

			// set proj to all objects who care
			Dimdim.setProj(Proj);
			dragAndDropMan.SetProj(Proj);
			setProjToAllEditOps(Proj);
		}

		private void SaveProject(string filename)
		{
			var isSavedInNewLocation = String.IsNullOrEmpty(filename);
			if (String.IsNullOrEmpty(filename))
				filename = shitbox.OpenProjectSaveDialog();
			if (String.IsNullOrEmpty(filename)) // still null? => user cancelled
				return;

			if (isSavedInNewLocation)
				VidkaIO.RebuildAuxillaryFiles(Proj, filename, MetaGenerator, true);
			ioOps.SaveProjToFile(Proj, filename);
			curFilename = filename;
			SetFileChanged(false);
		}

		#endregion

		#region ============================= object exchange =============================

		/// <summary>
		/// The project... It would be a crime for Logic class not to share it
		/// </summary>
		public VidkaProj Proj { get; private set; }
		/// <summary>
		/// Project dimensions helper class also used in the paint method
		/// </summary>
		public ProjectDimensions Dimdim { get; private set; }
		/// <summary>
		/// Hovers, selected clips, enabled clip bounds
		/// </summary>
		public VidkaUiStateObjects UiObjects { get; private set; }
		public VidkaFileMapping FileMapping { get; private set; }
		public MetaGeneratorInOtherThread MetaGenerator { get { return dragAndDropMan.MetaGenerator; } }
		public bool IsFileChanged { get; private set; }
		public string CurFileName { get {
			return curFilename;
		} }
		public string CurFileNameShort { get {
			return Path.GetFileName(curFilename);
		} }
		public string CurMediaFileName { get {
			return (UiObjects.CurrentClip != null)
				? UiObjects.CurrentClip.FileName
				: null;
		} }

		public void SetPreviewPlayer(IVideoPlayer videoPlayer)
		{
			this.videoPlayer = videoPlayer;
			previewLauncher.SetPreviewPlayer(videoPlayer);
			ShowFrameInVideoPlayer(UiObjects.CurrentMarkerFrame);
			// TODO: have a common interface maybe?
			foreach (var op in EditOpsAll) {
				op.SetVideoPlayer(videoPlayer);
			}
		}

		public void UiInitialized()
		{
			___UiTransactionBegin();
			UpdateCanvasWidthFromProjAndDimdim();
			___UiTransactionEnd();
			ShowFrameInVideoPlayer(UiObjects.CurrentMarkerFrame);
		}

		#endregion

		#region ============================= state tracking for resizing, scrolling and repaint =============================

		/// <summary>
		/// Call this at the begging of every method that potentially changes the state of UI
		/// </summary>
		private void ___UiTransactionBegin() {
			UiObjects.ClearStateChangeFlag();
			needToChangeCanvasWidth = null;
			needToChangeScrollX = null;
		}

		/// <summary>
		/// Call this at the end of every method that potentially changes the state of UI
		/// </summary>
		private void ___UiTransactionEnd() {
			if (needToChangeCanvasWidth.HasValue) {
				shitbox.UpdateCanvasWidth(needToChangeCanvasWidth.Value);
				___Ui_stateChanged();
			}
			if (needToChangeScrollX.HasValue) {
				shitbox.UpdateCanvasHorizontalScroll(needToChangeScrollX.Value);
				___Ui_stateChanged();
			}
			if (UiObjects.DidSomethingChange())
				shitbox.PleaseRepaint();
			if (UiObjects.DidSomethingChange_originalTimeline())
                Fire_PleaseSetPlayerAbsPosition((UiObjects.CurrentClip != null)
					? PreviewPlayerAbsoluteLocation.BottomRight
					: PreviewPlayerAbsoluteLocation.TopRight);
		}

		/// <summary>
		/// Call this b/w _begin and _end to force repaint
		/// </summary>
		private void ___Ui_stateChanged() {
			UiObjects.UiStateChanged();
		}

		/// <summary>
		/// Call this to update scrollX (forces repaint)
		/// </summary>
		private void ___Ui_updateScrollX(int scrollX) {
			needToChangeScrollX = scrollX;
		}

		/// <summary>
		/// Call this to update canvas width (forces repaint)
		/// </summary>
		private void ___Ui_updateCanvasWidth(int w) {
			needToChangeCanvasWidth = w;
		}

		#endregion

		#region ============================= mouse tracking =============================

		/// <param name="h">Height of the canvas</param>
		/// <param name="w">Width of the canvas</param>
		public void MouseMoved(int x, int y, int w, int h)
		{
			___UiTransactionBegin();
			mouseX = x;
			var timeline = Dimdim.collision_whatTimeline(y, h);
			UiObjects.SetTimelineHover(timeline);
			switch (timeline) {
				case ProjectDimensionsTimelineType.Main:
					var clip = Dimdim.collision_main(x);
					UiObjects.SetHoverVideo(clip);
					CheckClipTrimCollision(x);
                    CheckClipEasingCollision_mainTimeline(x, y, clip);
                    break;
                case ProjectDimensionsTimelineType.MainEases:
                    var clipEase = Dimdim.collision_mainEases(x);
                    UiObjects.SetHoverVideo(clipEase);
                    CheckClipEasingCollision_easesTimeline(x, y, clipEase);
                    break;
				case ProjectDimensionsTimelineType.Original:
					if (UiObjects.CurrentClip == null)
						break;
					var hoverClip = Dimdim.collision_original_all(x, w, UiObjects.CurClipAllUsagesVideoAndAudio);
					UiObjects.SetHoverGeneric(hoverClip, Proj);
					CheckClipTrimCollision(x);
					break;
				case ProjectDimensionsTimelineType.Audios:
					var aclip = Dimdim.collision_audio(x);
					UiObjects.SetHoverAudio(aclip);
					CheckClipTrimCollision(x);
					break;
				default:
					UiObjects.SetHoverVideo(null);
					UiObjects.SetHoverAudio(null);
					UiObjects.SetTrimHover(TrimDirection.None);
					break;
			}
			//cxzxc("t-hvr:" + UiObjects.TrimHover.ToString() + ",clip:" + UiObjects.CurrentVideoClipHover.cxzxc());
			___UiTransactionEnd();
		}

		public void MouseLeave()
		{
			___UiTransactionBegin();
			UiObjects.SetTimelineHover(ProjectDimensionsTimelineType.None);
			UiObjects.SetHoverVideo(null);
			___UiTransactionEnd();
		}

		//------------------------ helpers -------------------------------

		/// <summary>
		/// Check trim mouse collision and set TrimHover in UiObjects.
		/// recycled lastCollision_x1 and lastCollision_x2 are used.
		/// </summary>
		private void CheckClipTrimCollision(int x)
		{
			if (!Dimdim.lastCollision_succeeded)
			{
				UiObjects.SetTrimHover(TrimDirection.None);
				return;
			}
			var boundThres = BOUND_THRESH_MAX;
			var blockWidth = Dimdim.lastCollision_x2 - Dimdim.lastCollision_x1;
			if (blockWidth < 4 * BOUND_THRESH_MAX)
				boundThres = blockWidth / 4;
			if (x - Dimdim.lastCollision_x1 <= boundThres)
				UiObjects.SetTrimHover(TrimDirection.Left);
			else if (Dimdim.lastCollision_x2 - x <= boundThres)
				UiObjects.SetTrimHover(TrimDirection.Right);
			else
				UiObjects.SetTrimHover(TrimDirection.None);
			UiObjects.SetTrimThreshPixels(boundThres);
		}

        private void CheckClipEasingCollision_mainTimeline(int x, int y, VidkaClipVideoAbstract clip)
        {
            UiObjects.SetShowEasingHandles(false);
            if (clip == null || UiObjects.TrimHover == TrimDirection.None)
                return;
            if (UiObjects.TrimHover == TrimDirection.Left && clip.EasingLeft > 0)
                return;
            if (UiObjects.TrimHover == TrimDirection.Right && clip.EasingRight > 0)
                return;
            var y1 = Dimdim.lastCollision_y1;
            var y2 = Dimdim.lastCollision_y2;
            var yEasingThresh = Math.Max(y2 - BOUND_EASING_BOTTOM_PIXELS, (y2 - y1) / 2);
            if (y >= yEasingThresh)
                UiObjects.SetShowEasingHandles(true);
        }

        private void CheckClipEasingCollision_easesTimeline(int x, int y, VidkaClipVideoAbstract clip)
        {
            UiObjects.SetTrimHover(TrimDirection.None);
            UiObjects.SetShowEasingHandles(false);
            if (clip == null)
                return;
            var boundThres = BOUND_THRESH_MAX;
            if (Dimdim.lastCollision_easeSide == TrimDirection.Left)
            {
                if (x - Dimdim.lastCollision_x1 <= boundThres)
                {
                    UiObjects.SetTrimHover(TrimDirection.Left);
                    UiObjects.SetShowEasingHandles(true);
                }
            }
            else if (Dimdim.lastCollision_easeSide == TrimDirection.Right)
            {
                if (Dimdim.lastCollision_x2 - x <= boundThres)
                {
                    UiObjects.SetTrimHover(TrimDirection.Right);
                    UiObjects.SetShowEasingHandles(true);
                }
            }
        }

		#endregion

		#region ============================= frame of view (scroll/zoom) =============================

		/// <summary>
		/// Called by VideoShitbox when user scrolls with scrollbar or mousewheel
		/// </summary>
		public void setScrollX(int x)
		{
			Dimdim.setScroll(x);
		}

		public void ZoomIn(int width)
		{
			___UiTransactionBegin();
			//Dimdim.ZoomIn(mouseX); // I decided not to zoom into the mouse... too unstable
			Dimdim.ZoomIn(Dimdim.convert_Frame2ScreenX(UiObjects.CurrentMarkerFrame), width);
			UpdateCanvasWidthFromProjAndDimdim();
			UpdateCanvasScrollXFromDimdim();
			___UiTransactionEnd();
		}
		/// <summary>
		/// width parameter is needed here to prevent user from zooming out too much
		/// </summary>
		public void ZoomOut(int width)
		{
			___UiTransactionBegin();
			Dimdim.ZoomOut(mouseX, width);
			UpdateCanvasWidthFromProjAndDimdim();
			UpdateCanvasScrollXFromDimdim();
			___UiTransactionEnd();
		}

		/// <summary>
		/// Call this in ALL spots where proj length is subject to change
		/// </summary>
		public void UpdateCanvasWidthFromProjAndDimdim() {
			var widthNeedsToBeSet = Dimdim.getTotalWidthPixelsForceRecalc();
			___Ui_updateCanvasWidth(widthNeedsToBeSet);
		}

		/// <summary>
		/// Call this in ALL spots where scrollx is subject to change
		/// </summary>
		private void UpdateCanvasScrollXFromDimdim() {
			var scrollx = Dimdim.getCurrentScrollX();
			___Ui_updateScrollX(scrollx);
		}

		#region ============================= marker =============================

		// TODO: the code below does not use UiObjects.ClearStateChangeFlag()
		// - Mon, June 15, 2015

		/// <summary>
		/// Used during playback for animation of the marker (or cursor, if u like...)
		/// </summary>
		public void SetFrameMarker_ForceRepaint(long frame)
		{
			___UiTransactionBegin();
			UiObjects.SetCurrentMarkerFrame(frame);
			updateFrameOfViewFromMarker();
			___UiTransactionEnd();
		}

		/// <summary>
		/// Used when HOME key is pressed
		/// </summary>
		public void SetFrameMarker_0_ForceRepaint()
		{
			___UiTransactionBegin();
			SetFrameMarker_ShowFrameInPlayer(0);
			___UiTransactionEnd();
		}

		public void SetFrameMarker_End_ForceRepaint()
		{
			___UiTransactionBegin();
			var frameLastClip = Proj.GetTotalLengthOfVideoClipsFrame();
			SetFrameMarker_ShowFrameInPlayer(frameLastClip);
			___UiTransactionEnd();
		}

		/// <summary>
		/// Used from within this class, on mouse press, when arrow keys are pressed,
		/// by drag ops and other ops (e.g. or when a clip is deleted)
		/// </summary>
		public long SetFrameMarker_ShowFrameInPlayer(long frame)
		{
			printFrameToConsole(frame);
			UiObjects.SetCurrentMarkerFrame(frame);
			updateFrameOfViewFromMarker();
			ShowFrameInVideoPlayer(UiObjects.CurrentMarkerFrame); // one more thing... unrelated... update the doggamn WMP
			return frame;
		}

		private void printFrameToConsole(long frame) {
			var sec = Proj.FrameToSec(frame);
			var secFloor = (long)sec;
			var secFloorFrame = Proj.SecToFrame(secFloor);
			var frameRemainder = frame - secFloorFrame;
			var timeSpan = TimeSpan.FromSeconds(secFloor);
			cxzxc(String.Format("frame={0} ({1}.{2})"
				, frame
				, timeSpan.ToTsString_MinuteOrHour()
				, frameRemainder));
		}

		private void updateFrameOfViewFromMarker()
		{
			if (UiObjects.OriginalTimelinePlaybackMode)
				return;

			//var frame = UiObjects.CurrentMarkerFrame;
			var screenX = Dimdim.convert_Frame2ScreenX(UiObjects.CurrentMarkerFrame);
			var absX = Dimdim.convert_FrameToAbsX(UiObjects.CurrentMarkerFrame);
			if (screenX < 0)
			{
				// screen jumps back
				int scrollX = absX - shitbox.Width + SCREEN_MARKER_JUMP_LEEWAY;
				if (scrollX < 0)
					scrollX = 0;
				Dimdim.setScroll(scrollX);
				___Ui_updateScrollX(scrollX);
			}
			else if (screenX >= shitbox.Width)
			{
				// screen jumps forward
				int scrollX = absX - SCREEN_MARKER_JUMP_LEEWAY;
				var maxScrollValue = Dimdim.getTotalWidthPixels() - shitbox.Width;
				if (scrollX > maxScrollValue)
					scrollX = maxScrollValue;
				Dimdim.setScroll(scrollX);
				___Ui_updateScrollX(scrollX);
			}
		}

		/// <summary>
		/// This is a marker-related function, so we keep it in the marker region
		/// </summary>
		private long setCurFrameMarkerPositionToNextOrPrevClip(Keys keyData)
		{
			long frameOffset = 0;
			var curClipIndex = Proj.GetVideoClipIndexAtFrame_forceOnLastClip(UiObjects.CurrentMarkerFrame, out frameOffset);
			if (curClipIndex == -1)
				return SetFrameMarker_ShowFrameInPlayer(0);
			var clip = Proj.ClipsVideo[curClipIndex];
			var framesToStartOfClip = frameOffset - clip.FrameStartNoEase;
			if (keyData == Keys.Left)
			{
				frameOffset = 0;
				if (framesToStartOfClip > 0) // special case: go to beginning of this clip
					clip = Proj.ClipsVideo[curClipIndex];
				else if (curClipIndex > 0)
					clip = Proj.ClipsVideo[curClipIndex-1];
			}
			else if (keyData == Keys.Right)
			{
				frameOffset = 0;
				if (curClipIndex < Proj.ClipsVideo.Count - 1)
					clip = Proj.ClipsVideo[curClipIndex + 1];
				else
					frameOffset = clip.LengthFrameCalc;
			}
			var frameAbs = Proj.GetVideoClipAbsFramePositionLeft(clip);
			UiObjects.SetActiveVideo(clip, Proj);
			UiObjects.SetHoverVideo(null);
			SetFrameMarker_ShowFrameInPlayer(frameAbs + frameOffset);
			return 0;
		}

		public void SetCurrentVideoClip_ForceRepaint(VidkaClipVideoAbstract clip)
		{
			___UiTransactionBegin();
            if (Proj.ClipsVideo.Contains(clip))
            {
			    UiObjects.SetActiveVideo(clip, Proj);
			    UiObjects.SetHoverVideo(null);
            }
			___UiTransactionEnd();
		}

		#endregion

		#endregion frome of view

		#region ============================= Playback/Feedback on WMP =============================

		public void PlayPause(bool onlyLockedClips=false)
		{
			if (!previewLauncher.IsPlaying)
			{
				if (UiObjects.OriginalTimelinePlaybackMode)
				{
                    var projFullClip = SetupTmpProjForOriginalPlayback();
					previewLauncher.StartPreviewPlayback(projFullClip, UiObjects.CurrentMarkerFrame, false);
				}
				else
					previewLauncher.StartPreviewPlayback(Proj, UiObjects.CurrentMarkerFrame, onlyLockedClips);
			}
			else
				previewLauncher.StopPlayback();
		}

        public void StopAllPlayback()
        {
            previewLauncher.StopPlayback();
        }

		/// <summary>
		/// Navigate to that frame in the damn AVI file and pause the damn WMP
		/// </summary>
		public void ShowFrameInVideoPlayer(long frame)
		{
			if (previewLauncher.IsPlaying)
				previewLauncher.StopPlayback();
			long frameOffset;
			var clipIndex = Proj.GetVideoClipIndexAtFrame(frame, out frameOffset);
			var secOffset = Proj.FrameToSec(frameOffset);
			if (clipIndex == -1) {
				videoPlayer.SetStillFrameNone();
			}
			else {
				var clip = Proj.ClipsVideo[clipIndex];
				videoPlayer.SetStillFrame(clip.FileName, secOffset);
				//cxzxc("preview1:" + secOffset);
			}
		}

        #region ---------------------- Helpers -----------------------------
        
        private VidkaProj SetupTmpProjForOriginalPlayback()
        {
            Proj_forOriginalPlayback.ClipsAudio.Clear();
            Proj_forOriginalPlayback.ClipsVideo.Clear();
            if (UiObjects.CurrentVideoClip != null)
            {
                var clipFull = UiObjects.CurrentVideoClip.MakeCopy_VideoClip();
                clipFull.FrameStart = 0;
                clipFull.FrameEnd = UiObjects.CurrentClip.FileLengthFrames;
                Proj_forOriginalPlayback.ClipsVideo.Add(clipFull);
            }
            else if (UiObjects.CurrentAudioClip != null)
            {
                var clipFull = UiObjects.CurrentAudioClip.MakeCopy_AudioClip();
                Proj_forOriginalPlayback.ClipsVideo.Add(new VidkaClipVideo
                {
                    FileName = UiObjects.CurrentAudioClip.FileName,
                    FrameStart = 0,
                    FrameEnd = UiObjects.CurrentClip.FileLengthFrames,
                });
            }
            return Proj_forOriginalPlayback;
        }

        #endregion
        
        #endregion

        #region ============================= editing =============================

        #region ---------------------- UNDO/REDO -----------------------------
        
        public void Redo()
		{
            if (!editActionsManager.HasRedo)
				return;
			if (previewLauncher.IsPlaying)
			{
				cxzxc("Undo/redo disabled during playback to avoid whoopsie-doodles!");
				return;
			}
			___UiTransactionBegin();
            editActionsManager.Redo();
			SetFileChanged(editActionsManager.IsChanged);
            ___Ui_stateChanged();
			___UiTransactionEnd();
		}
		public void Undo()
		{
            if (!editActionsManager.HasUndo)
				return;
			if (previewLauncher.IsPlaying)
			{
				cxzxc("Undo/redo disabled during playback to avoid whoopsie-doodles!");
				return;
			}

			___UiTransactionBegin();
            editActionsManager.Undo();
            SetFileChanged(editActionsManager.IsChanged);
            ___Ui_stateChanged();
			___UiTransactionEnd();
		}
		public void AddUndableAction_andFireRedo(UndoableAction action)
		{
            if (editActionsManager.HasRedo)
                cxzxc("----------");

			___UiTransactionBegin();
            editActionsManager.AddUndableAction_andFireRedo(action);
			SetFileChanged(editActionsManager.IsChanged);
			___Ui_stateChanged();
			___UiTransactionEnd();
		}

		private void SetFileChanged(bool changed)
		{
			IsFileChanged = changed;
            Fire_PleaseSetFormTitle((curFilename ?? "Untitled") + (changed ? " *" : ""));
		}

		#endregion
		
		#region ---------------------- mouse dragging operations -----------------------------

		public void MouseDragStart(MouseButtons button, int x, int y, int w, int h)
		{
			mouseX = x; // prob not needed, since it is always set in mouseMove, but whatever
			
			___UiTransactionBegin();
			if (CurEditOp == null || CurEditOp.DoesNewMouseDragCancelMe)
			{
				bool anotherClipWasSetFromOriginalTimeline = false;

				// unless we have an active op that requests this drag action,
				// use the mouse press to calculate click collision
				var timeline = Dimdim.collision_whatTimeline(y, h);
				UiObjects.SetTimelineHover(timeline);
				switch (timeline) {
					case ProjectDimensionsTimelineType.Main:
						var clip = Dimdim.collision_main(x);
						UiObjects.SetActiveVideo(clip, Proj);
						break;
                    case ProjectDimensionsTimelineType.MainEases:
                        var clipEase = Dimdim.collision_mainEases(x);
                        UiObjects.SetActiveVideo(clipEase, Proj);
                        break;
					case ProjectDimensionsTimelineType.Original:
						if (UiObjects.CurrentVideoClipHover != null && UiObjects.CurrentVideoClipHover != UiObjects.CurrentClip)
						{
							UiObjects.SetActiveVideo(UiObjects.CurrentVideoClipHover, Proj);
							SetFrameMarker_ShowFrameInPlayer(UiObjects.CurrentClipFrameAbsPos ?? UiObjects.CurrentMarkerFrame);
							anotherClipWasSetFromOriginalTimeline = true;
						}
                        if (UiObjects.CurrentAudioClipHover != null && UiObjects.CurrentAudioClipHover != UiObjects.CurrentClip)
                        {
                            UiObjects.SetActiveAudio(UiObjects.CurrentAudioClipHover);
                            SetFrameMarker_ShowFrameInPlayer(UiObjects.CurrentClipFrameAbsPos ?? UiObjects.CurrentMarkerFrame);
                            anotherClipWasSetFromOriginalTimeline = true;
                        }
						break;
					case ProjectDimensionsTimelineType.Audios:
						var aclip = Dimdim.collision_audio(x);
						UiObjects.SetActiveAudio(aclip);
						break;
					default:
						UiObjects.SetActiveVideo(null, Proj);
						break;
				}

				ActivateCorrectOp((op) => {
					return op.TriggerBy_MouseDragStart(button, x, y);
				});

				// update current frame marker on left click press
				if (button == MouseButtons.Left && !previewLauncher.IsPlaying && !anotherClipWasSetFromOriginalTimeline)
				{
					if (timeline == ProjectDimensionsTimelineType.Original && UiObjects.CurrentClip != null)
					{
						var clip = UiObjects.CurrentClip;
						var cursorFrame = Dimdim.convert_ScreenX2Frame_OriginalTimeline(x, clip.FileLengthFrames, w);
						if (cursorFrame < 0)
							cursorFrame = 0;
                        if (cursorFrame >= clip.FrameStartNoEase && cursorFrame < clip.FrameEndNoEase)
                        {
							UiObjects.SetOriginalTimelinePlaybackMode(false);
							SetFrameMarker_ShowFrameInPlayer(cursorFrame + (UiObjects.CurrentClipFrameAbsPos ?? 0) - UiObjects.CurrentClip.FrameStartNoEase);
						}
						else {
							// we are outside the clip bounds on the original timeline,
							// so I assume user wants to view some external segment on original
							// and I will switch to OriginalTimelinePlayback
							printFrameToConsole(cursorFrame);
							UiObjects.SetOriginalTimelinePlaybackMode(true);
							UiObjects.SetCurrentMarkerFrame(cursorFrame);
							// show in video player
							var secOffset = Proj.FrameToSec(cursorFrame);
							videoPlayer.SetStillFrame(clip.FileName, secOffset);
						}
					}
					else
					{
						UiObjects.SetOriginalTimelinePlaybackMode(false);
						var cursorFrame = Dimdim.convert_ScreenX2Frame(x);
						if (cursorFrame < 0)
							cursorFrame = 0;
						SetFrameMarker_ShowFrameInPlayer(cursorFrame);
					}
						// ? (UiObjects.CurrentClipFrameAbsPos ?? 0) - UiObjects.CurrentClip.FrameStart + UiObjects.CurrentClip.FileLengthFrames * x / w
						// : Dimdim.convert_ScreenX2Frame(x);
					// NOTE: if you want for negative frames to show original clip's thumb in player, remove this first  
				}
			}
			if (CurEditOp != null)
				CurEditOp.MouseDragStart(x, y, w, h);
			___UiTransactionEnd();
		}

		/// <param name="deltaX">relative to where the mouse was pressed down</param>
		/// <param name="deltaY">relative to where the mouse was pressed down</param>
		public void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h)
		{
			___UiTransactionBegin();
			if (CurEditOp != null)
				CurEditOp.MouseDragged(x, y, deltaX, deltaY, w, h);
			___UiTransactionEnd();
		}

		/// <param name="deltaX">relative to where the mouse was pressed down</param>
		/// <param name="deltaY">relative to where the mouse was pressed down</param>
		public void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
			___UiTransactionBegin();
			if (CurEditOp != null)
			{
				CurEditOp.MouseDragEnd(x, y, deltaX, deltaY, w, h);
				if (CurEditOp.IsDone) {
					CapitulateCurOp();
				}
			}
			___UiTransactionEnd();
		}

		public void EnterPressed()
		{
			if (CurEditOp == null)
				return;
			___UiTransactionBegin();
			CurEditOp.EnterPressed();
			if (CurEditOp.IsDone)
				CapitulateCurOp();
			___UiTransactionEnd();
		}

		public void EscapePressed()
		{
			if (CurEditOp == null)
				return;
			___UiTransactionBegin();
			CapitulateCurOp();
			___UiTransactionEnd();
		}

		public void LeftRightArrowKeys(Keys keyData)
		{
			___UiTransactionBegin();
			if (keyData == Keys.Left || keyData == Keys.Right)
			{
				CapitulateCurOp();
				setCurFrameMarkerPositionToNextOrPrevClip(keyData);
			}
			else
			{
				var frameDelta = ArrowKey2FrameDelta(keyData);
				if (CurEditOp != null)
				{
					CurEditOp.KeyPressedArrow(keyData);
					if (frameDelta != 0)
						CurEditOp.ApplyFrameDelta(frameDelta);
				}
				else if (frameDelta != 0)
					SetFrameMarker_ShowFrameInPlayer(UiObjects.CurrentMarkerFrame + frameDelta);
			}
			___UiTransactionEnd();
		}

		public void ControlPressed()
		{
			if (CurEditOp == null)
				return;
			___UiTransactionBegin();
			CurEditOp.ControlPressed();
			___UiTransactionEnd();
		}

		public void ShiftPressed()
		{
			if (CurEditOp == null)
				return;
			___UiTransactionBegin();
			CurEditOp.ShiftPressed();
			___UiTransactionEnd();
		}

		#endregion

		#region ---------------------- misc operations -----------------------------

        public void RenderSegment(int index)
        {
            exportToSegment.RenderOneSegment(index);
        }

		public void deleteAllNonlockedClips()
		{
			var oldClips = Proj.ClipsVideo;
			var newClips = Proj.ClipsVideo.Where(x => x.IsLocked).ToList();
			AddUndableAction_andFireRedo(new UndoableAction
			{
				Redo = () =>
				{
					cxzxc("Delete all non-locked clips");
					Proj.ClipsVideo = newClips;
				},
				Undo = () =>
				{
					cxzxc("UNDO Delete all non-locked clips");
					Proj.ClipsVideo = oldClips;
				},
				PostAction = () =>
				{
					Proj.Compile();
					UpdateCanvasWidthFromProjAndDimdim();
				}
			});
		}

		#endregion

		#region ----------------- helpers ------------------------------------

        public void cxzxc(string text) {
            AppendToConsole(VidkaConsoleLogLevel.Debug, text);
		}
        public void iiii(string text) {
            AppendToConsole(VidkaConsoleLogLevel.Info, text);
		}
        public void eeee(string text) {
            AppendToConsole(VidkaConsoleLogLevel.Error, text);
		}

		/// <summary>
		/// Calls setProj for all our EditOps. Call whenever Proj gets reassigned to
		/// </summary>
		private void setProjToAllEditOps(VidkaProj Proj)
		{
			foreach (var op in EditOpsAll)
				op.setProj(Proj);
		}

		/// <summary>
		/// Reset gears to neutral... :P
		/// </summary>
		private void CapitulateCurOp()
		{
			if (CurEditOp == null)
				return;
			CurEditOp.EndOperation();
			CurEditOp = null;
            if (Debug_outputEditOpLifecycle)
			    iiii("Edit mode: none");
		}

		/// <summary>
		/// returns 1, -1, MANY_FRAMES_STEP, -MANY_FRAMES_STEP
		/// </summary>
		private long ArrowKey2FrameDelta(Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.Left))
				return -1;
			else if (keyData == (Keys.Control | Keys.Right))
				return 1;
			//else if (keyData == (Keys.Alt | Keys.Left)) // like virtualDub :)
			else if (keyData == (Keys.Shift | Keys.Left))
				return -MANY_FRAMES_STEP;
			//else if (keyData == (Keys.Alt | Keys.Right)) // like virtualDub :)
			else if (keyData == (Keys.Shift | Keys.Right))
				return MANY_FRAMES_STEP;
			return 0;
		}

		private void ActivateCorrectOp(
			Func<MouseOpAbstract, bool> trigger
			//, Action<EditOperationAbstract> init
			)
		{
			CurEditOp = EditOpsAll.FirstOrDefault(op => trigger(op));
            if (CurEditOp != null)
            {
				CurEditOp.Init();
                if (Debug_outputEditOpLifecycle)
                    iiii("Edit op: " + CurEditOp.Description);
            }
		}

        private void AddUndoableAction_insertAudioClipAtMarkerPosition(VidkaClipAudio newClip)
        {
            newClip.FrameOffset = UiObjects.CurrentMarkerFrame;
            AddUndableAction_andFireRedo(new UndoableAction
            {
                Redo = () =>
                {
                    Proj.ClipsAudio.Add(newClip);
                    UiObjects.SetActiveAudio(newClip);
                },
                Undo = () =>
                {
                    Proj.ClipsAudio.Remove(newClip);
                },
            });
        }

		#endregion

		#endregion

		#region ============================= misc operations =============================
		
		public void ShowWhereTheClipIsUsed()
		{
			if (UiObjects.CurrentClip == null) {
				cxzxc("Nothing selected!");
				return;
			}
			___UiTransactionBegin();
			UiObjects.PleaseShowAllUsages(Proj);
			___UiTransactionEnd();
		}

        public void checkForErrors()
        {
            foreach (var vclip in Proj.ClipsVideo)
            {
                if (!File.Exists(vclip.FileName))
                    eeee(String.Format("File does not exist! ({0})", vclip.FileName));
            }
            foreach (var vclip in Proj.ClipsAudio)
            {
                if (!File.Exists(vclip.FileName))
                    eeee(String.Format("File does not exist! ({0})", vclip.FileName));
            }
        }

		#endregion

        #region ============================= copy + paste and adding/removing clips =============================

        public void CopyCurClipToClipboard()
		{
			if (UiObjects.CurrentVideoClip != null)
			{
				var copy = UiObjects.CurrentVideoClip;
				Clipboard.Clear();
				Clipboard.SetData(VidkaClipboardHolderFormat, new ClipboardObjectHolder
				{
					Type = ClipboardObjectType.VideoClip,
					VideoClip = copy,
				});
			}
            else if (UiObjects.CurrentAudioClip != null)
            {
                var copy = UiObjects.CurrentAudioClip;
                Clipboard.Clear();
                Clipboard.SetData(VidkaClipboardHolderFormat, new ClipboardObjectHolder
                {
                    Type = ClipboardObjectType.AudioClip,
                    AudioClip = copy,
                });
            }
		}
		public void CutCurClipToClipboard()
		{
            CopyCurClipToClipboard();
            InvokeOpByName(DeleteCurSelectedClip.Name);
		}
		public void PasteClipFromClipboard()
		{
			if (!Clipboard.ContainsData(VidkaClipboardHolderFormat))
				return;
			var obj = Clipboard.GetData(VidkaClipboardHolderFormat);
			if (obj == null)
				return;
			if (!(obj is ClipboardObjectHolder))
				return;
			var holder = (ClipboardObjectHolder)obj;
			if (holder.Type == ClipboardObjectType.VideoClip && holder.VideoClip != null)
			{
				this.AddUndoableAction_insertClipAtMarkerPosition(holder.VideoClip);
			}
            if (holder.Type == ClipboardObjectType.AudioClip && holder.AudioClip != null)
            {
                AddUndoableAction_insertAudioClipAtMarkerPosition(holder.AudioClip);
            }
		}

		public void ReplaceClip(VidkaClip clip, VidkaClip newClip)
		{
			if (Proj.ClipsVideo.Contains(clip))
			{
				var vclip = (VidkaClipVideoAbstract)clip;
				var vclip2 = (VidkaClipVideoAbstract)newClip;
				AddUndableAction_andFireRedo(new UndoableAction
				{
					Redo = () =>
					{
						if (Proj.ClipsVideo.ReplaceElement(vclip, vclip2))
							UiObjects.SetActiveVideo(vclip2, Proj);
					},
					Undo = () =>
					{
						if (Proj.ClipsVideo.ReplaceElement(vclip2, vclip))
							UiObjects.SetActiveVideo(vclip, Proj);
					},
					PostAction = () =>
					{
						if (UiObjects.CurrentVideoClip is VidkaClipTextSimple)
							VidkaIO.RebuildAuxillaryFile_SimpleText((VidkaClipTextSimple)UiObjects.CurrentVideoClip, Proj, MetaGenerator);
                        if (vclip.IsRenderBreakupPoint != vclip2.IsRenderBreakupPoint)
                            Fire_ProjectUpdated_AsFarAsMenusAreConcerned();
					}
				});
			}
            else if (Proj.ClipsAudio.Contains(clip))
            {
                var aclip = (VidkaClipAudio)clip;
                var aclip2 = (VidkaClipAudio)newClip;
                AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        if (Proj.ClipsAudio.ReplaceElement(aclip, aclip2))
                            UiObjects.SetActiveAudio(aclip2);
                    },
                    Undo = () =>
                    {
                        if (Proj.ClipsAudio.ReplaceElement(aclip2, aclip))
                            UiObjects.SetActiveAudio(aclip);
                    },
                });
            }
			//TODO: audio..
		}

		#endregion
    }
}
