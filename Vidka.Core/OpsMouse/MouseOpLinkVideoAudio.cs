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
	class MouseOpLinkVideoAudio : MouseOpAbstract
	{
        public MouseOpLinkVideoAudio(IVidkaOpContext iEditor,
			VidkaUiStateObjects uiObjects,
			ProjectDimensions dimdim,
			IVideoShitbox editor,
			IVideoPlayer videoPlayer)
			: base(iEditor, uiObjects, dimdim, editor, videoPlayer)
		{}

		public override string Description { get {
			return "Link video and audio";
		} }

        public override bool DoesNewMouseDragCancelMe { get { return false; } }

        public override bool TriggerBy_KeyPress(Keys key)
        {
            if (uiObjects.CurrentVideoClip == null)
                return false;
            if (proj.ClipsAudio.Count == 0)
                return false;
            return (key == Keys.Oemtilde);
        }

        public override void Init()
        {
            uiObjects.SetShowVideoAudioLinkage(true); 
        }

        public override void MouseDragStart(int x, int y, int w, int h) { }
        public override void MouseDragged(int x, int y, int deltaX, int deltaY, int w, int h) { }

		public override void MouseDragEnd(int x, int y, int deltaX, int deltaY, int w, int h)
		{
            var vclip = uiObjects.CurrentVideoClip;
            var aclip = uiObjects.CurrentAudioClipHover;

            var absPosVid = proj.GetVideoClipAbsFramePositionLeft(vclip) - vclip.FrameStart - vclip.EasingLeft;
            var absPosAud = aclip.FrameOffset - aclip.FrameStart;
            var synchFrames = absPosAud - absPosVid;
            var newLink = new VidkaAudioClipLink
            {
                AudioClip = uiObjects.CurrentAudioClipHover,
                SynchFrames = synchFrames,
            };

            var oldLinkToSameAudio = vclip.AudioClipLinks.FirstOrDefault(lll => lll.AudioClip == aclip);
            if (oldLinkToSameAudio != null)
            {
                // .... swap VidkaAudioClipLink for the updated one
                iEditor.AddUndableAction_andFireRedo(new UndoableAction()
                {
                    Redo = () =>
                    {
                        cxzxc("update video-audio link: " + synchFrames);
                        vclip.AudioClipLinks.Remove(oldLinkToSameAudio);
                        vclip.AudioClipLinks.Add(newLink);
                    },
                    Undo = () =>
                    {
                        cxzxc("restore prev link " + oldLinkToSameAudio.SynchFrames);
                        vclip.AudioClipLinks.Remove(newLink);
                        vclip.AudioClipLinks.Add(oldLinkToSameAudio);
                    },
                    PostAction = () => { }
                });
                closeThisOp();
                return;
            }
            if (vclip == null || aclip == null)
            {
                closeThisOp();
                return;
            }
            iEditor.AddUndableAction_andFireRedo(new UndoableAction()
            {
                Redo = () =>
                {
                    cxzxc("Linking video-audio: " + synchFrames);
                    vclip.AudioClipLinks.Add(newLink);
                },
                Undo = () =>
                {
                    cxzxc("UNDO linking");
                    vclip.AudioClipLinks.Remove(newLink);
                },
                PostAction = () => {}
            });
            closeThisOp();
        }

		public override void EnterPressed()
		{
            closeThisOp();
		}

		public override void EndOperation()
		{
            closeThisOp();
		}

        private void closeThisOp()
        {
            uiObjects.SetShowVideoAudioLinkage(false);
            IsDone = true;
        }
	}
}
