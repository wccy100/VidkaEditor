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
using Vidka.Core.Ops;

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
		private Pen penDefault = new Pen(Color.Black, 1); // new Pen(Color.FromArgb(255, 30, 30, 30), 1);
		private Pen penBorder = new Pen(Color.Black, 1);
		private Pen penMarker = new Pen(Color.Black, 2);
		private Pen penBorderDrag = new Pen(Color.Blue, 5);
		private Pen penHover = new Pen(Color.Blue, 4);
		private Pen penActiveClip = new Pen(Color.LightBlue, 6);
		private Pen penActiveBoundary = new Pen(Color.Red, 6);
		private Pen penActiveBoundaryPrev = new Pen(Color.Purple, 6);
		private Pen penActiveSealed = new Pen(Color.LawnGreen, 6);
		private Pen penSealed = new Pen(Color.LawnGreen, 4); // marks the split line when prev.frameEnd == cur.frameStart
		private Pen penGray = new Pen(Color.Gray, 1);
		private Brush brushDefault = new SolidBrush(Color.Black);
		private Brush brushLightGray = new SolidBrush(Color.FromArgb(unchecked((int)0xFFfbfbfb)));
		private Brush brushLightGray2 = new SolidBrush(Color.FromArgb(unchecked((int)0xFFf5f5f5)));
		private Brush brushLightGray3 = new SolidBrush(Color.FromArgb(unchecked((int)0xFFeeeeee)));
		private Brush brushActive = new SolidBrush(Color.LightBlue);
		private Brush brushLockedClip = new SolidBrush(Color.Beige);
		private Brush brushLockedActiveClip = new SolidBrush(Color.DarkKhaki);
		private Brush brushWhite = new SolidBrush(Color.White);
        private Brush brushHazy = new SolidBrush(Color.FromArgb(200, 230, 230, 230));
        private Brush brushHazyActive = new SolidBrush(Color.FromArgb(90, 0xAD, 0xD8, 0xE6));
		private Brush brushHazyCurtain = new SolidBrush(Color.FromArgb(200, 245, 245, 245)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
        private Brush brushHazyMute = new SolidBrush(Color.FromArgb(200, 200, 200, 200)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
        private Brush brushHazyCustomAudio = new SolidBrush(Color.FromArgb(70, 255, 200, 240)); //new SolidBrush(Color.FromArgb(200, 180, 180, 180));
		private Font fontDefault = SystemFonts.DefaultFont;

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

		public EditorDrawOps()
		{
			// init
			destRect = new Rectangle();
			srcRect = new Rectangle();
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
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? brushLightGray2 : brushLightGray, 0, yMain1, Width, yMainHalf - yMain1);
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Main) ? brushLightGray3 : brushLightGray2, 0, yMainHalf, Width, yMain2 - yMainHalf);
			g.FillRectangle((hover == ProjectDimensionsTimelineType.Audios) ? brushLightGray3 : brushLightGray2, 0, yAudio1, Width, yAudio2 - yAudio1);
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

			g.DrawLine(penGray, 0, y1, Width, y1);
			for (var i = 0; i < actualNSegments+1; i++)
			{
				var curSecond = startingSecond + i * secondsPerSegment;
				var scrX = dimdim.convert_Sec2ScreenX(curSecond);
				var ts = TimeSpan.FromSeconds(curSecond);
				g.DrawLine(penGray, scrX, y1, scrX, y2);
				g.DrawString(ts.ToString_MinuteOrHour(), fontDefault, brushDefault, scrX+2, y1+4);
			}
			if (secondsPerSegment == 1 && framesPerSegment <= proj.FrameRate)
			{
				// draw frame ticks as well
				for (var i = frameStart; i < frameEnd; i++) {
					var scrX = dimdim.convert_Frame2ScreenX(i);
					g.DrawLine(penGray, scrX, y1, scrX, y1+3);
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
						var brush = brushWhite;
						if (vclip == currentVideoClip && vclip.IsLocked)
							brush = brushLockedActiveClip;
						else if (vclip == currentVideoClip)
							brush = brushActive;
						else if (vclip.IsLocked)
							brush = brushLockedClip;
						drawVideoClip(vclip, vclipPrev,
							curFrame,
							y1, cliph, clipvh,
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
			int y1, int cliph, int clipvh,
			Brush brushClip)
		{
			int x1 = dimdim.convert_Frame2ScreenX(curFrame);
			int x2 = dimdim.convert_Frame2ScreenX(curFrame + vclip.LengthFrameCalc);
			int clipw = x2 - x1;

			// active video clip deserves a special outline, fill white otherwise to hide gray background
			g.FillRectangle(brushClip, x1, y1, clipw, clipvh);
			DrawClipBitmaps(
				vclip: vclip,
				x1: x1,
				y1: y1,
				clipw: clipw,
				clipvh: clipvh,
				secStart: proj.FrameToSec(vclip.FrameStart),
				len: proj.FrameToSec(vclip.LengthFrameCalc));
            if (vclip.HasAudio || vclip.HasCustomAudio)
            {
                var waveFile = vclip.HasCustomAudio ? vclip.CustomAudioFilename : vclip.FileName;
                var waveOffset = vclip.HasCustomAudio ? vclip.CustomAudioOffset : 0;
                var waveLength = vclip.HasCustomAudio ? vclip.CustomAudioLengthSec : vclip.FileLengthSec;
                DrawWaveform(waveFile, waveLength ?? 0, vclip.FileLengthSec ?? 0, waveOffset, x1, y1 + clipvh, clipw, cliph - clipvh,
					proj.FrameToSec(vclip.FrameStart), proj.FrameToSec(vclip.FrameEnd));
				if (vclip.IsMuted)
					g.FillRectangle(brushHazyMute, x1, y1 + clipvh, x2 - x1, cliph - clipvh);
                if (vclip.HasCustomAudio)
                    g.FillRectangle(brushHazyCustomAudio, x1, y1 + clipvh, x2 - x1, cliph - clipvh);
				// waveform separator
                //g.DrawLine(penGray, x1, y1 + clipvh, x2, y1 + clipvh);
                g.DrawRectangle(penGray, x1, y1 + clipvh, x2 - x1, cliph - clipvh);
			}
			// outline rect
            g.DrawRectangle(penDefault, x1, y1, clipw, vclip.HasAudio ? cliph : clipvh);
			// if vclipPrev.end == vclip.start and they are same file, mark green indicator
			if (vclipPrev != null && vclipPrev.FileName == vclip.FileName && vclipPrev.FrameEnd == vclip.FrameStart) {
				g.DrawLine(penSealed, x1, y1 + 10, x1, y1 + clipvh);
			}
			// still analyzing...
			if (vclip.IsNotYetAnalyzed)
				g.DrawString("Still analyzing...", fontDefault, brushDefault, x1+5, y1+5);
		}

		private void drawDraggyVideo(long curFrame, int y1, int cliph, int clipvh, EditorDraggy draggy)
		{
			var draggyX = dimdim.convert_Frame2ScreenX(curFrame);
			var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
			var draggyH = (draggy.HasAudio) ? cliph : clipvh;
			if (draggy.VideoClip != null)
			{
				g.FillRectangle(brushWhite, draggyX, y1, draggyW, draggyH);
				g.FillRectangle(brushActive, draggyX, y1, draggyW, clipvh);
			}
			g.DrawRectangle(penBorderDrag, draggyX, y1, draggyW, draggyH);
			g.DrawString(draggy.Text, fontDefault, brushDefault, draggyX + 5, y1 + 5);
			
			// debug rect
			//g.DrawRectangle(penDefault, draggy.MouseX-draggy.MouseXOffset, y1-2, draggyW, cliph+5);
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
                        proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd));

					//throw new NotImplementedException("DrawWaveform that takes Audio clip!!!");
					//DrawWaveform(g, proj, aclip, x1, y1, clipw, cliph,
					//	proj.FrameToSec(aclip.FrameStart), proj.FrameToSec(aclip.FrameEnd));

                    // active video clip deserves a special outline
                    if (aclip == uiObjects.CurrentAudioClip)
                        g.FillRectangle(brushHazyActive, x1, y1, clipw, cliph);

					// outline rect
					g.DrawRectangle(penDefault, x1, y1, clipw, cliph);
				}
			}
			if (draggy.Mode == EditorDraggyMode.AudioTimeline)
			{
				var draggyX = draggy.MouseX - draggy.MouseXOffset;
				var draggyW = dimdim.convert_FrameToAbsX(draggy.FrameLength); // hacky, i know
				g.DrawRectangle(penBorderDrag, draggyX, y1, draggyW, cliph);
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
			destRect.X = xCenter - destRect.Width / 2;
			destRect.Y = yCenter - destRect.Height / 2;
			g.DrawImage(bmpThumb, destRect: destRect, srcRect: srcRect, srcUnit: GraphicsUnit.Pixel);
		}

		public void DrawBorder(Graphics g, int Width, int Height)
		{
			g.DrawRectangle(penBorder, 0, 0, Width - 1, Height - 1);
		}

		public void OutlineClipVideoHover()
		{
			var vclip = uiObjects.CurrentVideoClipHover;

			int y1 = dimdim.getY_main1(Height);
			int y2 = vclip.HasAudio
				? dimdim.getY_main2(Height)
				: dimdim.getY_main_half(Height);
			//int yaudio = dimdim.getY_main_half(Height);
			int x1 = dimdim.getScreenX1_video(vclip);
			int clipW = dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); // hacky, I know
            DrawOutlineOfAnyClip(x1, y1, clipW, y2);
            // draw seal when prev/next clip match
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            if (uiObjects.TrimHover != TrimDirection.None)
            {
                var index = proj.ClipsVideo.IndexOf(vclip);
                if (uiObjects.TrimHover == TrimDirection.Left && index > 0)
                {
                    var prevClip = proj.ClipsVideo[index - 1];
                    if (prevClip.FileName == vclip.FileName && prevClip.FrameEnd == vclip.FrameStart + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(penActiveSealed, x1 + mouseTrimPixels, y1, x1 + mouseTrimPixels, y2);
                }
                if (uiObjects.TrimHover == TrimDirection.Right && index < proj.ClipsVideo.Count - 1)
                {
                    var nextClip = proj.ClipsVideo[index + 1];
                    if (nextClip.FileName == vclip.FileName && nextClip.FrameStart == vclip.FrameEnd + uiObjects.MouseDragFrameDelta)
                        g.DrawLine(penActiveSealed, x1 + clipW + mouseTrimPixels, y1, x1 + clipW + mouseTrimPixels, y2);
                }
            }
		}

        private void DrawOutlineOfAnyClip(int x1, int y1, int clipW, int y2)
        {
            var mouseTrimPixels = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta);
            g.DrawRectangle(penHover, x1, y1, clipW, y2 - y1);
            if (uiObjects.TrimHover == TrimDirection.Left)
                drawTrimBracket(x1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, mouseTrimPixels);
            if (uiObjects.TrimHover == TrimDirection.Right)
                drawTrimBracket(x1 + clipW, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, mouseTrimPixels);
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
            DrawOutlineOfAnyClip(x1, y1, x2-x1, y2);
		}

		internal void DrawCurrentFrameMarker()
		{
			var y2 = Height - dimdim.getY_timeAxisHeight(Height);
			var markerX = dimdim.convert_Frame2ScreenX(uiObjects.CurrentMarkerFrame);
			g.DrawLine(penMarker, markerX, 0, markerX, y2);
			//g.DrawString("" + uiObjects.CurrentMarkerFrame, fontDefault, brushDefault, markerX, 0);
		}

		internal void DrawCurtainForOriginalPlayback()
		{
			int y1 = dimdim.getY_original2(Height);
			int y2 = Height;
			g.FillRectangle(brushHazyCurtain, 0, y1, Width, y2-y1);
		}

		public void DrawOriginalTimelineAndItsClipOrClips()
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
			int xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameStart, currentClip.FileLengthFrames, Width);
			int xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(currentClip.FrameEnd, currentClip.FileLengthFrames, Width);

			// draw entire original clip (0 .. vclip.FileLength)
			g.FillRectangle(brushWhite, 0, y1, Width, y2 - y1);
			g.FillRectangle(brushActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);
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
                0, currentClip.FileLengthSec ?? 0);
			if (currentClip.IsMuted)
				g.FillRectangle(brushHazyMute, xOrig1, yaudio, xOrig2 - xOrig1, y2 - yaudio);
            if (currentClip.HasCustomAudio)
                g.FillRectangle(brushHazyCustomAudio, xOrig1, yaudio, xOrig2 - xOrig1, y2 - yaudio);
			g.DrawLine(penGray, 0, yaudio, Width, yaudio);
			g.DrawRectangle(penDefault, 0, y1, Width, y2 - y1);

			// draw clip bounds and diagonals (where they are)
			foreach (var vclip in uiObjects.CurClipAllUsagesVideo)
			{
				xOrig1 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameStart, vclip.FileLengthFrames, Width);
				xOrig2 = dimdim.convert_Frame2ScreenX_OriginalTimeline(vclip.FrameEnd, vclip.FileLengthFrames, Width);
				var xMain1 = dimdim.getScreenX1_video(vclip);
				var xMain2 = xMain1 + dimdim.convert_FrameToAbsX(vclip.LengthFrameCalc); //hacky, I know
				int yMainTop = dimdim.getY_main1(Height);
				int xMainDelta = dimdim.convert_FrameToAbsX(uiObjects.MouseDragFrameDelta); //hacky, I know
				int xOrigDelta = dimdim.convert_Frame2ScreenX_OriginalTimeline(uiObjects.MouseDragFrameDelta, currentClip.FileLengthFrames, Width); // hacky, I know

				var type = (vclip == uiObjects.CurrentVideoClipHover)
					? OutlineClipType.Hover
					: OutlineClipType.Active;
				g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xMain1, yMainTop, xOrig1, y2);
				g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xMain2, yMainTop, xOrig2, y2);
				g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xOrig1, y1, xOrig1, y2);
				g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xOrig2, y1, xOrig2, y2);
				if (type == OutlineClipType.Hover)
				{
					if (uiObjects.TrimHover == TrimDirection.Left)
					{
						g.DrawLine(penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
						drawTrimBracket(xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
					}
					if (uiObjects.TrimHover == TrimDirection.Right)
					{
						g.DrawLine(penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
						drawTrimBracket(xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
					}
				}
			}
			
			// draw marker on 
			var frameOffset = uiObjects.OriginalTimelinePlaybackMode
				? uiObjects.CurrentMarkerFrame
				: uiObjects.CurrentMarkerFrame - (uiObjects.CurrentClipFrameAbsPos ?? 0) + currentClip.FrameStart;
			int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, Width);
			g.DrawLine(penMarker, xMarker, y1, xMarker, y2);
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
                0, currentClip.FileLengthSec ?? 0);
            // outline
            g.DrawRectangle(penDefault, 0, y1, Width, y2 - y1);
            // current active
            g.FillRectangle(brushHazyActive, xOrig1, y1, xOrig2 - xOrig1, y2 - y1);

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
                g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xMain1, yMainTop, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xMain2, yMainTop, xOrig2, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xOrig1, y1, xOrig1, y2);
                g.DrawLine((type == OutlineClipType.Hover) ? penHover : penGray, xOrig2, y1, xOrig2, y2);
                if (type == OutlineClipType.Hover)
                {
                    if (uiObjects.TrimHover == TrimDirection.Left)
                    {
                        g.DrawLine(penActiveBoundary, xMain1 + xMainDelta, yMainTop, xOrig1 + xOrigDelta, y2);
                        drawTrimBracket(xOrig1, y1, y2, TrimDirection.Left, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                    if (uiObjects.TrimHover == TrimDirection.Right)
                    {
                        g.DrawLine(penActiveBoundary, xMain2 + xMainDelta, yMainTop, xOrig2 + xOrigDelta, y2);
                        drawTrimBracket(xOrig2, y1, y2, TrimDirection.Right, uiObjects.TrimThreshPixels, xOrigDelta);
                    }
                }
            }

            // draw marker on 
            var frameOffset = uiObjects.OriginalTimelinePlaybackMode
                ? uiObjects.CurrentMarkerFrame
                : uiObjects.CurrentMarkerFrame - (currentClip.FrameOffset) + currentClip.FrameStart;
            int xMarker = dimdim.convert_Frame2ScreenX_OriginalTimeline(frameOffset, currentClip.FileLengthFrames, Width);
            g.DrawLine(penMarker, xMarker, y1, xMarker, y2);
		}

		internal void DrawDraggySeparately()
		{
			var draggy = uiObjects.Draggy;
			if (draggy.Mode != EditorDraggyMode.DraggingFolder)
				return;
			var coordX = draggy.MouseX;
			var coordY = Height / 3 - 50;
			g.FillRectangle(brushHazy, coordX, coordY, 500, 100);
			g.DrawRectangle(penDefault, coordX, coordY, 500, 100);
			g.DrawString(draggy.Text, fontDefault, brushDefault, coordX, coordY+30);
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
			double secStart, double secEnd)
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
				howManyThumbs = 1;
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
		private void drawTrimBracket(int x, int y1, int y2, TrimDirection trimDirection, int bracketLength, int trimDeltaX)
		{
			if (trimDeltaX == 0)
				drawTrimBracketSingle(g, penActiveBoundary, x, y1, y2, trimDirection, bracketLength);
			else
			{
				g.FillRectangle(brushHazy, Math.Min(x, x + trimDeltaX), y1, Math.Abs(trimDeltaX), y2-y1);
				drawTrimBracketSingle(g, penActiveBoundaryPrev, x, y1, y2, trimDirection, bracketLength);
				drawTrimBracketSingle(g, penActiveBoundary, x + trimDeltaX, y1, y2, trimDirection, bracketLength);
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

		#endregion

		#region ================================== AlignVideoAudio ===================================

		internal void AlignVideoAudio_drawVideo(VidkaClipVideoAbstract vclip)
		{
			var y1 = (int)(Height * 0.1);
			var cliph = (int)(Height * 0.5);
			var clipvh = (int)(Height * 0.3);
			drawVideoClip(vclip, null, 0, y1, cliph, clipvh, brushWhite);
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
            DrawWaveform(aclip.FileName, aclip.FileLengthSec ?? 0, aclip.FileLengthSec ?? 0, 0, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio, 0, aclip.FileLengthSec ?? 0);
            g.DrawRectangle(penDefault, x1 + xOffset, yaudio, x2 - x1, y2 - yaudio);
        }

		#endregion
    }
}
