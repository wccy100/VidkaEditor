﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.ExternalOps;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Components
{
    public abstract class DrawOp
    {
        protected IVidkaOpContext context;
        protected ImageCacheManager imgCache;

        // misc vars for shortcuts
        protected ProjectDimensions dimdim;
        protected VidkaUiStateObjects uiObjects;
        protected VidkaFileMapping fileMapping;

        public DrawOp(IVidkaOpContext context, ImageCacheManager imageMan)
        {
            this.context = context;
            this.imgCache = imageMan;
            if (context != null)
            {
                dimdim = context.Dimdim;
                fileMapping = context.FileMapping;
                uiObjects = context.UiObjects;
            }
        }

        public abstract void Paint(Graphics g, int w, int h);
    }
}
