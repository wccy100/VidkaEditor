using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core.Properties;

namespace Vidka.Core
{
	public static class Utils
	{
		#region =============== editing helpers ===================

		public static void SetFrameMarker_LeftOfVClip(this IVidkaOpContext iEditor, VidkaClipVideoAbstract vclip, VidkaProj proj)
		{
			long frameMarker = proj.GetVideoClipAbsFramePositionLeft(vclip);
			iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
		}

		public static void SetFrameMarker_RightOfVClipJustBefore(this IVidkaOpContext iEditor, VidkaClipVideoAbstract vclip, VidkaProj proj)
		{
			long frameMarker = proj.GetVideoClipAbsFramePositionLeft(vclip);
			var rightThreshFrames = proj.SecToFrame(Settings.Default.RightTrimMarkerOffsetSeconds);
			// if clip is longer than RightTrimMarkerOffsetSeconds, we can skip to end-RightTrimMarkerOffsetSeconds
			if (vclip.LengthFrameCalc > rightThreshFrames)
				frameMarker += vclip.LengthFrameCalc - rightThreshFrames;
			iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
		}

        public static void SetFrameMarker_LeftOfAClip(this IVidkaOpContext iEditor, VidkaClipAudio clip)
        {
            iEditor.SetFrameMarker_ShowFrameInPlayer(clip.FrameOffset);
        }

        public static void SetFrameMarker_RightOfAClipJustBefore(this IVidkaOpContext iEditor, VidkaClipAudio clip, VidkaProj proj)
        {
            long frameMarker = clip.FrameOffset; // start
            var rightThreshFrames = proj.SecToFrame(Settings.Default.RightTrimMarkerOffsetSeconds);
            // if clip is longer than RightTrimMarkerOffsetSeconds, we can skip to end-RightTrimMarkerOffsetSeconds
            if (clip.LengthFrameCalc > rightThreshFrames)
                frameMarker += clip.LengthFrameCalc - rightThreshFrames;
            iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
        }

		#endregion

        #region =============== misc helpers ===================

        public static bool IsLRShiftKey(this Keys key)
        {
            return key == (Keys.LButton | Keys.ShiftKey)
                || key == (Keys.RButton | Keys.ShiftKey);
        }

        #endregion

    }
}
