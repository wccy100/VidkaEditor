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
		#region =============== native extensions ===================

		public static void AddUnique<T>(this List<T> list, T obj) {
			if (list.Contains(obj))
				return;
			list.Add(obj);
		}

		public static string StringJoin(this IEnumerable<string> list, string separator) {
			return string.Join(separator, list);
		}

		public static bool AreAllTheSame<T>(this IEnumerable<T> list, Func<T, T, bool> comparator)
		{
			if (list.Count() <= 1)
				return true;
			var first = list.FirstOrDefault();
			var oneDifferent = list.Skip(1).Any(y => !comparator(first, y));
			return !oneDifferent;
		}

        public static IEnumerable<T> DistinctHaving<T, TK>(this IEnumerable<T> list, Func<T, TK> comparator)
		{
            return list.GroupBy(comparator).Select(x => x.FirstOrDefault());
		}

		public static bool ReplaceElement<T>(this List<T> list, T objOld, T objNew)
		{
			if (!list.Contains(objOld))
				return false;
			var index = list.IndexOf(objOld);
			list.Remove(objOld);
			list.Insert(index, objNew);
			return true;
		}

		public static string ToString_MinuteOrHour(this TimeSpan ts) {
			return ts.ToString((ts.TotalHours >= 1) ? @"hh\:mm\:ss" : @"mm\:ss");
		}

		#endregion

		#region =============== editing helpers ===================

		public static void SetFrameMarker_LeftOfVClip(this ISomeCommonEditorOperations iEditor, VidkaClipVideoAbstract vclip, VidkaProj proj)
		{
			long frameMarker = proj.GetVideoClipAbsFramePositionLeft(vclip);
			iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
		}

		public static void SetFrameMarker_RightOfVClipJustBefore(this ISomeCommonEditorOperations iEditor, VidkaClipVideoAbstract vclip, VidkaProj proj)
		{
			long frameMarker = proj.GetVideoClipAbsFramePositionLeft(vclip);
			var rightThreshFrames = proj.SecToFrame(Settings.Default.RightTrimMarkerOffsetSeconds);
			// if clip is longer than RightTrimMarkerOffsetSeconds, we can skip to end-RightTrimMarkerOffsetSeconds
			if (vclip.LengthFrameCalc > rightThreshFrames)
				frameMarker += vclip.LengthFrameCalc - rightThreshFrames;
			iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
		}

        public static void SetFrameMarker_LeftOfAClip(this ISomeCommonEditorOperations iEditor, VidkaClipAudio clip)
        {
            iEditor.SetFrameMarker_ShowFrameInPlayer(clip.FrameOffset);
        }

        public static void SetFrameMarker_RightOfAClipJustBefore(this ISomeCommonEditorOperations iEditor, VidkaClipAudio clip, VidkaProj proj)
        {
            long frameMarker = clip.FrameOffset; // start
            var rightThreshFrames = proj.SecToFrame(Settings.Default.RightTrimMarkerOffsetSeconds);
            // if clip is longer than RightTrimMarkerOffsetSeconds, we can skip to end-RightTrimMarkerOffsetSeconds
            if (clip.LengthFrameCalc > rightThreshFrames)
                frameMarker += clip.LengthFrameCalc - rightThreshFrames;
            iEditor.SetFrameMarker_ShowFrameInPlayer(frameMarker);
        }

		#endregion
	}
}
