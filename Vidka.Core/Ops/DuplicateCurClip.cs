using Miktemk.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class DuplicateCurClip : _VidkaOp
    {
        public DuplicateCurClip(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "DuplicateCurClip";


        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.Control && e.Shift && e.KeyCode == Keys.D);
        }

        public override void Run()
        {
            var uiObjects = Context.UiObjects;
            var proj = Context.Proj;
            if (uiObjects.CurrentClip == null)
                return;
            if (uiObjects.CurrentVideoClip != null)
            {
                var toDuplicate = uiObjects.CurrentVideoClip;
                var clipIndex = proj.ClipsVideo.IndexOf(toDuplicate);
                var duplicat = toDuplicate.MakeCopy_VideoClip();
                Context.AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        cxzxc("duplicate vclip " + clipIndex);
                        proj.ClipsVideo.Insert(clipIndex + 1, duplicat);
                        uiObjects.SetActiveVideo(duplicat, proj);
                    },
                    Undo = () =>
                    {
                        cxzxc("UNDO duplicate vclip " + clipIndex);
                        proj.ClipsVideo.Remove(duplicat);
                        uiObjects.SetActiveVideo(toDuplicate, proj);
                    },
                    PostAction = () =>
                    {
                        uiObjects.SetHoverVideo(null);
                    }
                });
            }
            if (uiObjects.CurrentAudioClip != null)
            {
                var toDuplicate = uiObjects.CurrentAudioClip;
                var duplicat = toDuplicate.MakeCopy_AudioClip();
                duplicat.FrameOffset += toDuplicate.LengthFrameCalc;
                Context.AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        cxzxc("duplicate aclip " + Path.GetFileName(duplicat.FileName));
                        proj.ClipsAudio.Add(duplicat);
                        uiObjects.SetActiveAudio(duplicat);
                    },
                    Undo = () =>
                    {
                        cxzxc("UNDO duplicate aclip " + Path.GetFileName(duplicat.FileName));
                        proj.ClipsAudio.Remove(duplicat);
                        uiObjects.SetActiveAudio(toDuplicate);
                    },
                    PostAction = () =>
                    {
                        uiObjects.SetHoverVideo(null);
                        uiObjects.SetHoverAudio(null);
                    }
                });
            }
        }

    }
}
