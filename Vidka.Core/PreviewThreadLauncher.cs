using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vidka.Core.Model;

namespace Vidka.Core
{

	public class PreviewThreadMutex {
		public PreviewThreadMutex() {
			IsPlaying = false;
		}
		public bool IsPlaying { get; set; }
		public VidkaProj Proj { get; set; }
		public int CurClipIndex { get; set; }
		public long CurFrame { get; set; }
		public double CurPlayerStartPositionSec { get; set; }
		public double CurStopPositionSec { get; set; }
		// both of these used for frame marker positioning
		public long CurClipMarkerStartPos { get; set; }
		//public long CurClipStartFrame { get; set; }

		public bool OnlyLockedClips { get; set; }
	}

	public class PreviewThreadLauncher
	{
		private const double SECONDS_PAUSE_MIN = 0.2;
		private const double SECONDS_PAUSE_MAX = 0.3;
		private const double STOP_BEFORE_THRESH = 1/30.0;
		private IVideoPlayer player;
        private IAudioPlayer playerAudio;
        private IVidkaOpContext editor;
		private PreviewThreadMutex mutex;
		private Timer ticker;

		//current state


        public PreviewThreadLauncher(IVideoPlayer player, IAudioPlayer playerAudio, IVidkaOpContext editor)
        {
            this.player = player;
            this.playerAudio = playerAudio;
			this.editor = editor;
			mutex = new PreviewThreadMutex();
			ticker = new Timer();
			ticker.Tick += PlaybackTickerFunc;
		}

		// called when swappng players for fast mode
		public void SetPreviewPlayer(IVideoPlayer videoPlayer)
		{
			this.player = videoPlayer;
		}

		public void StartPreviewPlayback(VidkaProj proj, long frameStart, bool onlyLockedClips)
		{
			lock (mutex)
			{
				// ... what we are going to play
				long frameOffset;
				var curClipIndex = proj.GetVideoClipIndexAtFrame(frameStart, out frameOffset);
				var clip = onlyLockedClips
					? proj.GetNextLockedVideoClipStartingAtIndex(curClipIndex, out curClipIndex)
					: proj.GetVideoClipAtIndex(curClipIndex);
				if (clip == null) {
					editor.AppendToConsole(VidkaConsoleLogLevel.Info, "Nothing to play!");
					return;
				}
				// ... set up mutex
				mutex.Proj = proj;
				mutex.OnlyLockedClips = onlyLockedClips;
				mutex.IsPlaying = true;
				mutex.CurClipIndex = curClipIndex;
				mutex.CurFrame = frameStart;
                // ... setup audio player with all possible clips that may come up
                InitAudioData(proj);
                // ... start playback now
				StartPlaybackOfClip(clip, frameOffset);
				// ... set up ticker
				ticker.Interval = (int)(1000 * proj.FrameToSec(1)); // 1 tick per frame... its a hack but im too lazy
				ticker.Start();
				editor.AppendToConsole(VidkaConsoleLogLevel.Debug, "StartPlayback");
			}
		}

        private void InitAudioData(VidkaProj proj)
        {
            foreach (var clip in proj.ClipsAudio)
            {
                var secOffset = mutex.Proj.FrameToSec(clip.FrameOffset);
                var secStart = mutex.Proj.FrameToSec(clip.FrameStartNoEase);
                var secEnd = mutex.Proj.FrameToSec(clip.FrameEnd);
                playerAudio.AddAudioClip(secOffset, secStart, secEnd, clip.FileName, clip);
            }
            long curFrame = 0;
            foreach (var clip in proj.ClipsVideo)
            {
                var secOffset = mutex.Proj.FrameToSec(curFrame);
                var secStart = mutex.Proj.FrameToSec(clip.FrameStartNoEase);
                var secEnd = mutex.Proj.FrameToSec(clip.FrameEndNoEase);
                if (clip.HasCustomAudio)
                    playerAudio.AddAudioClip(
                        secOffset,
                        clip.CustomAudioOffset + secStart,
                        clip.CustomAudioOffset + secEnd,
                        clip.CustomAudioFilename,
                        clip);
                curFrame += clip.LengthFrameCalc;
            }
        }

		private void PlaybackTickerFunc(object sender, EventArgs e)
		{
			lock (mutex)
			{
				mutex.CurFrame++;
				var secCurClip = player.GetPositionSec();
				var frameMarkerPosition = mutex.CurClipMarkerStartPos + mutex.Proj.SecToFrame(secCurClip - mutex.CurPlayerStartPositionSec);
				editor.SetFrameMarker_ForceRepaint(frameMarkerPosition);
				if (secCurClip >= mutex.CurStopPositionSec - STOP_BEFORE_THRESH || player.IsStopped())
				{
					var newIndex = mutex.CurClipIndex + 1;
					var clip = mutex.OnlyLockedClips
						? mutex.Proj.GetNextLockedVideoClipStartingAtIndex(newIndex, out newIndex)
						: mutex.Proj.GetVideoClipAtIndex(newIndex);
					mutex.CurClipIndex = newIndex;
					if (clip == null)
					{
						StopPlayback();
					}
					else
					{
						StartPlaybackOfClip(clip);
						editor.AppendToConsole(VidkaConsoleLogLevel.Debug, "Next clip: " + mutex.CurClipIndex);
					}
				}
                var curAbsSec = mutex.Proj.FrameToSec(frameMarkerPosition);
                playerAudio.WeAreHereStartPlaying(curAbsSec);
			}

		}

		private void StartPlaybackOfClip(VidkaClipVideoAbstract clip, long? frameOffsetCustom = null)
		{
			mutex.CurClipMarkerStartPos = mutex.Proj.GetVideoClipAbsFramePositionLeft(clip);
			if (frameOffsetCustom.HasValue)
				mutex.CurClipMarkerStartPos += frameOffsetCustom.Value - clip.FrameStartNoEase;
			var curAbsSec = mutex.Proj.FrameToSec(mutex.CurClipMarkerStartPos);
			var ppFrameStart = clip.GetPlaybackFrameStart(frameOffsetCustom);
			var ppFrameEnd = clip.GetPlaybackFrameEnd(frameOffsetCustom);
			var clipSecStart = mutex.Proj.FrameToSec(ppFrameStart);
			var clipSecEnd = mutex.Proj.FrameToSec(ppFrameEnd);
			mutex.CurPlayerStartPositionSec = clipSecStart;
			mutex.CurStopPositionSec = clipSecEnd;
			editor.SetCurrentVideoClip_ForceRepaint(clip);
            var doMute = (clip.HasCustomAudio || clip.IsMuted);
            //if (clip.HasCustomAudio)
            //    playerAudio.PlayAudioClip(clip.CustomAudioFilename, clip.CustomAudioOffset + clipSecStart, clip.CustomAudioOffset + clipSecEnd);
			player.PlayVideoClip(clip.FileName, clipSecStart, clipSecEnd, doMute);
            playerAudio.PauseAll();
            playerAudio.WeAreHereStartPlaying(curAbsSec);
		}

		public void StopPlayback()
		{
			lock (mutex)
			{
				ticker.Stop();
				mutex.IsPlaying = false;
				player.StopWhateverYouArePlaying();
                playerAudio.StopWhateverYouArePlaying();
                playerAudio.Clear();
				editor.AppendToConsole(VidkaConsoleLogLevel.Debug, "StopPlayback");
			}

			//curThread
			
		}

		internal void SplitPerformedIncrementClipIndex()
		{
			lock (mutex)
			{
				mutex.CurClipIndex++;
			}
		}

		public bool IsPlaying { get { return mutex.IsPlaying; } }


    }
}
