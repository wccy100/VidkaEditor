using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class ToggleConsoleVisibility : _VidkaOp
    {
        public ToggleConsoleVisibility(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "ToggleConsoleVisibility";


        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.O);
        }

        public override void Run()
        {
            Context.Fire_PleaseToggleConsoleVisibility();
        }

    }
}
