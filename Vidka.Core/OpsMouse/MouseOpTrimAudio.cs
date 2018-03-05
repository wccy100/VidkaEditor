﻿using Miktemk.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vidka.Core.Error;
using Vidka.Core.Model;
using Vidka.Core.Properties;
using Vidka.Core.UiObj;

namespace Vidka.Core.OpsMouse
{
    // TODO: everything
	class MouseOpTrimAudio : MouseOpAbstract
	{
		private TrimDirection side;
		private ProjectDimensionsTimelineType timeline;
		private bool keyboardMode;

        public MouseOpTrimAudio(IVidkaOpContext iEditor,
			VidkaUiStateObjects uiObjects,
			ProjectDimensions dimdim,
			IVideoShitbox editor,
			IVideoPlayer videoPlayer,
			TrimDirection side,
			ProjectDimensionsTimelineType timeline)
			: base(iEditor, uiObjects, dimdim, editor, videoPlayer)
		{
			this.side = side;
			this.timeline = timeline;
			keyboardMode = false;
		}

		public override string Description { get {
			return "Trim video (" + side.ToString() + ")";
		} }

		public override bool TriggerBy_MouseDragStart(MouseButtons button, int x, int y)
		{
			return (button == MouseButtons.Left)
				&& (uiObjects.TimelineHover == timeline)
				&& (uiObjects.CurrentAudioClipHover != null)
                && (!uiObjects.CurrentAudioClipHover.IsLocked)
				&& (uiObjects.TrimHover == side);
		}

		public override void MouseDragStart(int x, int y, int w, int h)
		{
			IsDone = false;
			// I assume its not null, otherwise how do u have CurrentAudioClipTrimHover?
            var clip = uiObjects.CurrentAudioClipHover;
			uiObjects.SetActiveAudio(clip);
			keyboardMode = false;
		}

		public override void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h)
		{
			performDefensiveProgrammingCheck();
			var clip = uiObjects.CurrentAudioClip;
			var frameDelta = (timeline == ProjectDimensionsTimelineType.Original)
				? dimdim.convert_ScreenX2Frame_OriginalTimeline(deltaX, clip.FileLengthFrames, w)
				: dimdim.convert_AbsX2Frame(deltaX);
			//cxzxc("fd:" + frameDelta + ",isO:" + isOriginal);
			var frameDeltaContrained = clip.HowMuchCanBeTrimmed(side, frameDelta);
			
			// set UI objects...
			uiObjects.setMouseDragFrameDelta(frameDeltaContrained);
		}

		public override void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
			performDefensiveProgrammingCheck();
            var clip = uiObjects.CurrentAudioClip;
			var deltaConstrained = clip.HowMuchCanBeTrimmed(side, uiObjects.MouseDragFrameDelta);
			if (deltaConstrained != 0)
			{
                var startOld = clip.FrameStart;
                var endOld = clip.FrameEnd;
                var offsetOld = clip.FrameOffset;
                var startNew = (side == TrimDirection.Left)
                    ? clip.FrameStart + deltaConstrained
                    : clip.FrameStart;
                var endNew = (side == TrimDirection.Right)
                    ? clip.FrameEnd + deltaConstrained
                    : clip.FrameEnd;
                var offsetNew = (side == TrimDirection.Left)
                    ? clip.FrameOffset + deltaConstrained
                    : clip.FrameOffset;
				iEditor.AddUndableAction_andFireRedo(new UndoableAction() {
					Redo = () => {
						cxzxc("Trim " + side + ": " + deltaConstrained);
                        clip.FrameStart = startNew;
                        clip.FrameEnd = endNew;
                        clip.FrameOffset = offsetNew;
					},
					Undo = () => {
						cxzxc("UNDO Trim " + side + ": " + deltaConstrained);
                        clip.FrameStart = startOld;
                        clip.FrameEnd = endOld;
                        clip.FrameOffset = offsetOld;
					},
					PostAction = () => {
						iEditor.UpdateCanvasWidthFromProjAndDimdim();
						if (side == TrimDirection.Left)
							iEditor.SetFrameMarker_LeftOfAClip(clip);
						else if (side == TrimDirection.Right)
							iEditor.SetFrameMarker_RightOfAClipJustBefore(clip, proj);
					}
				});
			}
			if (uiObjects.MouseDragFrameDelta != 0)
			{
				// switch to KB mode
				keyboardMode = true;
				editor.AppendToConsole(VidkaConsoleLogLevel.Info, "Use arrow keys to adjust...");
			}
			else
			{
				// if there was no change (mouse click) then cancel this op
				IsDone = true;
			}
			uiObjects.setMouseDragFrameDelta(0);
		}

		public override void ApplyFrameDelta(long deltaFrame)
		{
			if (!keyboardMode)
				return;
			performDefensiveProgrammingCheck();
			var clip = uiObjects.CurrentVideoClip;
			var deltaConstrained = clip.HowMuchCanBeTrimmed(side, deltaFrame);
			if (deltaConstrained != 0)
			{
				iEditor.AddUndableAction_andFireRedo(new UndoableAction() {
					Redo = () => {
						cxzxc("Trim " + side + ": " + deltaConstrained);
						if (side == TrimDirection.Left)
							clip.FrameStart += deltaConstrained;
						else if (side == TrimDirection.Right)
							clip.FrameEnd += deltaConstrained;
					},
					Undo = () => {
						cxzxc("UNDO Trim " + side + ": " + deltaConstrained);
						if (side == TrimDirection.Left)
							clip.FrameStart -= deltaConstrained;
						else if (side == TrimDirection.Right)
							clip.FrameEnd -= deltaConstrained;
					},
					PostAction = () => {
						// show in video player
						var frameEdge = (side == TrimDirection.Right) ? clip.FrameEnd - 1 : clip.FrameStart;
						var second = proj.FrameToSec(frameEdge);
						videoPlayer.SetStillFrame(clip.FileName, second);
						//cxzxc("preview2:" + second);
					}
				});
			}
			// set ui objects (repaint regardless to give feedback to user that this operation is still in action)
			uiObjects.SetHoverVideo(clip);
			uiObjects.SetTrimHover(side);
			uiObjects.UiStateChanged();
		}

		public override void EnterPressed()
		{
			if (keyboardMode)
				IsDone = true;
		}

		public override void EndOperation()
		{
			IsDone = false;
			keyboardMode = false;
			uiObjects.SetTrimHover(TrimDirection.None);
		}

		//------------------------ privates --------------------------
		private void performDefensiveProgrammingCheck()
		{
			if (uiObjects.CurrentAudioClip == null) // should never happen but who knows
				throw new HowTheFuckDidThisHappenException(
					proj,
					String.Format(VidkaErrorMessages.TrimDragCurVideoNull, side.ToString()));
		}
	}
}
