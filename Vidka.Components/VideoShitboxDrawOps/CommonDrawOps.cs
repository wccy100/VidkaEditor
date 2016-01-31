using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.ExternalOps;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

namespace Vidka.Components.VideoShitboxDrawOps
{
    /// <summary>
    /// To allow to choose the different colors and styles of highlight
    /// </summary>
    public enum OutlineClipType
    {
        Hover = 1,
        Active = 2,
    }

    public class CommonDrawOps
    {
        // constants
        private const int THUMB_MARGIN = 20;
        private const int THUMB_MARGIN_Y = 50;
        private const int EASING_BEZIER_MAX_WIDTH = 60;

        // helpers
        private Rectangle destRect, srcRect;
        private Point bzP1, bzP1c, bzP2, bzP2c;

        public CommonDrawOps()
        {
            destRect = new Rectangle();
            srcRect = new Rectangle();
            bzP1 = new Point();
            bzP1c = new Point();
            bzP2 = new Point();
            bzP2c = new Point();
        }

        /// <summary>
        /// singleton
        /// </summary>
        public static CommonDrawOps Ops {
            get { return singleton ?? (singleton = new CommonDrawOps()); }
        }
        private static CommonDrawOps singleton;

        /// <summary>
        /// The last parameter, sec2X is used to porperly position the wave within the given x1..x1+clipw space if it is too short
        /// </summary>
        public void DrawWaveform(
            Graphics g,
            IVidkaOpContext context,
            ImageCacheManager imgCache,
            string mediaFilename,
            double audioLengthSec,
            double videoLengthSec,
            double audioOffsetSec,
            int x1, int y1, int clipw, int cliph,
            double secStart, double secEnd,
            bool isMuted, bool hasCustomAudio)
        {
            double audioT1 = (audioOffsetSec + secStart) / audioLengthSec;
            double audioT2 = (audioOffsetSec + secEnd) / audioLengthSec;
            if (audioT1 > 1)
                return;
            if (audioT2 < 0)
                return;

            string waveFile = context.FileMapping.AddGetWaveFilenameJpg(mediaFilename);
            if (File.Exists(waveFile))
            {
                Bitmap bmpWave = imgCache.getWaveImg(waveFile);//Image ot

                double audioT1L = 0, audioT2R = 0;
                if (audioT1 < 0)
                {
                    audioT1L = -audioT1;
                    audioT1 = 0;
                }
                if (audioT2 > 1)
                {
                    audioT2R = audioT2 - 1;
                    audioT2 = 1;
                }
                var totalT = audioT1L + (audioT2 - audioT1) + audioT2R;
                var xAud1 = audioT1L * clipw / totalT;
                var xAud2 = (audioT1L + (audioT2 - audioT1)) * clipw / totalT;
                var xSrc1 = (int)(bmpWave.Width * audioT1);
                var xSrc2 = (int)(bmpWave.Width * audioT2);
                srcRect.X = xSrc1;
                srcRect.Width = xSrc2 - xSrc1;
                srcRect.Y = 0;
                srcRect.Height = bmpWave.Height; //TODO: use constant from Ops
                destRect.X = x1 + (int)xAud1;
                destRect.Y = y1;
                destRect.Width = (int)(xAud2 - xAud1);
                destRect.Height = cliph;
                g.DrawImage(bmpWave, destRect: destRect, srcRect: srcRect, srcUnit: GraphicsUnit.Pixel);
            }
            if (isMuted)
                g.FillRectangle(PPP.brushHazyMute, destRect);
            if (hasCustomAudio)
                g.FillRectangle(PPP.brushHazyCustomAudio, destRect);
        }

        /// <param name="secStart">needs to be in seconds to figure out which thumb</param>
        /// <param name="len">needs to be in seconds to figure out which thumb</param>
        public void DrawClipBitmaps(
            Graphics g,
            IVidkaOpContext context,
            ImageCacheManager imgCache,
            VidkaClipVideoAbstract vclip,
            int x1, int y1, int clipw, int clipvh,
            double secStart, double len)
        {
            string thumbsFile = context.FileMapping.AddGetThumbnailFilename(vclip.FileName);
            //if (!File.Exists(thumbsFile))
            //	return;
            //Image origThumb = System.Drawing.Image.FromFile(thumbsFile, true);
            //var bmpThumb = new Bitmap(origThumb);
            var heightForThumbs = Math.Max(clipvh - 2 * THUMB_MARGIN_Y, ThumbnailExtraction.ThumbH);
            var thumbPrefWidth = heightForThumbs * ThumbnailExtraction.ThumbW / ThumbnailExtraction.ThumbH;
            var howManyThumbs = (clipw - THUMB_MARGIN) / (thumbPrefWidth + THUMB_MARGIN);
            if (howManyThumbs == 0)
            {
                howManyThumbs = 1;

            }
            var xCenteringOffset = (clipw - howManyThumbs * (thumbPrefWidth + THUMB_MARGIN)) / 2;
            var isStill = vclip is VidkaClipImage
                || vclip is VidkaClipTextSimple; // TODO: I hate this code
            for (int i = 0; i < howManyThumbs; i++)
            {
                //DrawVideoThumbnail(
                //	g: g,
                //	bmpAll: bmpThumb,
                //	timeSec: secStart + (i + 0.5) * len / howManyThumbs,
                //	xCenter: x1 + xCenteringOffset + i * (thumbPrefWidth + THUMB_MARGIN) + (thumbPrefWidth + THUMB_MARGIN) / 2,
                //	yCenter: y1 + clipvh / 2,
                //	preferredWidth: thumbPrefWidth,
                //	maxWidth: clipw);
                var timeSec = secStart + (i + 0.5) * len / howManyThumbs;
                var imageIndex = (int)(timeSec / ThumbnailExtraction.ThumbIntervalSec);
                if (isStill)
                    imageIndex = 0;
                DrawVideoThumbnail(
                    g: g,
                    imgCache: imgCache,
                    filenameAll: thumbsFile,
                    index: imageIndex,
                    xCenter: x1 + xCenteringOffset + i * (thumbPrefWidth + THUMB_MARGIN) + (thumbPrefWidth + THUMB_MARGIN) / 2,
                    yCenter: y1 + clipvh / 2,
                    preferredWidth: thumbPrefWidth,
                    maxWidth: clipw);
            }
            //bmpThumb.Dispose();
            //origThumb.Dispose();
        }

        private void DrawVideoThumbnail(Graphics g, ImageCacheManager imgCache, string filenameAll, int index, int xCenter, int yCenter, int preferredWidth, int maxWidth)
        {
            var bmpThumb = imgCache.getThumb(filenameAll, index);
            srcRect.X = 0;
            srcRect.Y = 0;
            srcRect.Width = ThumbnailExtraction.ThumbW;
            srcRect.Height = ThumbnailExtraction.ThumbH;
            destRect.Width = preferredWidth;
            destRect.Height = preferredWidth * ThumbnailExtraction.ThumbH / ThumbnailExtraction.ThumbW;
            if (maxWidth < preferredWidth)
            {
                destRect.Width = maxWidth;
                var srcWidth = ThumbnailExtraction.ThumbW * maxWidth / preferredWidth;
                srcRect.X = (ThumbnailExtraction.ThumbW - srcWidth) / 2;
                srcRect.Width = srcWidth;
            }
            destRect.X = xCenter - destRect.Width / 2;
            destRect.Y = yCenter - destRect.Height / 2;
            g.DrawImage(bmpThumb, destRect: destRect, srcRect: srcRect, srcUnit: GraphicsUnit.Pixel);
        }

        /// <summary>
        /// This thing could be complex because we have curves... lol
        /// </summary>
        public void DrawVideoClipBorder(
            Graphics g,
            Pen pen15,
            int x1, int x2, int y1, int y2, int yEase2,
            int xEaseL, int xEaseR, long easeL, long easeR)
        {
            //g.DrawRectangle(pen15, x1, y1, clipW, y2 - y1);
            g.DrawLine(pen15, x1, y1, x2, y1);
            g.DrawLine(pen15, x1, y1, x1, y2);
            g.DrawLine(pen15, x2, y1, x2, y2);
            var yEase1 = y2;
            var bzXMainL = Math.Min(x1 + EASING_BEZIER_MAX_WIDTH, x2);
            var bzXMainR = Math.Max(x2 - EASING_BEZIER_MAX_WIDTH, x1);
            if (bzXMainL > bzXMainR)
                bzXMainL = bzXMainR = (bzXMainL + bzXMainR) / 2;
            if (easeL > 0)
            {
                var xEase1 = x1 - xEaseL;
                var xEase2 = x1;
                g.DrawLine(pen15, xEase1, yEase1, xEase2, yEase1);
                g.DrawLine(pen15, xEase1, yEase2, xEase2, yEase2);
                g.DrawLine(pen15, xEase1, yEase1, xEase1, yEase2);
                var bzXMainHalf = (xEase2 + bzXMainL) / 2;
                bzP1.X = xEase2;
                bzP1.Y = yEase2;
                bzP1c.X = bzXMainHalf;
                bzP1c.Y = yEase2;
                bzP2.X = bzXMainL;
                bzP2.Y = yEase1;
                bzP2c.X = bzXMainHalf;
                bzP2c.Y = yEase1;
                drawBesierFromMyBzPoints(g, pen15);
            }
            else
                g.DrawLine(pen15, x1, yEase1, bzXMainL, yEase1);
            if (easeR > 0)
            {
                var xEase1 = x2;
                var xEase2 = x2 + xEaseR;
                g.DrawLine(pen15, xEase1, yEase1, xEase2, yEase1);
                g.DrawLine(pen15, xEase1, yEase2, xEase2, yEase2);
                g.DrawLine(pen15, xEase2, yEase1, xEase2, yEase2);
                var bzXMainHalf = (xEase1 + bzXMainR) / 2;
                bzP1.X = xEase1;
                bzP1.Y = yEase2;
                bzP1c.X = bzXMainHalf;
                bzP1c.Y = yEase2;
                bzP2.X = bzXMainR;
                bzP2.Y = yEase1;
                bzP2c.X = bzXMainHalf;
                bzP2c.Y = yEase1;
                drawBesierFromMyBzPoints(g, pen15);
            }
            else
                g.DrawLine(pen15, x2, yEase1, bzXMainR, yEase1);
            g.DrawLine(pen15, bzXMainL, yEase1, bzXMainR, yEase1);
        }

        /// <summary>
        /// Draws one red bracket if drag frames = 0. If there has been a drag > 0,
        /// draws 2 brackets: one purple for original edge, one red for active (under mouse)
        /// </summary>
        public void DrawTrimBracket(Graphics g, int x, int y1, int y2, TrimDirection trimDirection, int bracketLength, int trimDeltaX,
            Pen penActive = null,
            Pen penActivePrev = null)
        {
            if (penActive == null)
                penActive = PPP.penActiveBoundary;
            if (penActivePrev == null)
                penActivePrev = PPP.penActiveBoundaryPrev;
            if (trimDeltaX == 0)
                drawTrimBracketSingle(g, penActive, x, y1, y2, trimDirection, bracketLength);
            else
            {
                g.FillRectangle(PPP.brushHazy, Math.Min(x, x + trimDeltaX), y1, Math.Abs(trimDeltaX), y2 - y1);
                drawTrimBracketSingle(g, penActivePrev, x, y1, y2, trimDirection, bracketLength);
                drawTrimBracketSingle(g, penActive, x + trimDeltaX, y1, y2, trimDirection, bracketLength);
            }
        }

        #region ---------------------------- helpers and shit -----------------------------------

        private void drawBesierFromMyBzPoints(Graphics g,  Pen penGray)
        {
            g.DrawBezier(penGray, bzP1, bzP1c, bzP2c, bzP2);
        }

        /// <summary>
        /// Only used in drawTrimBracket()
        /// </summary>
        private void drawTrimBracketSingle(Graphics g, Pen pen, int x, int y1, int y2, TrimDirection direction, int bracketLength)
        {
            var bracketDx = (direction == TrimDirection.Left)
                ? bracketLength
                : -bracketLength;
            g.DrawLine(pen, x, y1, x, y2);
            g.DrawLine(pen, x, y1, x + bracketDx, y1);
            g.DrawLine(pen, x, y2, x + bracketDx, y2);
        }

        #endregion


        #region ================================== AlignVideoAudio ===================================

        // .... these are copied from the old EditorDrawOps

        //internal void AlignVideoAudio_drawVideo(VidkaClipVideoAbstract vclip)
        //{
        //    var y1 = (int)(Height * 0.1);
        //    var cliph = (int)(Height * 0.5);
        //    var clipvh = (int)(Height * 0.3);
        //    var yEase = (int)(Height * 0.7);
        //    drawVideoClip(vclip, null, 0, y1, cliph, clipvh, yEase, PPP.brushWhite);
        //}

        //internal void AlignVideoAudio_drawAudio(VidkaClipAudio aclip, float audioOffsetSec)
        //{
        //    if (!aclip.FileLengthSec.HasValue || aclip.FileLengthSec == 0)
        //        return;
        //    if (string.IsNullOrEmpty(aclip.FileName))
        //        return;
        //    var yaudio = (int)(Height * 0.6);
        //    var y2 = (int)(Height * 0.9);
        //    int x1 = dimdim.convert_Frame2ScreenX(0);
        //    int x2 = dimdim.convert_Frame2ScreenX(0 + aclip.LengthFrameCalc);
        //    int xOffset = dimdim.convert_SecToAbsX(audioOffsetSec);
        //    DrawWaveform(aclip.FileName, aclip.FileLengthSec ?? 0, aclip.FileLengthSec ?? 0, 0, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio, 0, aclip.FileLengthSec ?? 0, false, false);
        //    g.DrawRectangle(PPP.penDefault, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio);
        //}

        #endregion
    }
}
