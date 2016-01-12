using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.Model;

namespace Vidka.Core
{
	/// <summary>
	/// This class serves 2 functions:
	/// 1 - keeps all interactive UI objects in one place.
	/// 2 - provides easier management of when a state changes
	///	 to help trigger repaint only when neccesary.
	/// </summary>
	public class VidkaUiStateObjects
	{
		private bool stateChanged;
		private bool originalTimelineSelectionChanged; // used to change player position when console is hidden and player floats around

		private readonly IEnumerable<VidkaClipVideoAbstract> EmptyCollection_ClipsVideo;
		private readonly IEnumerable<VidkaClipAudio> EmptyCollection_ClipsAudio;
		private readonly VidkaClipVideoAbstract[] ArrayOfOne_ClipsVideo;
		private readonly VidkaClipAudio[] ArrayOfOne_ClipsAudio;

		// settable properties
		public ProjectDimensionsTimelineType TimelineHover { get; private set; }
		public VidkaClipVideoAbstract CurrentVideoClip { get; private set; }
		public VidkaClipAudio CurrentAudioClip { get; private set; }
		public VidkaClipVideoAbstract CurrentVideoClipHover { get; private set; }
		public VidkaClipAudio CurrentAudioClipHover { get; private set; }
		public long? CurrentClipFrameAbsPos { get; private set; }
		public TrimDirection TrimHover { get; private set; }
		public int TrimThreshPixels { get; set; }
		public IEnumerable<VidkaClipVideoAbstract> CurClipAllUsagesVideo { get; set; }
		public IEnumerable<VidkaClipAudio> CurClipAllUsagesAudio { get; set; }
		public long CurrentMarkerFrame { get; private set; }
		public bool OriginalTimelinePlaybackMode { get; private set; }
		public long MouseDragFrameDelta { get; private set; }
        public bool MouseDragFrameDeltaMTO { get; private set; } // MTO = Main Timeline Only
        public bool ShowEasingHandles { get; set; }
        public EditorDraggy Draggy { get; private set; }
		
		// additional helpers
		public VidkaClip CurrentClip { get {
			return (VidkaClip)CurrentVideoClip ?? (VidkaClip)CurrentAudioClip;
		} }
        public bool CurrentClipIsVideo { get {
			return CurrentVideoClip != null;
		} }
		public IEnumerable<VidkaClip> CurClipAllUsagesVideoAndAudio { get {
			return CurClipAllUsagesVideo.Any()
				? CurClipAllUsagesVideo.Cast<VidkaClip>()
				: CurClipAllUsagesAudio.Cast<VidkaClip>();
		} }
		

		public VidkaUiStateObjects()
        {
			EmptyCollection_ClipsVideo = new VidkaClipVideoAbstract[] { };
			EmptyCollection_ClipsAudio = new VidkaClipAudio[] { };
			ArrayOfOne_ClipsVideo = new VidkaClipVideoAbstract[] { null };
			ArrayOfOne_ClipsAudio = new VidkaClipAudio[] { null };

			// all the above should be null by default... but what the hell
			TimelineHover = ProjectDimensionsTimelineType.None;
			CurrentVideoClip = null;
			CurrentAudioClip = null;
			CurrentVideoClipHover = null;
			CurrentAudioClipHover = null;
			TrimHover = TrimDirection.None;
            ShowEasingHandles = false;
			CurrentMarkerFrame = 0;
			MouseDragFrameDelta = 0;
            MouseDragFrameDeltaMTO = false;
			Draggy = new EditorDraggy();
		}

		#region state change management

		/// <summary>
		/// Call this before every serious interaction method. Then do some shit.
		/// Then call DidSomethingChange() to see if you need to repaint. 
		/// </summary>
		public void ClearStateChangeFlag() {
			stateChanged = false;
			originalTimelineSelectionChanged = false;
		}
		/// <summary>
		/// Call if a repaint should be forced anyway, regardless
		/// </summary>
		public void UiStateChanged() {
			stateChanged = true;
		}
		/// <summary>
		/// Call this at the end of every serious interaction method.
		/// if this returns true, then you probably need to repaint. 
		/// </summary>
		public bool DidSomethingChange() {
			return stateChanged;
		}

		/// <summary>
		/// Used to change player position when console is hidden and player floats around
		/// </summary>
		public bool DidSomethingChange_originalTimeline() {
			return originalTimelineSelectionChanged;
		}

		#endregion

		#region functions that change shit

		public void SetTimelineHover(ProjectDimensionsTimelineType hover)
		{
			if (TimelineHover != hover)
				stateChanged = true;
			TimelineHover = hover;
		}

		/// <summary>
		/// There can only be one hover b/w video and audio line, so audio will be set to null
		/// </summary>
		public void SetHoverVideo(VidkaClipVideoAbstract hover) 
		{
			if (CurrentVideoClipHover != hover ||
				CurrentAudioClipHover != null)
				stateChanged = true;
			CurrentVideoClipHover = hover;
			CurrentAudioClipHover = null;
		}

		/// <summary>
		/// There can only be one hover b/w video and audio line, so video will be set to null
		/// </summary>
		public void SetHoverAudio(VidkaClipAudio hover)
		{
			if (CurrentAudioClipHover != hover ||
				CurrentVideoClipHover != null)
				stateChanged = true;
			CurrentAudioClipHover = hover;
			CurrentVideoClipHover = null;
		}

		/// <summary>
		/// Uses proj.ClipsAudio.Contains(hoverClip) to determine which to set HoverAudio or HoverVideo
		/// </summary>
		public void SetHoverGeneric(VidkaClip hoverClip, VidkaProj proj)
		{
			if (proj.ClipsAudio.Contains(hoverClip))
				SetHoverAudio((VidkaClipAudio)hoverClip);
			else // video!!!
				SetHoverVideo((VidkaClipVideoAbstract)hoverClip);
		}

		/// <summary>
		/// There can only be one selected (active) b/w video and audio line, so audio will be set to null
		/// Needs proj to find absolute frame position (CurrentClipFrameAbsPos)
		/// </summary>
		public void SetActiveVideo(VidkaClipVideoAbstract active, VidkaProj proj)
		{
			if (CurrentVideoClip != active ||
				CurrentAudioClip != null)
			{
				stateChanged = true;
				originalTimelineSelectionChanged = true;
				SetOriginalTimelinePlaybackMode(false);
			}
			CurrentVideoClip = ArrayOfOne_ClipsVideo[0] = active;
			CurrentAudioClip = null;
			resetCurrentClipUsages();
			if (active != null)
				CurClipAllUsagesVideo = ArrayOfOne_ClipsVideo;
            UpdateCurrentClipFrameAbsPos(proj);
        }

		/// <summary>
		/// There can only be one selected (active) b/w video and audio line, so video will be set to null
		/// </summary>
		public void SetActiveAudio(VidkaClipAudio active)
		{
			if (CurrentAudioClip != active ||
				CurrentVideoClip != null)
			{
				stateChanged = true;
				originalTimelineSelectionChanged = true;
				SetOriginalTimelinePlaybackMode(false);
			}
			CurrentAudioClip = ArrayOfOne_ClipsAudio[0] = active;
			CurrentVideoClip = null;
			resetCurrentClipUsages();
			if (active != null)
				CurClipAllUsagesAudio = ArrayOfOne_ClipsAudio;
            UpdateCurrentClipFrameAbsPos(null);
		}

        /// <summary>
        /// proj parameter is only needed for video clip, to find its abs position
        /// </summary>
        public void UpdateCurrentClipFrameAbsPos(VidkaProj proj)
        {
            CurrentClipFrameAbsPos = null;
            if (CurrentVideoClip != null && proj != null)
                CurrentClipFrameAbsPos = (long?)proj.GetVideoClipAbsFramePositionLeft(CurrentVideoClip);
            else if (CurrentAudioClip != null)
                CurrentClipFrameAbsPos = (long?)CurrentAudioClip.FrameOffset;
        }

		public void SetCurrentMarkerFrame(long frame) {
			if (frame < 0)
				frame = 0;
			if (CurrentMarkerFrame != frame)
				stateChanged = true;
			CurrentMarkerFrame = frame;
		}

		public void IncCurrentMarkerFrame(long frameInc)
		{
			SetCurrentMarkerFrame(CurrentMarkerFrame + frameInc);
		}

		public void SetOriginalTimelinePlaybackMode(bool flag)
		{
			if (OriginalTimelinePlaybackMode != flag)
				stateChanged = true;
			OriginalTimelinePlaybackMode = flag;
		}

		public void SetTrimHover(TrimDirection trimHover)
		{
			if (trimHover != TrimHover)
				stateChanged = true;
			TrimHover = trimHover;
		}

        public void SetShowEasingHandles(bool flag)
        {
            if (ShowEasingHandles != flag)
                stateChanged = true;
            ShowEasingHandles = flag;
        }

		public void PleaseShowAllUsages(VidkaProj proj)
		{
			stateChanged = true;
			CurClipAllUsagesVideo = proj.ClipsVideo.Where(x => x.FileName == CurrentClip.FileName);
			CurClipAllUsagesAudio = proj.ClipsAudio.Where(x => x.FileName == CurrentClip.FileName);
		}

		public void SetTrimThreshPixels(int trimThreshPixels)
		{
			if (trimThreshPixels != TrimThreshPixels)
				stateChanged = true;
			TrimThreshPixels = trimThreshPixels;
		}

		public void setMouseDragFrameDelta(long frameDelta)
		{
			if (MouseDragFrameDelta != frameDelta)
				stateChanged = true;
			MouseDragFrameDelta = frameDelta;
		}

        public void setMouseDragFrameDeltaMainTimelineOnly(bool p)
        {
            if (MouseDragFrameDeltaMTO != p)
                stateChanged = true;
            MouseDragFrameDeltaMTO = p;
        }

		public void SetDraggyCoordinates(
			EditorDraggyMode? mode = null,
			long? frameLength = null,
			string text = null,
			int? mouseX = null,
			int? mouseXOffset = null,
			bool? hasAudio = null,
            long? frameAbsLeft = null)
		{
			if (mode.HasValue && mode.Value != Draggy.Mode)
				stateChanged = true;
			if (frameLength.HasValue && frameLength.Value != Draggy.FrameLength)
				stateChanged = true;
			if (text != Draggy.Text)
				stateChanged = true;
			if (mouseX.HasValue && mouseX.Value != Draggy.MouseX)
				stateChanged = true;
			if (mouseXOffset.HasValue && mouseXOffset.Value != Draggy.MouseXOffset)
				stateChanged = true;
			if (hasAudio.HasValue && hasAudio.Value != Draggy.HasAudio)
				stateChanged = true;
            if (frameAbsLeft.HasValue && frameAbsLeft.Value != Draggy.FrameAbsLeft)
                stateChanged = true;
			Draggy.SetCoordinates(
				mode: mode,
				frameLength: frameLength,
				text: text,
				mouseX: mouseX,
				mouseXOffset: mouseXOffset,
				hasAudio: hasAudio,
                frameAbsLeft: frameAbsLeft);
		}

		public void ClearDraggy() {
			if (Draggy.Mode == EditorDraggyMode.None)
				return;
			Draggy.Clear();
			stateChanged = true;
		}

		public void SetDraggyVideo(VidkaClipVideoAbstract clip)
		{
			if (Draggy.VideoClip != clip)
				stateChanged = true;
			Draggy.VideoClip = clip;
			Draggy.HasAudio = (clip != null) ? clip.HasAudio : false;
		}

		public void SetDraggyAudio(VidkaClipAudio clip)
		{
			if (Draggy.AudioClip != clip)
				stateChanged = true;
			Draggy.AudioClip = clip;
		}

		public void ClearAll()
		{
			ClearDraggy();
			CurrentVideoClip = null;
			CurrentAudioClip = null;
			CurrentVideoClipHover = null;
			CurrentAudioClipHover = null;
			resetCurrentClipUsages();
			TrimHover = TrimDirection.None;
            ShowEasingHandles = false;
			SetCurrentMarkerFrame(0);
			stateChanged = true;
		}

		private void resetCurrentClipUsages()
		{
			CurClipAllUsagesVideo = EmptyCollection_ClipsVideo;
			CurClipAllUsagesAudio = EmptyCollection_ClipsAudio;
		}

		#endregion
    }

	public enum TrimDirection {
		None = 0,
		Left = 1,
		Right = 2,
	}
}
