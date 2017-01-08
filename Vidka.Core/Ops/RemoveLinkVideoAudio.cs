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
    public class RemoveLinkVideoAudio : _VidkaOp
    {
        public RemoveLinkVideoAudio(IVidkaOpContext context) : base(context) { }
        public override string CommandName => Name;
        public const string Name = "RemoveLinkVideoAudio";


        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.Control && e.KeyCode == Keys.Oemtilde);
        }

        public override void Run()
        {
            var uiObjects = Context.UiObjects;
            var proj = Context.Proj;
            if (uiObjects.CurrentClip == null)
                return;

            if (uiObjects.CurrentVideoClip != null)
            {
                var vclip = uiObjects.CurrentVideoClip;
                var linksCache = vclip.AudioClipLinks.ToArray();
                Context.AddUndableAction_andFireRedo(new UndoableAction()
                {
                    Redo = () =>
                    {
                        cxzxc("removing link linking");
                        vclip.AudioClipLinks.Clear();
                    },
                    Undo = () =>
                    {
                        cxzxc("restoring video-audio links");
                        vclip.AudioClipLinks.AddRange(linksCache);
                    },
                    PostAction = () => { }
                });
            }
            if (uiObjects.CurrentAudioClip != null)
            {
                cxzxc("TODO: RemoveLinkVideoAudio when audio clip is selected");
            }
        }

    }
}
