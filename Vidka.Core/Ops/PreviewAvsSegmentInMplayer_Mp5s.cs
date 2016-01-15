using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.ExternalOps;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class PreviewAvsSegmentInMplayer_Mp5s : PreviewAvsSegmentInMplayerAbstract
    {
        public PreviewAvsSegmentInMplayer_Mp5s(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "PreviewAvsSegmentInMplayer_Mp5s";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.Control && !e.Shift && e.KeyCode == Keys.B);
        }

        public override void Run()
        {
            PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview, false, ExternalPlayerType.Mplayer);
        }
    }
}
