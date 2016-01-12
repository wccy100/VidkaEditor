﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;

namespace Vidka.Core.Ops
{
    public class ShowClipUsage : _VidkaOp
    {
        public ShowClipUsage(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "ShowClipUsage";

        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.W);
        }

        public override void Run()
        {
            if (Context.UiObjects.CurrentClip == null)
            {
                Context.cxzxc("Nothing selected!");
                return;
            }
            Context.UiObjects.PleaseShowAllUsages(Context.Proj);
        }

    }
}