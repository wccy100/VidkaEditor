using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class SplitCurClipVideo_FinalizeLeft : SplitCurClipVideo
    {
        public SplitCurClipVideo_FinalizeLeft(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(SplitCurClipVideo_FinalizeLeft);
        public override string CommandName => Name;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.L && !e.Shift && !e.Control);
        }

        protected override void AdditionalActionsOnUndo(VidkaClipVideoAbstract clip, VidkaClipVideoAbstract clipNewOnTheLeft)
        {
            //clipNewOnTheLeft.IsLocked = false; // who cares, its is removed on undo anyway by SplitCurClipVideo.Run
        }
        protected override void AdditionalActionsOnRedo(VidkaClipVideoAbstract clip, VidkaClipVideoAbstract clipNewOnTheLeft)
        {
            clipNewOnTheLeft.IsLocked = true;
        }
    }
}
