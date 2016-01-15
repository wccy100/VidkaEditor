using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Components.VideoShitboxDrawOps
{
    public class DrawVideoAudioLinkage : DrawOp
    {
        public static readonly Pen penVideoAudioLinkageGood = new Pen(Color.LightGreen, 10);
        public static readonly Pen penVideoAudioLinkageBad = new Pen(Color.Red, 10);
        public static readonly Pen penTarget = new Pen(Color.Red, 8);

        private Rectangle rect;

        public DrawVideoAudioLinkage(IVidkaOpContext context, ImageCacheManager imageMan)
            : base(context, imageMan)
        {
            rect = new Rectangle();
            penVideoAudioLinkageGood.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            penVideoAudioLinkageBad.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        }
        public override void Paint(Graphics g, int w, int h)
        {
            //int y1Video = dimdim.getY_main1(h);
            int y2Video = dimdim.getY_main2(h);
            //int yVideoAudio = dimdim.getY_main_half(h);
            int y1Audio = dimdim.getY_audio1(h);
            //int y2Audio = dimdim.getY_audio2(h);

            if (context.Proj.ClipsAudio.Count == 0)
                return;

            if (uiObjects.ShowVideoAudioLinkage && uiObjects.CurrentVideoClip != null)
            {
                dimdim.GetVClipScreenPosition(uiObjects.CurrentVideoClip, context.Proj, h, ref rect);
                if (uiObjects.CurrentAudioClipHover != null)
                {
                    var xAudio = dimdim.convert_Frame2ScreenX(uiObjects.CurrentAudioClipHover.FrameOffset);
                    var wAudio = dimdim.convert_FrameToAbsX(uiObjects.CurrentAudioClipHover.LengthFrameCalc);
                    g.DrawLine(penVideoAudioLinkageBad, rect.X + rect.Width / 2, rect.Y + rect.Height, xAudio + wAudio/2, y1Audio);
                    drawTarget(g, xAudio + wAudio / 2, y1Audio);
                }
                drawTarget(g, rect.X + rect.Width / 2, rect.Y + rect.Height);
            }

            var draggy = context.UiObjects.Draggy;
            dimdim.IterateOverVisibleVideoClips(context.Proj, w, (vclip, vclipPrev, x1, x2, curFrameVideo, index) =>
            {
                foreach (var aclipLink in vclip.AudioClipLinks)
                {
                    var aclipWhereabouts = curFrameVideo - vclip.FrameStart + aclipLink.SynchFrames;
                    var offsetShouldBe = aclipWhereabouts + aclipLink.AudioClip.FrameStart;
                    var deltaFrames = offsetShouldBe - aclipLink.AudioClip.FrameOffset;
                    var pen = (deltaFrames == 0)
                        ? penVideoAudioLinkageGood
                        : penVideoAudioLinkageBad;
                    var audioX = dimdim.convert_Frame2ScreenX(aclipLink.AudioClip.FrameOffset);
                    var audioW = dimdim.convert_FrameToAbsX(aclipLink.AudioClip.LengthFrameCalc);
                    var xChain = (Math.Max(x1, audioX) + Math.Min(x2, audioX + audioW))/2;
                    var xChainBent = xChain - dimdim.convert_FrameToAbsX(deltaFrames);
                    g.DrawLine(pen, xChain, y2Video, xChainBent, y1Audio);
                }
            });
        }

        private void drawTarget(Graphics g, int x, int y)
        {
            var radius1 = 1;
            var radius2 = 20;
            g.DrawEllipse(penTarget, x - radius1, y - radius1, radius1 * 2, radius1 * 2);
            g.DrawEllipse(penTarget, x - radius2, y - radius2, radius2 * 2, radius2 * 2);
        }

    }
}
