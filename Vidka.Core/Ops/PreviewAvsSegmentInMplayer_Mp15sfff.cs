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
    public class PreviewAvsSegmentInMplayer_Mp15sfff : PreviewAvsSegmentInMplayerAbstract
    {
        public PreviewAvsSegmentInMplayer_Mp15sfff(IVidkaOpContext context) : base(context) { }

        public const string Name = nameof(PreviewAvsSegmentInMplayer_Mp15sfff);
        public override string CommandName => Name;

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.Control && e.Shift && e.KeyCode == Keys.G);
        }

        public override void Run()
        {
            PreviewAvsSegmentInMplayer(Settings.Default.SecondsMplayerPreview2, true, ExternalPlayerType.Mplayer);
        }
    }
}
