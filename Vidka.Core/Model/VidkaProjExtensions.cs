using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vidka.Core.Model
{
	public static class VidkaProjExtensions
	{
		#region ================================= helper methods for proj class ========================

		public static double FrameToSec(this VidkaProj proj, long frame) {
			return frame / proj.FrameRate;
		}
		public static int SecToFrame(this VidkaProj proj, double sec) {
			return (int)(sec * proj.FrameRate);
		}
		public static double SecToFrameDouble(this VidkaProj proj, double sec) {
			return sec * proj.FrameRate;
		}

		/// <summary>
		/// used in ProjectDimensions.recalculateProjectWidth.
		/// The other part of this calculation is GetTotalLengthOfAudioClipsSec 
		/// </summary>
		public static long GetTotalLengthOfVideoClipsFrame(this VidkaProj proj)
		{
			long totalFrames = 0;
			foreach (var ccc in proj.ClipsVideo) {
				totalFrames += ccc.LengthFrameCalc;
			}
			return totalFrames;
		}

		/// <summary>
		/// used in ProjectDimensions.recalculateProjectWidth.
		/// </summary>
		public static long GetTotalLengthOfAudioClipsFrame(this VidkaProj proj)
		{
			long maxFrame = 0;
			foreach (var ccc in proj.ClipsAudio) {
				maxFrame = Math.Max(maxFrame, ccc.FrameEnd);
			}
			return maxFrame;
		}

		/// <summary>
		/// returns null if index is out of bounds
		/// </summary>
		public static VidkaClipVideoAbstract GetVideoClipAtIndex(this VidkaProj proj, int index)
		{
			if (index < 0 || index >= proj.ClipsVideo.Count)
				return null;
			return proj.ClipsVideo[index];
		}

		/// <summary>
		/// returns clip at index, if it is locked, and, if not, looks for the next clip
		/// returns null if index is out of bounds
		/// </summary>
		public static VidkaClipVideoAbstract GetNextLockedVideoClipStartingAtIndex(this VidkaProj proj, int index, out int newIndex)
		{
			newIndex = -1;
			if (index < 0 || index >= proj.ClipsVideo.Count)
				return null;
			var clip = proj.ClipsVideo.Skip(index).FirstOrDefault(x => x.IsLocked);
			if (clip != null)
				newIndex = proj.ClipsVideo.IndexOf(clip);
			return clip;
		}

		/// <summary>
		/// returns either the first or last clip if index is out of bounds respectively.
		/// If there are no clips at all, returns null
		/// </summary>
		public static VidkaClipVideoAbstract GetVideoClipAtIndexForce(this VidkaProj proj, int index)
		{
			if (proj.ClipsVideo.Count == 0)
				return null;
			if (index < 0)
				return proj.ClipsVideo.FirstOrDefault();
			if (index >= proj.ClipsVideo.Count)
				return proj.ClipsVideo.LastOrDefault();
			return proj.ClipsVideo[index];
		}
		

		/// <summary>
		/// Returns index of the clip under the given frame (curFrame) and also how far into the clip,
		/// the marker is (out frameOffset). NOTE: frameOffset is relative to beginning of the video file,
		/// not to clip.FrameStart!
		/// If curFrame is not on any of the clips -1 is returned
		/// </summary>
		public static int GetVideoClipIndexAtFrame(this VidkaProj proj, long curFrame, out long frameOffset)
		{
			frameOffset = 0;
			long totalFrame = 0;
			int index = 0;
			foreach (var ccc in proj.ClipsVideo)
			{
				if (curFrame >= totalFrame && curFrame < totalFrame + ccc.LengthFrameCalc)
				{
					frameOffset = curFrame - totalFrame + ccc.FrameStartNoEase;
					return index;
				}
				index++;
				totalFrame += ccc.LengthFrameCalc;
			}
			return -1;
		}

		/// <summary>
		/// Returns same thing as GetVideoClipIndexAtFrame, except when the marker is too far out,
		/// it returns the last clip's index and frameOffset = clip.FrameEnd (again, relative to start of file)
		/// If there are no clips at all, returns -1 and frameOffset is 0
		/// </summary>
		public static int GetVideoClipIndexAtFrame_forceOnLastClip(this VidkaProj proj, long curFrame, out long frameOffset)
		{
			var index = proj.GetVideoClipIndexAtFrame(curFrame, out frameOffset);
			if (index == -1 && proj.ClipsVideo.Count > 0) {
				// ze forcing...
				index = proj.ClipsVideo.Count - 1;
				frameOffset = proj.ClipsVideo[index].FrameEndNoEase;
			}
			return index;
		}
		

		/// <summary>
		/// The inverse of GetVideoClipIndexAtFrame.
		/// Instead returns the frame of the clip (left side) within project absolute frame space.
		/// Returns -1 if the clip is not even in the project
		/// </summary>
		public static long GetVideoClipAbsFramePositionLeft(this VidkaProj proj, VidkaClipVideoAbstract clip)
		{
			long totalFrames = 0;
			foreach (var ccc in proj.ClipsVideo)
			{
				if (ccc == clip)
					return totalFrames;
				totalFrames += ccc.LengthFrameCalc;
			}
			return -1;
		}

		public static VidkaProj Crop(this VidkaProj proj, long frameStart, long framesLength, int? newW = null, int? newH = null, bool onlyLockedClips = false)
		{
			var newProj = new VidkaProj() {
				FrameRate = proj.FrameRate,
				Width = newW ?? proj.Width,
				Height = newH ?? proj.Height,
			};
			long frameEnd = frameStart + framesLength;
			long curFrame = 0;
			foreach (var vclip in proj.ClipsVideo) {
				var curFrame2 = curFrame + vclip.LengthFrameCalc; // abs right bound of vclip
				// outside: too early
				if (curFrame2 <= frameStart) {
					curFrame += vclip.LengthFrameCalc;
					continue;
				}
				// outside: too late
				if (curFrame >= frameEnd)
					break;
				if (onlyLockedClips && !vclip.IsLocked) {
					// we are on a the first non-locked clip of our subsequence...
					// but we skip it to the first locked clip, thus, we don't need asd1d21w to be called
					if (!newProj.ClipsVideo.Any())
						curFrame = frameStart;
					continue;
				}
				var newVClip = vclip.MakeCopy_VideoClip();
				// trim start, if neccessary (asd1d21w)
				if (curFrame < frameStart)
					newVClip.FrameStart += (frameStart - curFrame);
				// trim end, if neccessary
				if (curFrame2 > frameEnd)
					newVClip.FrameEnd -= (curFrame2 - frameEnd);
				newProj.ClipsVideo.Add(newVClip);
				curFrame += vclip.LengthFrameCalc;
			}
            foreach (var aclip in proj.ClipsAudio)
            {
                var newAClip = aclip.MakeCopy_AudioClip();
                newAClip.FrameOffset = aclip.FrameOffset - frameStart;
                if (newAClip.FrameOffset + newAClip.LengthFrameCalc < 0)
                    continue;
                if (newAClip.FrameOffset > frameStart + framesLength)
                    continue;
                if (newAClip.FrameOffset < 0)
                {
                    newAClip.FrameStart = newAClip.FrameStart - newAClip.FrameOffset;
                    newAClip.FrameOffset = 0;
                }
                newProj.ClipsAudio.Add(newAClip);
            }
            var firstStatCandidate = proj.ClipsVideo.FirstOrDefault(x => x.IsPixelTypeStandard);
            if (firstStatCandidate != null)
                newProj.PixelTypeStandardClip = firstStatCandidate;
			return newProj;
		}

		#endregion

		#region ================================= helper methods for clips ========================

		/// <summary>
		/// Returns what the delta should be not to violate the trimming of this clip
		/// </summary>
		public static long HowMuchCanBeTrimmed(this VidkaClip clip, TrimDirection side, long delta)
		{
			if (clip == null)
				return 0;
			if (side == TrimDirection.Left)
			{
                if (clip.FrameStart + delta < 0) // left bound...
					return -clip.FrameStart; // ...to make 0
                else if (clip.FrameStartNoEase + delta >= clip.FrameEndNoEase) // right bound...
					return -clip.FrameStartNoEase + clip.FrameEndNoEase - 1; // ...to make frameEndNoEase-1
				return delta;
			}
			else if (side == TrimDirection.Right)
			{
                if (clip.FrameEndNoEase + delta <= clip.FrameStartNoEase) // left bound...
					return -clip.FrameEndNoEase + clip.FrameStartNoEase + 1; // ...to male frameStartNoEase+1
                else if (clip.FrameEnd + delta >= clip.FileLengthFrames) // right bound...
					return -clip.FrameEnd + clip.FileLengthFrames; // ...to make clip.LengthFrameCalc
				return delta;
			}
			return 0;
		}

        public static long HowMuchCanBeEased(this VidkaClipVideoAbstract clip, TrimDirection side, long delta)
        {
            if (clip == null)
                return 0;
            if (side == TrimDirection.Left)
            {
                var deltaBoundPositive = clip.LengthFrameCalcNoEase - 1 - clip.EasingLeft - clip.EasingRight;
                var deltaBoundNegative = clip.EasingLeft;
                if (delta > 0 && delta > deltaBoundNegative)
                    return deltaBoundNegative;
                else if (delta < 0 && delta < -deltaBoundPositive)
                    return -deltaBoundPositive;
                return delta;
            }
            else if (side == TrimDirection.Right)
            {
                var deltaBoundPositive = clip.LengthFrameCalcNoEase - 1 - clip.EasingLeft - clip.EasingRight;
                var deltaBoundNegative = clip.EasingRight;
                if (delta < 0 && delta < -deltaBoundNegative)
                    return -deltaBoundNegative;
                else if (delta > 0 && delta > deltaBoundPositive)
                    return deltaBoundPositive;
                return delta;
            }
            return 0;
        }

        // see Regex cheat sheet http://www.mikesdotnetting.com/article/46/c-regular-expressions-cheat-sheet
        private static Regex regexNonWordChar = new Regex("\\W");
        private static Regex regexBeginsWithNumber = new Regex("^\\d");
        public static string FilenameToVarName(this string filename)
        {
            var underscores = Path.GetFileName(filename)
                .Replace(' ', '_')
                .Replace('.', '_')
                ;
            var clean = regexNonWordChar.Replace(underscores, "");
            if (regexBeginsWithNumber.IsMatch(clean))
                clean = "_" + clean;
            return clean;
        }

        public static RenderableProject GetVideoClipsForRendering(this VidkaProj proj)
        {
            var arrClips = proj.ClipsVideo.ToArray();
            var fileList1 = arrClips
                .DistinctHaving(x => x.FileName)
                .Select(x => new RenderableMediaFile {
                    FileName = x.FileName,
                    VarName = x.FileName.FilenameToVarName(),
                    Type = GetRenderableVideoFileType(x),
                });
            // custom audios...
            var fileList2 = arrClips
                .Where(x => x.HasCustomAudio)
                .DistinctHaving(x => x.CustomAudioFilename)
                .Select(x => new RenderableMediaFile
                {
                    FileName = x.CustomAudioFilename,
                    VarName = x.CustomAudioFilename.FilenameToVarName(),
                    Type = RenderableMediaFileType.AudioSource,
                });
            var fileList = fileList1
                .Union(fileList2)
                .DistinctHaving(x => x.FileName)
                .ToList();
            var arrClips2 = arrClips.Select((x, i) => new VideoClipRenderable
            {
                VarName = x.FileName.FilenameToVarName() + "_" + i,
                VideoFile = fileList.FirstOrDefault(y => y.FileName == x.FileName),
                FrameStart = x.FrameStart,
                FrameEnd = x.FrameEnd,
                EasingLeft = x.EasingLeft,
                EasingRight = x.EasingRight,
                IsMuted = x.IsMuted,
                PostOp = x.PostOp,
                HasCustomAudio = x.HasCustomAudio,
                CustomAudioFile = fileList.FirstOrDefault(y => y.FileName == x.CustomAudioFilename),
                CustomAudioOffset = x.CustomAudioOffset,
                ClipType = GetRenderableTypeOfClip(x),
            }).ToArray();

            long maxLengthOfImageClip = 0;
            var imageClips = arrClips.Where(x => x is VidkaClipImage || x is VidkaClipTextSimple);
            if (imageClips.Any())
                maxLengthOfImageClip = imageClips.Select(x => x.LengthFrameCalc).Max();

            // .... set up the easings and their audio mixes
            long curFrameEnd = 0;
            for (int i = 0; i < arrClips2.Length; i++)
            {
                curFrameEnd += arrClips2[i].LengthFrameCalc;
                if (arrClips2[i].EasingLeft == 0 && arrClips2[i].EasingRight == 0)
                    continue;
                if (arrClips2[i].EasingLeft > 0)
                {
                    var curFrame = curFrameEnd - arrClips2[i].LengthFrameCalc;
                    long easeEndAbs = curFrame - arrClips2[i].EasingLeft;
                    // ... begin counting from clip's beginning and end (right) of left ease
                    long easerFrame = arrClips2[i].EasingLeft;
                    var index = i;
                    // ... walk backward until easing is used up
                    while (curFrame > easeEndAbs && index > 0)
                    {
                        index--;
                        long curClipLen = arrClips2[index].LengthFrameCalcNoEasing;
                        long easerFrame1 = easerFrame - curClipLen;
                        long easerFrame2 = easerFrame;
                        long easerOffset = 0;
                        // ... when offset is needed, when curClipLen (clip being audio-ed) is longer than the easing. Easing audio will begin at the right side of this clip using the offset
                        if (easerFrame1 < 0)
                        {
                            easerOffset = -easerFrame1;
                            easerFrame1 = 0;
                        }
                        arrClips2[index].MixesAudioFromVideo.Add(new VideoEasingAudioToMix {
                            ClipVarName = arrClips2[i].VarName,
                            FrameStart = easerFrame1,
                            FrameEnd = easerFrame2,
                            FrameOffset = easerOffset,
                        });
                        curFrame -= curClipLen;
                        easerFrame -= curClipLen;
                    }
                }
                if (arrClips2[i].EasingRight > 0)
                {
                    var curFrame = curFrameEnd;
                    long easeEndAbs = curFrame + arrClips2[i].EasingRight;
                    // ... begin counting from clip's end and beginning (left) of right ease
                    long easerFrame = arrClips2[i].LengthFrameCalc - arrClips2[i].EasingRight;
                    var index = i;
                    // ... walk forward until easing is used up
                    while (curFrame < easeEndAbs && index < arrClips2.Length - 1)
                    {
                        index++;
                        long curClipLen = arrClips2[index].LengthFrameCalcNoEasing;
                        long easerFrame1 = easerFrame;
                        long easerFrame2 = easerFrame + curClipLen;
                        // ... when curClipLen (the clip being audio-ed) exceeds the overflow. Overflow is small and overflows in just the first part of the clip
                        if (easerFrame2 > arrClips2[i].FrameEnd)
                        {
                            easerFrame2 = arrClips2[i].FrameEnd;
                        }
                        arrClips2[index].MixesAudioFromVideo.Add(new VideoEasingAudioToMix
                        {
                            ClipVarName = arrClips2[i].VarName,
                            FrameStart = easerFrame1,
                            FrameEnd = easerFrame2,
                            FrameOffset = 0,
                        });
                        curFrame += curClipLen;
                        easerFrame += curClipLen;
                    }
                }
            }
            var statVideos = arrClips2;
            //... this is important to reduce AVS overhead
            var firstStatCandidate = arrClips.FirstOrDefault(x => x.IsPixelTypeStandard);
            if (firstStatCandidate != null)
                statVideos = new[] { new VideoClipRenderable {
                    VideoFile = fileList.FirstOrDefault(f => f.FileName == firstStatCandidate.FileName)
                }};
            //... this is more important when project was cropped, b/c for cropped projects the clip marked
            //    as IsPixelTypeStandard can easily be outside
            if (proj.PixelTypeStandardClip != null)
            {
                var videoFileForThisOne = fileList.FirstOrDefault(f => f.FileName == proj.PixelTypeStandardClip.FileName);
                if (videoFileForThisOne == null)
                    fileList.Add(videoFileForThisOne = new RenderableMediaFile {
                        FileName = proj.PixelTypeStandardClip.FileName,
                        VarName = proj.PixelTypeStandardClip.FileName.FilenameToVarName(),
                        Type = GetRenderableVideoFileType(proj.PixelTypeStandardClip),
                    });
                statVideos = new[] { new VideoClipRenderable {
                    VideoFile = videoFileForThisOne
                }};
            }

            // ... make file var names all unique
            foreach (var fileFile in fileList)
            {
                var nonUniques = fileList.Where(x => x != fileFile && x.VarName == fileFile.VarName);
                foreach (var nonUnique in nonUniques)
                    nonUnique.VarName += "__" + VidkaIO.MakeGuidWord();
            }

            return new RenderableProject {
                Files = fileList,
                Clips = arrClips2,
                MaxLengthOfImageClip = maxLengthOfImageClip,
                StatVideos = statVideos,
            };
        }

        public static int RenderBreakupsCount(this VidkaProj proj)
        {
            return proj.ClipsVideo.Count(x => x.IsRenderBreakupPoint) + 1;
        }
        public static VidkaProj[] RenderBreakupsSplitIntoSubProjects(this VidkaProj proj)
        {
            var breakups = proj.ClipsVideo.Where(x => x.IsRenderBreakupPoint).ToArray();
            if (breakups.Length == 0)
                return new[] { proj };
            var result = new VidkaProj[breakups.Length + 1];
            long prevBreakup = 0;
            for (int i = 0; i < breakups.Length; i++)
            {
                var newBreakup = proj.GetVideoClipAbsFramePositionLeft(breakups[i]);
                result[i] = proj.Crop(prevBreakup, newBreakup - prevBreakup);
                prevBreakup = newBreakup;
            }
            // ... last one
            result[breakups.Length] = proj.Crop(prevBreakup, proj.GetTotalLengthOfVideoClipsFrame() - prevBreakup);
            return result;
        }
        
        #endregion

        #region ------------------------- helpers --------------------------

        private static RenderableMediaFileType GetRenderableVideoFileType(VidkaClipVideoAbstract vclip)
        {
            if (vclip is VidkaClipImage)
                return RenderableMediaFileType.ImageSource;
            if (vclip is VidkaClipTextSimple)
                return RenderableMediaFileType.ImageSource;
            return RenderableMediaFileType.DirectShowSource;
        }

        private static VideoClipRenderableType GetRenderableTypeOfClip(VidkaClipVideoAbstract clip)
        {
            if (clip is VidkaClipVideo)
                return VideoClipRenderableType.Video;
            if (clip is VidkaClipImage)
                return VideoClipRenderableType.Image;
            if (clip is VidkaClipTextSimple)
                return VideoClipRenderableType.Text;
            return VideoClipRenderableType.Video;
        }

        #endregion

        #region ------------------------- misc --------------------------

        /// <summary>
		/// Debug description
		/// </summary>
		public static string cxzxc(this VidkaClipVideoAbstract clip) {
			if (clip == null)
				return "null";
			return Path.GetFileName(clip.FileName);
        }
        #endregion

    }
}
