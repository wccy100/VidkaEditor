using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class RebuildProj : _VidkaOp
    {
        public RebuildProj(IVidkaOpContext context) : base(context) {}

        public const string Name = nameof(RebuildProj);
        public override string CommandName => Name;

        public override void Run()
        {
            Context.iiii("Rebuilding auxillary files...");
            if (!String.IsNullOrEmpty(Context.CurFileName))
                VidkaIO.RebuildAuxillaryFiles(Context.Proj, Context.CurFileName, Context.MetaGenerator, false);
            Context.iiii("Done rebuilding.");
        }

    }
}
