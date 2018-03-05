using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class ShowClipUsage : _VidkaOp
    {
        public ShowClipUsage(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(ShowClipUsage);
        public override string CommandName => Name;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.W);
        }

        public override void Run()
        {
            if (Context.UiObjects.CurrentClip == null)
            {
                cxzxc("Nothing selected!");
                return;
            }
            Context.UiObjects.PleaseShowAllUsages(Context.Proj);
        }

    }
}
