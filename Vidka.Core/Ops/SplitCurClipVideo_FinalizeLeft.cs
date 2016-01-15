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
        public override string CommandName { get { return Name; } }
        public new const string Name = "SplitCurClipVideo_FinalizeLeft";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.L && !e.Shift && !e.Control);
        }

        public override void Run()
        {
            base.Run();
            ClipNewOnTheLeft.IsLocked = true;
        }
    }
}
