using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;

namespace Vidka.Core.Model
{
	[Serializable]
	public class VidkaProj
	{
		public VidkaProj()
		{
			ClipsVideo = new List<VidkaClipVideoAbstract>();
			ClipsAudio = new List<VidkaClipAudio>();
			FrameRate = 30;
			Width = 1280;
			Height = 720;
		}

		public string Name { get; set; }
		public double FrameRate { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		[XmlArray("ClipsVideo")]
		[XmlArrayItem("VidkaClipVideo", typeof(VidkaClipVideo))]
		[XmlArrayItem("VidkaClipImage", typeof(VidkaClipImage))]
		[XmlArrayItem("VidkaClipTextSimple", typeof(VidkaClipTextSimple))]
		public List<VidkaClipVideoAbstract> ClipsVideo { get; set; }
		
		public List<VidkaClipAudio> ClipsAudio { get; set; }

        //public bool PreviewAvsSegmentLocalFilename { get; set; }

        /// <summary>
        /// Used when proj is cropped, esp for partial rendering
        /// </summary>
        [XmlIgnore]
        public VidkaClipVideoAbstract PixelTypeStandardClip { get; set; }

		/// <summary>
		/// computes all Calced* values
        /// call this whenever a new clip is added and frame rate changes.
		/// This will set all the helper variables in every clip
		/// </summary>
		public void Compile()
		{
			foreach (var vclip in ClipsVideo)
            {
				vclip.FileLengthFrames = this.SecToFrame(vclip.FileLengthSec ?? 0);
                foreach (var link in vclip.AudioClipLinks)
                    link.AudioClip = ClipsAudio.FirstOrDefault(areal => areal.Id == link.AudioClip.Id);
			}
            foreach (var aclip in ClipsAudio)
            {
                if (String.IsNullOrEmpty(aclip.Id))
                    aclip.Id = VidkaIO.MakeGuidWord();
                aclip.FileLengthFrames = this.SecToFrame(aclip.FileLengthSec ?? 0);
			}
		}
    }

	[Serializable]
	public class VidkaClip
	{
		public string FileName { get; set; }
		/// <summary>
		/// position (frames) wrt file
		/// </summary>
		public long FrameStart { get; set; }
		/// <summary>
		/// position (frames) wrt file
		/// </summary>
		public long FrameEnd { get; set; }
		/// <summary>
		/// Stored in seconds, but we will only use it to convert to FileLengthFrames with proj-fps
		/// </summary>
		public double? FileLengthSec { get; set; }
		/// <summary>
		/// will not be able to trim this clip anymore, it is marked different in UI.
		/// This helps to tell good clips from the rest of the garbage
		/// </summary>
		public bool IsLocked { get; set; }

		// helpers

		/// <summary>
		/// (FrameEnd - FrameStart) of this clip
		/// </summary>
		[XmlIgnore]
		public virtual long LengthFrameCalc { get { return FrameEnd - FrameStart; } }
        [XmlIgnore]
        public virtual long FrameStartNoEase { get { return FrameStart; } }
        [XmlIgnore]
        public virtual long FrameEndNoEase { get { return FrameEnd; } }
		/// <summary>
		/// Needs to be set by multiplying FileLengthSec by proj-fps
		/// </summary>
		[XmlIgnore]
		public long FileLengthFrames { get; set; }
		[XmlIgnore]
		public bool IsNotYetAnalyzed { get; set; }

        public virtual VidkaClip MakeCopy()
        {
            var clip = (VidkaClip)this.MemberwiseClone();
            return clip;
        }
	}

	[Serializable]
	public abstract class VidkaClipVideoAbstract : VidkaClip
	{
		public VidkaClipVideoAbstract()
		{
            AudioClipLinks = new List<VidkaAudioClipLink>();
            Subtitles = new List<VidkaSubtitle>();
		}

        public List<VidkaAudioClipLink> AudioClipLinks { get; protected set; }
        public List<VidkaSubtitle> Subtitles { get; protected set; }
		public bool IsMuted { get; set; }
		public string PostOp { get; set; }
		public bool HasCustomAudio { get; set; }
		public string CustomAudioFilename { get; set; }
        public double? CustomAudioLengthSec { get; set; }
		public float CustomAudioOffset { get; set; }
        public long EasingLeft { get; set; }
        public long EasingRight { get; set; }
        public bool IsPixelTypeStandard { get; set; }
        public bool IsRenderBreakupPoint { get; set; }
        public string Label { get; set; }

        [XmlIgnore]
		public virtual bool HasAudio { get { return false; } }
        [XmlIgnore]
        public override long LengthFrameCalc { get { return FrameEnd - FrameStart - EasingLeft - EasingRight; } }
        [XmlIgnore]
        public long LengthFrameCalcNoEase { get { return FrameEnd - FrameStart; } }
        [XmlIgnore]
        public override long FrameStartNoEase { get { return FrameStart + EasingLeft; } }
        [XmlIgnore]
        public override long FrameEndNoEase { get { return FrameEnd - EasingRight; } }

		public virtual VidkaClipVideoAbstract MakeCopy_VideoClip()
		{
            var clip = (VidkaClipVideoAbstract)this.MemberwiseClone(); // ... problem with MemberwiseClone is that it copies lists by ref... #$%^&
            clip.AudioClipLinks = new List<VidkaAudioClipLink>();
            clip.Subtitles = new List<VidkaSubtitle>();
            clip.IsRenderBreakupPoint = false;
            // TODO: copy over subtitles
			return clip;
		}
        public override VidkaClip MakeCopy() {
            return MakeCopy_VideoClip();
        }
		public virtual long GetPlaybackFrameStart(long? curstomFrameOffset) {
            return curstomFrameOffset ?? FrameStart + EasingLeft;
		}
		public virtual long GetPlaybackFrameEnd(long? curstomFrameOffset) {
            return FrameEnd - EasingRight;
		}
    }

	[Serializable]
	public class VidkaClipVideo : VidkaClipVideoAbstract
	{
		public VidkaClipVideo() {}

		public bool? HasAudioXml { get; set; }

		[XmlIgnore]
		public override bool HasAudio { get { return HasAudioXml ?? true; } }

        //TODO: when do we ever need this??
        //public override VidkaClipVideoAbstract MakeCopy_VideoClip()
        //{
        //    var clip = (VidkaClipVideoAbstract)this.MemberwiseClone();
        //    clip.AudioClipLinks = new List<VidkaAudioClipLink>();
        //    // TODO: copy over non-shallow values (subtitles, etc)
        //    return clip;
        //}
	}

	[Serializable]
	public class VidkaClipImage : VidkaClipVideoAbstract
	{
		public double FileLengthSecOverride { get; set; }

		public override long GetPlaybackFrameStart(long? curstomFrameOffset) {
			return 0;
		}
		public override long GetPlaybackFrameEnd(long? curstomFrameOffset) {
			return curstomFrameOffset.HasValue
				? FrameEnd - curstomFrameOffset.Value
				: LengthFrameCalc;
		}
	}

	[Serializable]
	public class VidkaClipTextSimple : VidkaClipVideoAbstract
	{
		public override long GetPlaybackFrameStart(long? curstomFrameOffset)
		{
			return 0;
		}
		public override long GetPlaybackFrameEnd(long? curstomFrameOffset)
		{
			return curstomFrameOffset.HasValue
				? FrameEnd - curstomFrameOffset.Value
				: LengthFrameCalc;
		}

		public string Text { get; set; }
		public int ArgbBackgroundColor { get; set; }
		public int ArgbFontColor { get; set; }
		public float FontSize { get; set; }
	}

	[Serializable]
	public class VidkaClipAudio : VidkaClip
	{
		public VidkaClipAudio() { }

        public string Id { get; set; }
		/// <summary>
		/// position (frames) wrt project's beginning of the start of this audio clip
		/// </summary>
		public long FrameOffset { get; set; }
        public string PostOp { get; set; }

		public VidkaClipAudio MakeCopy_AudioClip()
		{
			var clip = (VidkaClipAudio)this.MemberwiseClone();
            clip.Id = VidkaIO.MakeGuidWord();
			return clip;
		}
        public override VidkaClip MakeCopy() {
            return MakeCopy_AudioClip();
        }
    }

    [Serializable]
    public class VidkaAudioClipLink
    {
        /// <summary>
        /// Diff = absPosAud - absPosVid
        /// </summary>
        public long SynchFrames { get; set; }
        public VidkaClipAudio AudioClip { get; set; }
    }

	[Serializable]
	public class VidkaSubtitle
	{
		public VidkaSubtitle() {
		}

		public string Text { get; set; }
		/// <summary>
		/// relative to start of video file
		/// </summary>
		public int FrameStart { get; set; }
		/// <summary>
		/// relative to start of video file
		/// </summary>
		public int FrameEnd { get; set; }
	}
}
