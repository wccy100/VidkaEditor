using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Components.Properties;
using Vidka.Core;
using Vidka.Core.Model;
using Vidka.Core.ExternalOps;

namespace Vidka.Components
{
	/// <summary>
	/// To allow to choose the different colors and styles of highlight
	/// </summary>
	public enum OutlineClipType {
		Hover = 1,
		Active = 2,
	}

	public class EditorDrawOps
	{
		// constants
		private const int THUMB_MARGIN = 20;
		private const int THUMB_MARGIN_Y = 50;
        private const int EASING_BEZIER_MAX_WIDTH = 60;

		// objects used for drawing
		private Graphics g;
		private ProjectDimensions dimdim;
		private VidkaUiStateObjects uiObjects;
		private int Width, Height;
		private ImageCacheManager imgCache;
		private VidkaFileMapping fileMapping;
		private VidkaProj proj;
		
		// helpers
		private Rectangle destRect, srcRect;
        private Point bzP1, bzP1c, bzP2, bzP2c;

		public EditorDrawOps()
		{
			// init
			destRect = new Rectangle();
			srcRect = new Rectangle();
            bzP1 = new Point();
            bzP1c = new Point();
            bzP2 = new Point();
            bzP2c = new Point();
		}

		public void setParameters(
			Graphics g,
			EditorLogic logic,
			ImageCacheManager imgCache,
			int Width,
			int Height)
		{
			this.g = g;
			this.imgCache = imgCache;
			this.Width = Width;
			this.Height = Height;
			if (logic != null) {
				dimdim = logic.Dimdim;
				fileMapping = logic.FileMapping;
				uiObjects = logic.UiObjects;
				proj = logic.Proj;
			}
		}

		/// <summary>
		/// Used by controls that need to use draw ops but don't have main logic
		/// </summary>
		/// <param name="g"></param>
		/// <param name="imgCache"></param>
		/// <param name="dimdim"></param>
		/// <param name="fileMapping"></param>
		/// <param name="Width"></param>
		/// <param name="Height"></param>
		public void setParameters2(
			Graphics g,
			ImageCacheManager imgCache,
			ProjectDimensions dimdim,
			VidkaFileMapping fileMapping,
			VidkaUiStateObjects uiObjects,
			VidkaProj proj,
			int Width,
			int Height)
		{
			this.g = g;
			this.imgCache = imgCache;
			this.Width = Width;
			this.Height = Height;
			this.dimdim = dimdim;
			this.fileMapping = fileMapping;
			this.uiObjects = uiObjects;
			this.proj = proj;
		}

		#region ================================== Video shitbox ===================================

		public void PrepareCanvas()
		{
			int yMain1 = dimdim.getY_main1(Height);
			int yMain2 = dimdim.getY_main2(Height);
			int yMainHalf = dimdim.getY_main_half(Height);
			int yAudio1 = dimdim.getY_audio1(Height);
			int yAudio2 = dimdim.getY_audio2(Height);
			var hover = uiObjects.TimelineHover;
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? PPP.brushLightGray2 : PPP.brushLightGray, 0, yMain1, Width, yMainHalf - yMain1);
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? PPP.brushLightGray3 : PPP.brushLightGray2, 0, yMainHalf, Width, yMain2 - yMainHalf);
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Audios) ? PPP.brushLightGray3 : PPP.brushLightGray2, 0, yAudio1, Width, yAudio2 - yAudio1);
		}

		public void DrawTimeAxis()
		{
			// compute how many segments/ticks/etc to draw
			var frameStart = dimdim.convert_ScreenX2Frame(0);
			var frameEnd = dimdim.convert_ScreenX2Frame(Width);
			int nSegments = Width / Settings.Default.MinnimumTimelineSegmentSizeInPixels;
			long framesPerSegment = (frameEnd - frameStart) / nSegments;
			int secondsPerSegment = (int)(framesPerSegment / proj.FrameRate);
			secondsPerSegment = Utils.GetClosestSnapToSecondsForTimeAxis(secondsPerSegment);
			if (secondsPerSegment == 0) // we are zoomed in so much, but still show seconds
				secondsPerSegment = 1;
			// now that everything is rounded, how many segments do we really have?
			var actualFramesPerSegment = secondsPerSegment * proj.FrameRate;
			var actualNSegments = (int)((frameEnd - frameStart) / actualFramesPerSegment);
			var startingSecond = (long)Math.Floor(proj.FrameToSec(frameStart));
			
			// compute dimensions...
			var y1 = Height - dimdim.getY_timeAxisHeight(Height);
			var y2 = Height;

			g.DrawLine(PPP.penGray, 0, y1, Width, y1);
			for (var i = 0; i < actualNSegments+1; i++)
			{
				var curSecond = startingSecond + i * secondsPerSegment;
				var scrX = dimdim.convert_Sec2ScreenX(curSecond);
				var ts = TimeSpan.FromSeconds(curSecond);
				g.DrawLine(PPP.penGray, scrX, y1, scrX, y2);
				g.DrawString(ts.ToString_MinuteOrHour(), PPP.fontDefault, PPP.brushDefault, scrX+2, y1+4);
			}
			if (secondsPerSegment == 1 && framesPerSegment <= proj.FrameRate)
			{
				// draw frame ticks as well
				for (var i = frameStart; i < frameEnd; i++) {
					var scrX = dimdim.convert_Frame2ScreenX(i);
					g.DrawLine(PPP.penGray, scrX, y1, scrX, y1+3);
				}
			}
		}

		public void DrawProjectVideoTimeline()
		{
			var draggy = uiObjects.Draggy;
			var currentVideoClip = uiObjects.CurrentVideoClip;

			long curFrame = 0;
			int y1 = dimdim.getY_main1(Height);
			int y2 = dimdim.getY_main2(Height);
			int yaudio = dimdim.getY_main_half(Height);
            int yEase = dimdim.getY_main_easing2(Height);
            int cliph = y2 - y1; // clip height (video and audio)
			int clipvh = yaudio - y1; // clip (only video) height (just the video part, no audio!)
			int index = 0;
			int draggyVideoShoveIndex = dimdim.GetVideoClipDraggyShoveIndex(draggy);

			VidkaClipVideoAbstract vclipPrev = null;
			foreach (var vclip in proj.ClipsVideo)
			{
				if (dimdim.isEvenOnTheScreen(curFrame, curFrame + vclip.LengthFrameCalc, Width))
				{
					if (draggy.Mode == EditorDraggyMode.VideoTimeline && draggyVideoShoveIndex == index)
					{
						drawDraggyVideo(curFrame, y1, cliph, clipvh, draggy);
						curFrame += draggy.FrameLength;
					}

					if (draggy.VideoClip != vclip)
					{
						var brush = PPP.brushWhite;
						if (vclip == currentVideoClip && vclip.IsLocked)
							brush = PPP.brushLockedActiveClip;
						else if (vclip == currentVideoClip)
							brush = PPP.brushActive;
						else if (vclip.IsLocked)
							brush = PPP.brushLockedClip;
						drawVideoClip(vclip, vclipPrev,
							curFrame,
							y1, cliph, clipvh, yEase,
							brush);
					}
				}

				index++;
				if (draggy.VideoClip != vclip)
					curFrame += vclip.LengthFrameCalc;
				vclipPrev = vclip;
			}

			if (draggy.Mode == EditorDraggyMode.VideoTimeline && draggyVideoShoveIndex == index)
				drawDraggyVideo(curFrame, y1, cliph, clipvh, draggy);
		}

		private void drawVideoClip(
			VidkaClipVideoAbstract vclip,
			VidkaClipVideoAbstract vclipPrev,
			long curFrame,
			int y1, int cliph, int clipvh, int yEase,
			Brush brushClip)
		{
			int x1 = dimdim.convert_Frame2ScreenX(curFrame);
			int x2 = dimdim.convert_Frame2ScreenX(curFrame + vclip.LengthFrameCalc);
            var yEase1 = y1 + cliph;
            var yEase2 = yEase;
			int clipw = x2 - x1;
            var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
            var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);

			// .... active video clip deserves a special outline, fill white otherwise to hide gray background
			g.FillRectangle(brushClip, x1, y1, clipw, clipvh);
			DrawClipBitmaps(
				vclip: vclip,
				x1: x1,
				y1: y1,
				clipw: clipw,
				clipvh: clipvh,
				secStart: proj.FrameToSec(vclip.FrameStartNoEase),
				len: proj.FrameToSec(vclip.LengthFrameCalc));
            if (vclip.HasAudio || vclip.HasCustomAudio)
            {
                var waveFile = vclip.HasCustomAudio ? vclip.CustomAudioFilename : vclip.FileName;
                var waveOffset = vclip.HasCustomAudio ? vclip.CustomAudioOffset : 0;
                var waveLength = vclip.HasCustomAudio ? vclip.CustomAudioLengthSec : vclip.FileLengthSec;
                DrawWaveform(waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset, x1, y1 + clipvh, clipw, cliph - clipvh,
                    proj.FrameToSec(vclip.FrameStart + vclip.EasingLeft), proj.FrameToSec(vclip.FrameEnd - vclip.EasingRight),
                    vclip.IsMuted, vclip.HasCustomAudio);
				// waveform separator
                g.DrawLine(PPP.penGray, x1, y1 + clipvh, x2, y1 + clipvh);
                //g.DrawRectangle(PPP.penGray, x1, y1 + clipvh, x2 - x1, cliph - clipvh);
			}

			// .... still analyzing...
			if (vclip.IsNotYetAnalyzed)
				g.DrawString("Still analyzing...", PPP.fontDefault, PPP.brushDefault, x1+5, y1+5);
            // .... easings
            if (vclip.HasAudio || vclip.HasCustomAudio)
            {
                var waveFile = vclip.HasCustomAudio ? vclip.CustomAudioFilename : vclip.FileName;
                var waveOffset = vclip.HasCustomAudio ? vclip.CustomAudioOffset : 0;
                var waveLength = vclip.HasCustomAudio ? vclip.CustomAudioLengthSec : vclip.FileLengthSec;
                if (vclip.EasingLeft > 0)
                {
                    //g.FillRectangle(brushHazyCustomAudio, xEase1, yEase1, xEase2 - xEase1, yEase2 - yEase1);
                    //g.DrawRectangle(PPP.penGray, xEase1, yEase1, xEase2 - xEase1, yEase2 - yEase1);
                    DrawWaveform(waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset,
                        x1 - xEaseLeft, yEase1, xEaseLeft, yEase2 - yEase1,
                        proj.FrameToSec(vclip.FrameStart), proj.FrameToSec(vclip.FrameStart + vclip.EasingLeft),
                        vclip.IsMuted, vclip.HasCustomAudio);
                }
                if (vclip.EasingRight > 0)
                {
                    //g.FillRectangle(brushHazyCustomAudio, xEase1, yEase1, xEase2 - xEase1, yEase2 - yEase1);
                    //g.DrawRectangle(PPP.penGray, xEase1, yEase1, xEase2 - xEase1, yEase2 - yEase1);
                    DrawWaveform(waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset,
                        x2, yEase1, xEaseRight, yEase2 - yEase1,
                        proj.FrameToSec(vclip.FrameEnd - vclip.EasingRight), proj.FrameToSec(vclip.FrameEnd),
                        vclip.IsMuted, vclip.HasCustomAudio);
                }
            }

            if (vclip.IsPixelTypeStandard)
            {
                g.FillEllipse(PPP.brushGreenPixelTypeStandard, x1+5, y1+5, 10, 10);
            }

            // .... outline this clip
            DrawVideoClipBorder(PPP.penDefault, x1, x1 + clipw, y1, y1 + (vclip.HasAudio ? cliph : clipvh), yEase2, xEaseLeft, xEaseRight, vclip.EasingLeft, vclip.EasingRight);
            //g.DrawRectangle(PPP.penDefault, x1, y1, clipw, vclip.HasAudio ? cliph : clipvh);

            // if vclipPrev.end == vclip.start and they are same file, mark green indicator
            if (vclipPrev != null && vclipPrev.FileName == vclip.FileName && vclipPrev.FrameEnd == vclip.FrameStart)
                g.DrawLine(PPP.penSealed, x1, y1 + 10, x1, y1 + clipvh);
		}

		private void drawDraggyVideo(long curFrame, int y1, int cliph, int clipvh, EditorDraggy draggy)
		{
			var draggyX = dimdim.convert_Frame2ScreenX(curFrame);
			var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
			var draggyH = (draggy.HasAudio) ? cliph : clipvh;
			if (draggy.VideoClip != null)
			{
				g.FillRectangle(PPP.brushWhite, draggyX, y1, draggyW, draggyH);
				g.FillRectangle(PPP.brushActive, draggyX, y1, draggyW, clipvh);
			}
			g.DrawRectangle(PPP.penBorderDrag, draggyX, y1, draggyW, draggyH);
			g.DrawString(draggy.Text, PPP.fontDefault, PPP.brushDefault, draggyX + 5, y1 + 5);
			
			// debug rect
			//g.DrawRectangle(PPP.penDefault, draggy.MouseX-draggy.MouseXOffset, y1-2, draggyW, cliph+5);
		}

		public void DrawProjectAudioTimeline()
		{
			var draggy = uiObjects.Draggy;

			int y1 = dimdim.getY_audio1(Height);
			int y2 = dimdim.getY_audio2(Height);
			int cliph = y2 - y1;

			foreach (var aclip in proj.ClipsAudio)
			{
                var frame1 = aclip.FrameOffset;
                var frame2 = aclip.FrameOffset + aclip.LengthFrameCalc;
                if (dimdim.isEvenOnTheScreen(frame1, frame2, Width))
				{
                    int x1 = dimdim.convert_Frame2ScreenX(frame1);
                    int x2 = dimdim.convert_Frame2ScreenX(frame2);
					int clipw = x2 - x1;

                    DrawWaveform(aclip.FileName, aclip.FileLengthSec ?? 0, aclip.FileLengthSec ?? 0, 0, x1, y1, clipw, cliph,
                        proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd),
                        false, false);

					//throw new NotImplementedException("DrawWaveform that takes Audio clip!!!");
					//DrawWaveform(g, proj, aclip, x1, y1, clipw, cliph,
					//	proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd));

                    // active video clip deserves a special outline
                    if (aclip == uiObjects.CurrentAudioClip)
                        g.FillRectangle(PPP.brushHazyActive, x1, y1, clipw, cliph);

					// outline rect
					g.DrawRectangle(PPP.penDefault, x1, y1, clipw, cliph);
				}
			}
			if (draggy.Mode == EditorDraggyMode.AudioTimeline)
			{
				var draggyX = draggy.MouseX - draggy.MouseXOffset;
				var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
				g.DrawRectangle(PPP.penBorderDrag, draggyX, y1, draggyW, cliph);
			}
		}

		private void DrawVideoThumbnail(string filenameAll, int index, int xCenter, int yCenter, int preferredWidth, int maxWidth)
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

		public void DrawBorder(Graphics g, int Width, int Height)
		{
			g.DrawRectangle(PPP.penBorder, 0, 0, Width - 1, Height - 1);
		}

		public void OutlineClipVideoHover()
		{
			var vclip = uiObjects.CurrentVideoClipHover;

			int y1 = dimdim.getY_main1(Height);
            //int y2 = vclip.HasAudio
            //    ? dimdim.getY_main2(Height)
            //    : dimdim.getY_main_half(Height);
            int y2 = dimdim.getY_main2(Height); // I finally decided against 1/2 blue hover rect on no-audio clips, it confuses the easing collision -Oct 25, 2015
            int yAudio = dimdim.getY_main_half(Height);
            int yEase1 = dimdim.getY_main_easing1(Height);
            int yEase2 = dimdim.getY_main_easing2(Height);
			//int yaudio = dimdim.getY_main_half(Height);
			int x1 = dimdim.getScreenX1_video(vclip);
			int clipW = dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); // hacky, I know
            var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
            var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);
            //DrawOutlineOfAnyClip(x1, y1, clipW, y2, yAudio, yEase);

            DrawVideoClipBorder(PPP.penHover, x1, x1 + clipW, y1, y2, yEase2, xEaseLeft, xEaseRight, vclip.EasingLeft, vclip.EasingRight);
            
            // .... draw outline
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            //g.DrawRectangle(PPP.penHover, x1, y1, clipW, y2 - y1);
            g.DrawLine(PPP.penHover, x1, y1, x1 + clipW, y1);
            g.DrawLine(PPP.penHover, x1, y1, x1, y2);
            g.DrawLine(PPP.penHover, x1 + clipW, y1, x1 + clipW, y2);
            if (uiObjects.TrimHover == TrimDirection.Left)
            {
                if (uiObjects.ShowEasingHandles && vclip.EasingLeft > 0)
                    drawTrimBracket(x1 - xEaseLeft, yEase1, yEase2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else if (uiObjects.ShowEasingHandles)
                    drawTrimBracket(x1, yAudio, yEase2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else
                    drawTrimBracket(x1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
            }
            if (uiObjects.TrimHover == TrimDirection.Right)
            {
                if (uiObjects.ShowEasingHandles && vclip.EasingRight > 0)
                    drawTrimBracket(x1 + clipW + xEaseRight, yEase1, yEase2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else if (uiObjects.ShowEasingHandles)
                    drawTrimBracket(x1 + clipW, yAudio, yEase2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
                else
                    drawTrimBracket(x1 + clipW, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
            }

            // .... draw seal when prev/next clip match
            if (uiObjects.TrimHover != TrimDirection.None)
            {
                var index = proj.ClipsVideo.IndexOf(vclip);
                if (uiObjects.TrimHover == TrimDirection.Left && index > 0)
                {
                    var prevClip = proj.ClipsVideo[index - 1];
                    if (prevClip.FileName == vclip.FileName && prevClip.FrameEnd == vclip.FrameStart + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(PPP.penActiveSealed, x1 + mouseTrimPixels, y1, x1 + mouseTrimPixels, y2);
                }
                if (uiObjects.TrimHover == TrimDirection.Right && index < proj.ClipsVideo.Count - 1)
                {
                    var nextClip = proj.ClipsVideo[index + 1];
                    if (nextClip.FileName == vclip.FileName && nextClip.FrameStart == vclip.FrameEnd + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(PPP.penActiveSealed, x1 + clipW + mouseTrimPixels, y1, x1 + clipW + mouseTrimPixels, y2);
                }
            }
		}

        /// <summary>
        /// This thing could be complex because we have
        /// </summary>
        private void DrawVideoClipBorder(Pen pen15,
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
                drawBesierFromMyBzPoints(pen15);
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
                drawBesierFromMyBzPoints(pen15);
            }
            else
                g.DrawLine(pen15, x2, yEase1, bzXMainR, yEase1);
            g.DrawLine(pen15, bzXMainL, yEase1, bzXMainR, yEase1);
        }

		public void OutlineClipAudioHover()
		{
			var aclip = uiObjects.CurrentAudioClipHover;
			int y1 = dimdim.getY_audio1(Height);
            int y2 = dimdim.getY_audio2(Height);
			//var secStart = dimdim.FrameToSec(aclip.FrameStart);
			//var secEnd = dimdim.FrameToSec(aclip.FrameEnd);
			int x1 = dimdim.convert_Frame2ScreenX(aclip.FrameOffset);
			int x2 = dimdim.convert_Frame2ScreenX(aclip.FrameOffset + aclip.LengthFrameCalc);
            
            //DrawOutlineOfAnyClip(x1, y1, x2 - x1, y2, y2, y2);
            // .... draw outline
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            g.DrawRectangle(PPP.penHover, x1, y1, x2 - x1, y2 - y1);
            if (uiObjects.TrimHover == TrimDirection.Left)
                drawTrimBracket(x1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
            if (uiObjects.TrimHover == TrimDirection.Right)
                drawTrimBracket(x2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
		}

		internal void DrawCurrentFrameMarker()
		{
			var y2 = Height - dimdim.getY_timeAxisHeight(Height);
			var markerX = dimdim.convert_Frame2ScreenX(uiObjects.CurrentMarkerFrame);
			g.DrawLine(PPP.penMarker, markerX, 0, markerX, y2);
			//g.DrawString("" + uiObjects.CurrentMarkerFrame, PPP.fontDefault, PPP.brushDefault, markerX, 0);
		}

		internal void DrawCurtainForOriginalPlayback()
		{
			int y1 = dimdim.getY_original2(Height);
			int y2 = Height;
			g.FillRectangle(PPP.brushHazyCurtain, 0, y1, Width, y2-y1);
		}

		public void DrawCurrentClipVideoOnOriginalTimeline()
		{
			var currentClip = uiObjects.CurrentVideoClip;
            if (currentClip == null)
                return;
			int y1 = dimdim.getY_original1(Height);
			int y2 = dimdim.getY_original2(Height);
			int yaudio = dimdim.getY_original_half(Height);
			if (!currentClip.HasAudio)
				y2 = yaudio;

			// calculations for current (selected) clip to fill in the rect
			int xOrigEaseL = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart, currentClip.FileLengthFrames, Width);
            int xOrigEaseR = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd, currentClip.FileLengthFrames, Width);
            int xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart + currentClip.EasingLeft, currentClip.FileLengthFrames, Width);
            int xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd - currentClip.EasingRight, currentClip.FileLengthFrames, Width);

			// draw entire original clip (0 .. vclip.FileLength)
			g.FillRectangle(PPP.brushWhite, 0, y1, Width, y2 - y1);
            g.FillRectangle(PPP.brushActiveEased, xOrigEaseL, y1, xOrigEaseR - xOrigEaseL, y2 - y1);
            g.FillRectangle(PPP.brushActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);
			DrawClipBitmaps(
				vclip: currentClip,
				x1: 0,
				y1: y1,
				clipw: Width,
				clipvh: yaudio - y1,
				secStart: 0,
				len: currentClip.FileLengthSec ?? 0);
            var waveFile = currentClip.HasCustomAudio ? currentClip.CustomAudioFilename : currentClip.FileName;
            var waveOffset = currentClip.HasCustomAudio ? currentClip.CustomAudioOffset : 0;
            var waveLength = currentClip.HasCustomAudio ? currentClip.CustomAudioLengthSec : currentClip.FileLengthSec;
            DrawWaveform(waveFile, waveLength ?? 0, currentClip.FileLengthSec ?? 0, waveOffset,
                0, yaudio, Width, y2 - yaudio,
                0, currentClip.FileLengthSec ?? 0,
                currentClip.IsMuted, currentClip.HasCustomAudio);
			g.DrawLine(PPP.penGray, 0, yaudio, Width, yaudio);
			g.DrawRectangle(PPP.penDefault, 0, y1, Width, y2 - y1);

			// draw clip bounds and diagonals (where they are)
			foreach (var vclip in uiObjects.CurClipAllUsagesVideo)
			{
                xOrigEaseL = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart, vclip.FileLengthFrames, Width);
                xOrigEaseR = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd, vclip.FileLengthFrames, Width);
                xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart + vclip.EasingLeft, vclip.FileLengthFrames, Width);
                xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd - vclip.EasingRight, vclip.FileLengthFrames, Width);
				var xMain1 = dimdim.getScreenX1_video(vclip);
				var xMain2 = xMain1 + dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); //hacky, I know
				int yMainTop = dimdim.getY_main1(Height);
				int xMainDelta = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta); //hacky, I know
				int xOrigDelta = dimdim.convert_Frame2ScreenX_OriginalTimeline(uiObjects.MouseDragFrameDelta, currentClip.FileLengthFrames, Width); // hacky, I know
                var xEaseLeft = dimdim.convert_FrameToAbsX(vclip.EasingLeft);
                var xEaseRight = dimdim.convert_FrameToAbsX(vclip.EasingRight);

				var type = (vclip == uiObjects.CurrentVideoClipHover)
					? OutlineClipType.Hover
					: OutlineClipType.Active;
				g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain1, yMainTop, xOrig1, y2);
				g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain2, yMainTop, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig1, y1, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig2, y1, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penGray : PPP.penGray, xOrigEaseL, y1, xOrigEaseL, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penGray : PPP.penGray, xOrigEaseR, y1, xOrigEaseR, y2);
                if (type == OutlineClipType.Hover && !uiObjects.MouseDragFrameDeltaMTO)
				{
					if (uiObjects.TrimHover == TrimDirection.Left)
					{
                        if (uiObjects.ShowEasingHandles)
                            g.DrawLine(PPP.penActiveBoundary, xOrigEaseL, y1, xOrigEaseL, y2);
                        else
                        {
                            g.DrawLine(PPP.penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
                            drawTrimBracket(xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
                        }
					}
					if (uiObjects.TrimHover == TrimDirection.Right)
					{
                        if (uiObjects.ShowEasingHandles)
                            g.DrawLine(PPP.penActiveBoundary, xOrigEaseR, y1, xOrigEaseR, y2);
                        else
                        {
                            g.DrawLine(PPP.penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
                            drawTrimBracket(xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
                        }
					}
				}
			}
			
			// draw marker on 
			var frameOffset = uiObjects.OriginalTimelinePlaybackMode
				? uiObjects.CurrentMarkerFrame
				: uiObjects.CurrentMarkerFrame - (uiObjects.CurrentClipFrameAbsPos ?? 0) + currentClip.FrameStartNoEase;
			int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, Width);
			g.DrawLine(PPP.penMarker, xMarker, y1, xMarker, y2);
		}

		public void DrawCurrentClipAudioOnOriginalTimeline()
		{
            var currentClip = uiObjects.CurrentAudioClip;
            if (currentClip == null)
                return;
            int y1 = dimdim.getY_original1(Height);
            int y2 = dimdim.getY_original2(Height);

            // calculations for current (selected) clip to fill in the rect
            int xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart, currentClip.FileLengthFrames, Width);
            int xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd, currentClip.FileLengthFrames, Width);

            // draw entire original clip (0 .. vclip.FileLength)
            DrawWaveform(currentClip.FileName, currentClip.FileLengthSec ?? 0, currentClip.FileLengthSec ?? 0, 0,
                0, y1, Width, y2 - y1,
                0, currentClip.FileLengthSec ?? 0,
                false, false);
            // outline
            g.DrawRectangle(PPP.penDefault, 0, y1, Width, y2 - y1);
            // current active
            g.FillRectangle(PPP.brushHazyActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);

            // draw clip bounds and diagonals (where they are)
            foreach (var vclip in uiObjects.CurClipAllUsagesAudio)
            {
                xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart, vclip.FileLengthFrames, Width);
                xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd, vclip.FileLengthFrames, Width);
                var xMain1 = dimdim.convert_Frame2ScreenX(vclip.FrameOffset);
                var xMain2 = xMain1 + dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); //hacky, I know
                int yMainTop = dimdim.getY_audio1(Height);
                int xMainDelta = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta); //hacky, I know
                int xOrigDelta = dimdim.convert_Frame2ScreenX_OriginalTimeline(uiObjects.MouseDragFrameDelta, currentClip.FileLengthFrames, Width); // hacky, I know

                var type = (vclip == uiObjects.CurrentAudioClipHover)
                    ? OutlineClipType.Hover
                    : OutlineClipType.Active;
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain1, yMainTop, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xMain2, yMainTop, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig1, y1, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? PPP.penHover : PPP.penGray, xOrig2, y1, xOrig2, y2);
                if (type == OutlineClipType.Hover)
                {
                    if (uiObjects.TrimHover == TrimDirection.Left)
                    {
                        g.DrawLine(PPP.penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
                        drawTrimBracket(xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                    if (uiObjects.TrimHover == TrimDirection.Right)
                    {
                        g.DrawLine(PPP.penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
                        drawTrimBracket(xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                }
            }

            // draw marker on 
            var frameOffset = uiObjects.OriginalTimelinePlaybackMode
                ? uiObjects.CurrentMarkerFrame
                : uiObjects.CurrentMarkerFrame - (currentClip.FrameOffset) + currentClip.FrameStart;
            int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, Width);
            g.DrawLine(PPP.penMarker, xMarker, y1, xMarker, y2);
		}

		internal void DrawDraggySeparately()
		{
			var draggy = uiObjects.Draggy;
			if (draggy.Mode != EditorDraggyMode.DraggingFolder)
				return;
			var coordX = draggy.MouseX;
			var coordY = Height / 3 - 50;
			g.FillRectangle(PPP.brushHazy, coordX, coordY, 500, 100);
			g.DrawRectangle(PPP.penDefault, coordX, coordY, 500, 100);
			g.DrawString(draggy.Text, PPP.fontDefault, PPP.brushDefault, coordX, coordY+30);
		}

		#endregion

		#region ================================== helpers ===================================

        /// <summary>
        /// The last parameter, sec2X is used to porperly position the wave within the given x1..x1+clipw space if it is too short
        /// </summary>
		private void DrawWaveform(
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

            string waveFile = fileMapping.AddGetWaveFilenameJpg(mediaFilename);
			if (File.Exists(waveFile))
			{
				Bitmap bmpWave = imgCache.getWaveImg(waveFile);//Image ot

                double audioT1L = 0, audioT2R = 0;
                if (audioT1 < 0) {
                    audioT1L = -audioT1;
                    audioT1 = 0;
                }
                if (audioT2 > 1) {
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
		private void DrawClipBitmaps(
			VidkaClipVideoAbstract vclip,
			int x1, int y1, int clipw, int clipvh,
			double secStart, double len)
		{
			string thumbsFile = fileMapping.AddGetThumbnailFilename(vclip.FileName);
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

		/// <summary>
		/// Draws one red bracket if drag frames = 0. If there has been a drag > 0,
		/// draws 2 brackets: one purple for original edge, one red for active (under mouse)
		/// </summary>
		private void drawTrimBracket(int x, int y1, int y2, TrimDirection trimDirection, int bracketLength, int trimDeltaX,
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
				g.FillRectangle(PPP.brushHazy, Math.Min(x, x + trimDeltaX), y1, Math.Abs(trimDeltaX), y2-y1);
                drawTrimBracketSingle(g, penActivePrev, x, y1, y2, trimDirection, bracketLength);
                drawTrimBracketSingle(g, penActive, x + trimDeltaX, y1, y2, trimDirection, bracketLength);
			}
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

		#region helpers and shit
        
        private void drawBesierFromMyBzPoints(Pen penGray)
        {
            g.DrawBezier(penGray, bzP1, bzP1c, bzP2c, bzP2);
        }

		#endregion

		#endregion

		#region ================================== AlignVideoAudio ===================================

		internal void AlignVideoAudio_drawVideo(VidkaClipVideoAbstract vclip)
		{
			var y1 = (int)(Height * 0.1);
			var cliph = (int)(Height * 0.5);
            var clipvh = (int)(Height * 0.3);
            var yEase = (int)(Height * 0.7);
            drawVideoClip(vclip, null, 0, y1, cliph, clipvh, yEase, PPP.brushWhite);
		}

        internal void AlignVideoAudio_drawAudio(VidkaClipAudio aclip, float audioOffsetSec)
        {
            if (!aclip.FileLengthSec.HasValue || aclip.FileLengthSec == 0)
                return;
            if (string.IsNullOrEmpty(aclip.FileName))
                return;
            var yaudio = (int)(Height * 0.6);
            var y2 = (int)(Height * 0.9);
            int x1 = dimdim.convert_Frame2ScreenX(0);
            int x2 = dimdim.convert_Frame2ScreenX(0 + aclip.LengthFrameCalc);
            int xOffset = dimdim.convert_SecToAbsX(audioOffsetSec);
            DrawWaveform(aclip.FileName, aclip.FileLengthSec ?? 0, aclip.FileLengthSec ?? 0, 0, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio, 0, aclip.FileLengthSec ?? 0, false, false);
            g.DrawRectangle(PPP.penDefault, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio);
        }

		#endregion
    }
}
