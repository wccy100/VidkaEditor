using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vidka.Components
{
    public class DrawOpsCollection
    {
        private DrawOp[] ops;
        
        public DrawOpsCollection(DrawOp[] ops)
        {
            this.ops = ops;
        }

        public void Paint(Graphics g, int w, int h)
        {
            foreach (var op in ops)
                op.Paint(g, w, h);
        }

    }
}
