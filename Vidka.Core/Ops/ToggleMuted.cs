using Miktemk.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class ToggleMuted : _VidkaOp
    {
        public ToggleMuted(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(ToggleMuted);
        public override string CommandName => Name;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.M);
        }

        public override void Run()
        {
            if (UiObjects.CurrentVideoClip == null && UiObjects.CurrentAudioClip != null)
            {
                cxzxc("Does it really makes sense to mute an audio clip? What's the point of your audio clip then? It's like castrating a rooster...");
                return;
            }
            if (UiObjects.CurrentVideoClip == null)
                return;
            var clip = UiObjects.CurrentVideoClip;
            var oldValue = clip.IsMuted;
            var newValue = !oldValue;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Redo = () =>
                {
                    cxzxc((newValue ? "mute" : "unmute") + " clip");
                    clip.IsMuted = newValue;
                },
                Undo = () =>
                {
                    cxzxc("UNDO " + (newValue ? "mute" : "unmute") + " clip");
                    clip.IsMuted = oldValue;
                },
            });
        }

    }
}
