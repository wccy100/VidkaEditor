using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vidka.Core.Error;
using Vidka.Core.Model;
using Vidka.Core.Ops;

namespace Vidka.Core
{
	class EditOperationMoveAudio : EditOperationAbstract
	{
		private MetaGeneratorInOtherThread metaGenerator;
		private bool copyMode;
		private bool keyboardMode;
        private int clipX;
		private int clipW;
		private bool isStarted; // TODO: future plan to only initialize draggy when drag index is different... will avoid flickering... however there is no information about this index without the draggy

        public EditOperationMoveAudio(ISomeCommonEditorOperations iEditor,
			VidkaUiStateObjects uiObjects,
			ProjectDimensions dimdim,
			IVideoShitbox editor,
			IVideoPlayer videoPlayer,
			MetaGeneratorInOtherThread metaGenerator)
			: base(iEditor, uiObjects, dimdim, editor, videoPlayer)
		{
			this.metaGenerator = metaGenerator;
			copyMode = false;
			keyboardMode = false;
		}

		public override string Description { get {
			return copyMode ? "Copy clip" : "Move cip";
		} }

		public override bool TriggerBy_MouseDragStart(MouseButtons button, int x, int y)
		{
			return (button == MouseButtons.Left)
				&& (uiObjects.TimelineHover == ProjectDimensionsTimelineType.Audios)
				&& (uiObjects.CurrentAudioClipHover != null)
				&& (uiObjects.TrimHover == TrimDirection.None);
		}

		public override void MouseDragStart(int x, int y, int w, int h)
		{
            performDefensiveProgrammingCheck();
            IsDone = false;
			var clip = uiObjects.CurrentAudioClipHover;
            performDefensiveProgrammingCheck();
			clipX = dimdim.convert_Frame2ScreenX(clip.FrameOffset);
			clipW = dimdim.convert_FrameToAbsX(clip.LengthFrameCalc);
			uiObjects.SetActiveAudio(clip);
			uiObjects.SetDraggyCoordinates(
				mode: EditorDraggyMode.AudioTimeline,
				//mode: EditorDraggyMode.AudioTimeline, //tmp f-b-f drag test
				frameLength: clip.LengthFrameCalc,
				mouseX: x,
				mouseXOffset: x-clipX
			);
			if (Form.ModifierKeys == Keys.Control)
				copyMode = true;
			if (!copyMode)
				uiObjects.SetDraggyAudio(clip);
			uiObjects.SetHoverVideo(null);
			isStarted = false;
			keyboardMode = false;
		}

		public override void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            var x1 = x - deltaX;
			performDefensiveProgrammingCheck();
            var clip = uiObjects.CurrentAudioClip;
            var frameDelta = dimdim.convert_AbsX2Frame(deltaX);
            // don't allow clips to be dragged past frame 0!
            if (clip.FrameOffset + frameDelta < 0)
                frameDelta -= (clip.FrameOffset + frameDelta);
            var frameDeltaScrX = dimdim.convert_FrameToAbsX(frameDelta);
            uiObjects.SetDraggyCoordinates(mouseX: x1 + frameDeltaScrX);
		}

		public override void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            performDefensiveProgrammingCheck();
            var clip = uiObjects.CurrentAudioClip;
            var frameDelta = dimdim.convert_AbsX2Frame(deltaX);
            // don't allow clips to be dragged past frame 0!
            if (clip.FrameOffset + frameDelta < 0)
                frameDelta -= (clip.FrameOffset + frameDelta);
            if (frameDelta != 0)
            {
                if (copyMode)
                {
                    var newClip = clip.MakeCopy_AudioClip();
                    newClip.FrameOffset = clip.FrameOffset + frameDelta;
                    iEditor.AddUndableAction_andFireRedo(new UndoableAction()
                    {
                        Redo = () =>
                        {
                            cxzxc("copy: " + Path.GetFileName(clip.FileName));
                            proj.ClipsAudio.Add(newClip);
                            uiObjects.SetActiveAudio(newClip);
                        },
                        Undo = () =>
                        {
                            cxzxc("UNDO copy audio");
                            proj.ClipsAudio.Remove(newClip);
                            uiObjects.SetActiveAudio(clip);
                        },
                        PostAction = () =>
                        {
                            //uiObjects.UpdateCurrentClipFrameAbsPos(proj); // no need for this b/c SetActiveAudio above will also update abspos
                            iEditor.SetFrameMarker_ShowFrameInPlayer(uiObjects.CurrentAudioClip.FrameOffset);
                            iEditor.UpdateCanvasWidthFromProjAndDimdim();
                        }
                    });
                }
                else
                {
                    var oldOffset = clip.FrameOffset;
                    var newOffset = clip.FrameOffset + frameDelta;
                    iEditor.AddUndableAction_andFireRedo(new UndoableAction()
                    {
                        Redo = () =>
                        {
                            cxzxc("move: " + Path.GetFileName(clip.FileName));
                            clip.FrameOffset = newOffset;
                        },
                        Undo = () =>
                        {
                            cxzxc("UNDO move audio");
                            clip.FrameOffset = oldOffset;
                        },
                        PostAction = () =>
                        {
                            uiObjects.UpdateCurrentClipFrameAbsPos(proj);
                            iEditor.UpdateCanvasWidthFromProjAndDimdim();
                            iEditor.SetFrameMarker_ShowFrameInPlayer(clip.FrameOffset);
                        }
                    });
                }
            }
            IsDone = true;
            copyMode = false;
            uiObjects.ClearDraggy();
            uiObjects.UiStateChanged();
		}

		public override void KeyPressedArrow(Keys keyData)
		{
			//TODO: kb mode???
		}

		public override void ControlPressed()
		{
			copyMode = true;
			uiObjects.SetDraggyVideo(null);
			cxzxc("Changed to copy mode");
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
					String.Format(VidkaErrorMessages.MoveDragCurVideoNull));
		}
	}
}
