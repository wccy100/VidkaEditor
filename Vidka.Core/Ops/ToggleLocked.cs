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
    public class ToggleLocked : _VidkaOp
    {
        public ToggleLocked(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "ToggleLocked";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.F);
        }

        public override void Run()
        {
            if (Context.UiObjects.CurrentClip == null)
                return;
            var clip = Context.UiObjects.CurrentClip;
            var oldValue = clip.IsLocked;
            var newValue = !oldValue;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Redo = () =>
                {
                    cxzxc((newValue ? "lock" : "unlock") + " clip");
                    clip.IsLocked = newValue;
                },
                Undo = () =>
                {
                    cxzxc("UNDO " + (newValue ? "lock" : "unlock") + " clip");
                    clip.IsLocked = oldValue;
                },
            });
        }

    }
}
