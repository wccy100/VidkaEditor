using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vidka.Core.Ops
{
    public class TogglePreviewMode : _VidkaOp
    {
        public TogglePreviewMode(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "TogglePreviewMode";

        public override bool TriggerByKeyPress(KeyEventArgs e) {
            return (e.KeyCode == Keys.P);
        }

        public override void Run()
        {
            Context.Fire_PleaseTogglePreviewMode();
        }

    }
}
