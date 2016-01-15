using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vidka.Core.Error;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Core.OpsMouse
{
	class MouseOpSelectOriginalSegment : MouseOpAbstract
	{
		private bool keyboardMode;
		private long origFrame1;
		private long origFrame2;
        private long prevStart, prevEnd, prevEaseL, prevEaseR;
		private bool isStarted;

        public MouseOpSelectOriginalSegment(IVidkaOpContext iEditor,
			VidkaUiStateObjects uiObjects,
			ProjectDimensions dimdim,
			IVideoShitbox editor,
			IVideoPlayer videoPlayer)
			: base(iEditor, uiObjects, dimdim, editor, videoPlayer)
		{
			keyboardMode = false;
			isStarted = false;
		}

		public override string Description { get {
			return "Select Segment";
		} }

		public override bool TriggerBy_MouseDragStart(MouseButtons button, int x, int y)
		{
			return (button == MouseButtons.Right)
				&& (uiObjects.TimelineHover == ProjectDimensionsTimelineType.Original)
				&& (uiObjects.CurrentClip != null);
		}

		public override void MouseDragStart(int x, int y, int w, int h)
		{
			IsDone = false;
			keyboardMode = false;
            var clip = uiObjects.CurrentClip;
			prevStart = clip.FrameStart;
			prevEnd = clip.FrameEnd;
            prevEaseL = 0;
            prevEaseR = 0;
            if (clip is VidkaClipVideoAbstract)
            {
                var vclip = (VidkaClipVideoAbstract)clip;
                prevEaseL = vclip.EasingLeft;
                prevEaseR = vclip.EasingRight;
                vclip.EasingLeft = 0;
                vclip.EasingRight = 0;
            }
			origFrame1 = origFrame2 = dimdim.convert_ScreenX2Frame_OriginalTimeline(x, clip.FileLengthFrames, w);
            uiObjects.SetOriginalTimelinePlaybackMode(false);
            if (uiObjects.CurrentClipIsVideo)
            {
                var vclip = uiObjects.CurrentVideoClip;
                var clipAbsLeftFrame = proj.GetVideoClipAbsFramePositionLeft(vclip);
                uiObjects.SetActiveVideo(vclip, proj);
                uiObjects.SetCurrentMarkerFrame(clipAbsLeftFrame);
                updateVideoPlayerFromFrame2();
            }
            else
            {
                var aclip = uiObjects.CurrentAudioClip;
                uiObjects.SetActiveAudio(aclip);
                iEditor.SetFrameMarker_ShowFrameInPlayer(aclip.FrameOffset);
            }
        }

		public override void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            var clip = uiObjects.CurrentClip;
			origFrame2 = dimdim.convert_ScreenX2Frame_OriginalTimeline(x, clip.FileLengthFrames, w);
			if (origFrame1 != origFrame2) {
                clip.FrameStart = WhereWouldStartBe(origFrame1, origFrame2);
				clip.FrameEnd = WhereWouldEndBe(origFrame1, origFrame2, clip);
				uiObjects.UiStateChanged();
			}
			updateVideoPlayerFromFrame2();
		}

		public override void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
			if (origFrame1 != origFrame2)
			{
                var clip = uiObjects.CurrentClip;
                var newStart = WhereWouldStartBe(origFrame1, origFrame2);
                var newEnd = WhereWouldEndBe(origFrame1, origFrame2, clip);
				var oldStart = prevStart;
                var oldEnd = prevEnd;
                var oldEaseL = prevEaseL;
                var oldEaseR = prevEaseR;
				iEditor.AddUndableAction_andFireRedo(new UndoableAction()
				{
					Redo = () =>
					{
						cxzxc("Select region");
						clip.FrameStart = newStart;
                        clip.FrameEnd = newEnd;
                        if (clip is VidkaClipVideoAbstract)
                        {
                            var vclip = (VidkaClipVideoAbstract)clip;
                            vclip.EasingLeft = 0;
                            vclip.EasingRight = 0;
                        }
					},
					Undo = () =>
					{
						cxzxc("UNDO Select region");
						clip.FrameStart = oldStart;
						clip.FrameEnd = oldEnd;
                        if (clip is VidkaClipVideoAbstract)
                        {
                            var vclip = (VidkaClipVideoAbstract)clip;
                            vclip.EasingLeft = prevEaseL;
                            vclip.EasingRight = prevEaseR;
                        }
					},
					PostAction = () =>
					{
						iEditor.UpdateCanvasWidthFromProjAndDimdim();
                        if (uiObjects.CurrentClipIsVideo)
                        {
                            var vclip = uiObjects.CurrentVideoClip;
                            long frameMarker = proj.GetVideoClipAbsFramePositionLeft(vclip);
                            iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
                        }
					}
				});
				uiObjects.UiStateChanged();
			}
			// TODO: change here for KB mode
			IsDone = true;
			keyboardMode = false;
			uiObjects.ClearDraggy();
			uiObjects.UiStateChanged();
		}

		public override void KeyPressedArrow(Keys keyData)
		{
			//TODO: kb mode???
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


		//-------------------- helpers ------------------------------

		private void updateVideoPlayerFromFrame2()
        {
            if (uiObjects.CurrentVideoClip == null)
                return;
			var second = proj.FrameToSec(origFrame2);
			videoPlayer.SetStillFrame(uiObjects.CurrentVideoClip.FileName, second);
		}

        private long WhereWouldStartBe(long origFrame1, long origFrame2)
        {
            var start2b = Math.Min(origFrame1, origFrame2);
            if (start2b < 0)
                start2b = 0;
            return start2b;
        }

        private long WhereWouldEndBe(long origFrame1, long origFrame2, VidkaClip clip)
        {
            var end = Math.Max(origFrame1, origFrame2);
            if (end > clip.FileLengthFrames)
                end = clip.FileLengthFrames;
            return end;
        }

	}
}
