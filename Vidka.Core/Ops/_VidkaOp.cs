using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Vidka.Core.Ops
{
    public abstract class _VidkaOp
    {
        private IVidkaOpContext context;

        public _VidkaOp(IVidkaOpContext context)
        {
            this.context = context;
        }

        protected IVidkaOpContext Context { get { return context; } }

        #region ------------- to Override ----------------------

        public virtual string CommandName { get { return "_VidkaOp_noname"; } }
        public virtual bool TriggerByKeyPress(KeyEventArgs e) { return false; }
        public abstract void Run();

        #endregion
    }
}
