using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vidka.Core.Error;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core
{
	class EditOperationVideoEasings : EditOperationAbstract
	{
		private bool keyboardMode;
        private TrimDirection side;

        public EditOperationVideoEasings(IVidkaOpContext iEditor,
			VidkaUiStateObjects uiObjects,
			ProjectDimensions dimdim,
			IVideoShitbox editor,
			IVideoPlayer videoPlayer,
            TrimDirection side)
			: base(iEditor, uiObjects, dimdim, editor, videoPlayer)
		{
			this.side = side;
			keyboardMode = false;
		}

		public override string Description { get {
			return "Edit video easings";
		} }

        public override bool TriggerBy_MouseDragStart(MouseButtons button, int x, int y)
        {
            return (button == MouseButtons.Left)
                && (uiObjects.TimelineHover == ProjectDimensionsTimelineType.Main ||
                    uiObjects.TimelineHover == ProjectDimensionsTimelineType.MainEases)
                && (uiObjects.CurrentVideoClipHover != null)
                && (uiObjects.CurrentVideoClipHover.HasAudio || uiObjects.CurrentVideoClipHover.HasCustomAudio)
                //&& (!uiObjects.CurrentVideoClipHover.IsLocked)
                && (uiObjects.TrimHover == side)
                && (uiObjects.ShowEasingHandles);
        }

		public override void MouseDragStart(int x, int y, int w, int h)
		{
			var clip = uiObjects.CurrentVideoClipHover;
			uiObjects.SetActiveVideo(clip, proj);
			keyboardMode = false;

            var deltaBoundPositive = clip.LengthFrameCalc - 1 - clip.EasingLeft - clip.EasingRight;
            var deltaBoundNegative = clip.EasingLeft;
            //cxzxc("b-:" + deltaBoundNegative + ",b+:" + deltaBoundPositive);
		}

        //public override void KeyPressedOther(Keys key)
        //{
        //    if (key == Keys.J) {
        //        uiObjects.SetShowEasingHandles(true);
        //    }
        //}

		public override void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            var clip = uiObjects.CurrentVideoClip;
            var frameDelta = dimdim.convert_AbsX2Frame(deltaX);
            var frameDeltaContrained = clip.HowMuchCanBeEased(side, frameDelta);

            // set UI objects...
            uiObjects.setMouseDragFrameDeltaMainTimelineOnly(true);
            uiObjects.setMouseDragFrameDelta(frameDeltaContrained);
            //cxzxc("fd:" + frameDelta + ",fdC:" + frameDeltaContrained);

            // show in video player
            var frameClip = proj.GetVideoClipAbsFramePositionLeft(clip);
            var frameOffsetMarker = (side == TrimDirection.Left)
                ? frameClip - clip.EasingLeft + frameDeltaContrained
                : frameClip + clip.EasingRight + frameDeltaContrained;
            iEditor.ShowFrameInVideoPlayer(frameOffsetMarker);
		}

		public override void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            var clip = uiObjects.CurrentVideoClip;
            var frameDelta = dimdim.convert_AbsX2Frame(deltaX);
            var deltaConstrained = clip.HowMuchCanBeEased(side, frameDelta);

            //var clip = uiObjects.CurrentVideoClip;
            //var deltaConstrained = clip.HowMuchCanBeTrimmed(side, uiObjects.MouseDragFrameDelta);
            if (deltaConstrained != 0)
            {
                iEditor.AddUndableAction_andFireRedo(new UndoableAction()
                {
                    Redo = () =>
                    {
                        cxzxc("Easing " + side + ": " + deltaConstrained);
                        if (side == TrimDirection.Left)
                            clip.EasingLeft += -deltaConstrained;
                        else if (side == TrimDirection.Right)
                            clip.EasingRight += deltaConstrained;
                    },
                    Undo = () =>
                    {
                        cxzxc("UNDO easing " + side + ": " + deltaConstrained);
                        if (side == TrimDirection.Left)
                            clip.EasingLeft -= -deltaConstrained;
                        else if (side == TrimDirection.Right)
                            clip.EasingRight -= deltaConstrained;
                    },
                    PostAction = () =>
                    {
                        iEditor.UpdateCanvasWidthFromProjAndDimdim();
                        if (side == TrimDirection.Left) {
                            long frameMarker = proj.GetVideoClipAbsFramePositionLeft(clip);
			                iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker - clip.EasingLeft);
                        }
                        else if (side == TrimDirection.Right)
                            iEditor.SetFrameMarker_RightOfVClipJustBefore(clip, proj);
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
            uiObjects.setMouseDragFrameDeltaMainTimelineOnly(false);
        }

		public override void ApplyFrameDelta(long deltaFrame)
		{
            //if (!keyboardMode)
            //    return;
            //var clip = uiObjects.CurrentVideoClip;
            //var deltaConstrained = clip.HowMuchCanBeTrimmed(side, deltaFrame);
            //if (deltaConstrained != 0)
            //{
            //    iEditor.AddUndableAction_andFireRedo(new UndoableAction() {
            //        Redo = () => {
            //            cxzxc("Trim " + side + ": " + deltaConstrained);
            //            if (side == TrimDirection.Left)
            //                clip.FrameStart += deltaConstrained;
            //            else if (side == TrimDirection.Right)
            //                clip.FrameEnd += deltaConstrained;
            //        },
            //        Undo = () => {
            //            cxzxc("UNDO Trim " + side + ": " + deltaConstrained);
            //            if (side == TrimDirection.Left)
            //                clip.FrameStart -= deltaConstrained;
            //            else if (side == TrimDirection.Right)
            //                clip.FrameEnd -= deltaConstrained;
            //        },
            //        PostAction = () => {
            //            // show in video player
            //            var frameEdge = (side == TrimDirection.Right) ? clip.FrameEnd - 1 : clip.FrameStart;
            //            var second = proj.FrameToSec(frameEdge);
            //            videoPlayer.SetStillFrame(clip.FileName, second);
            //            //cxzxc("preview2:" + second);
            //        }
            //    });
            //}
            //// set ui objects (repaint regardless to give feedback to user that this operation is still in action)
            //uiObjects.SetHoverVideo(clip);
            //uiObjects.SetTrimHover(side);
            //uiObjects.UiStateChanged();
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
            uiObjects.SetShowEasingHandles(false);
		}
	}
}
