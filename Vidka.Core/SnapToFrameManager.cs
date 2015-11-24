using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidka.Core
{
    public class SnapToFrameManager
    {
        private const int NPointsMax = 100;

        private long[] framesToSnap;
        private int nPoints = 0;

        public SnapToFrameManager()
        {
            framesToSnap = new long[NPointsMax];
        }


    }
}
