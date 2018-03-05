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
    public class ToggleRenderSplitPoint : _VidkaOp
    {
        public ToggleRenderSplitPoint(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(ToggleRenderSplitPoint);
        public override string CommandName => Name;


        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.OemPipe);
        }

        public override void Run()
        {
            var clip = Context.UiObjects.CurrentVideoClip;
            if (clip == null)
                return;
            var oldValue = clip.IsRenderBreakupPoint;
            var newValue = !clip.IsRenderBreakupPoint;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Undo = () =>
                {
                    clip.IsRenderBreakupPoint = oldValue;
                },
                Redo = () =>
                {
                    clip.IsRenderBreakupPoint = newValue;
                },
                PostAction = () =>
                {
                    Context.Fire_ProjectUpdated_AsFarAsMenusAreConcerned();
                }
            });
        }

    }
}
