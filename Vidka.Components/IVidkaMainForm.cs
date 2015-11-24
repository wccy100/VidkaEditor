using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidka.Components
{
    public interface IVidkaMainForm
    {
        void SwapPreviewPlayerUI(VidkaPreviewMode mode);
    }

    public enum VidkaPreviewMode
    {
        Normal = 1,
        Fast = 2,
    }
}
