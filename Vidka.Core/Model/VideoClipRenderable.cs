using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vidka.Core.Model
{
    public enum VideoClipRenderableType
    {
        Video = 0,
        Image = 1,
        Text = 2,
        Blank = 3
    }

    /// <summary>
    /// This class is used by VidkaIO class to generate the AVS file
    /// An object of this class is obtained by calling GetVideoClipsForRendering method
    /// </summary>
    public class RenderableProject
    {
        public IEnumerable<RenderableMediaFile> Files { get; set; }
        public IEnumerable<VideoClipRenderable> Clips { get; set; }
        public IEnumerable<VideoClipRenderable> StatVideos { get; set; }
        public long MaxLengthOfImageClip { get; set; }
        public IEnumerable<AudioClipRenderable> AudioClips { get; set; }
    }

    /// <summary>
    /// The VarName of this one has to all be unique in RenderableProject.Files
    /// </summary>
    public enum RenderableMediaFileType
    {
        WTF = 0,
        DirectShowSource = 1,
        ImageSource = 2,
        AudioSource = 3,
    }
    public class RenderableMediaFile
    {
        public string FileName { get; set; }
        public string VarName { get; set; }
        public RenderableMediaFileType Type { get; set; }
    }

    public class VideoClipRenderable
    {
        public string VarName { get; set; }
        public RenderableMediaFile VideoFile { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public long EasingLeft { get; set; }
        public long EasingRight { get; set; }
        public bool IsMuted { get; set; }
        public string PostOp { get; set; }

        public bool HasCustomAudio { get; set; }
        public RenderableMediaFile CustomAudioFile { get; set; }
        //public string CustomAudioFilename { get; set; }
        public float CustomAudioOffset { get; set; }

        public VideoClipRenderableType ClipType { get; set; }

        public List<VideoEasingAudioToMix> MixesAudioFromVideo { get; private set; }

        //--------------- calculated --------------

        public long LengthFrameCalc { get { return FrameEnd - FrameStart; } }
        public long LengthFrameCalcNoEasing { get { return FrameEnd - EasingRight - FrameStart - EasingLeft; } }

        //--------------- constructor --------------

        public VideoClipRenderable()
        {
            MixesAudioFromVideo = new List<VideoEasingAudioToMix>();
        }
    }

    public class AudioClipRenderable
    {
        public string FileName { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public long FrameOffset { get; set; }
        public string PostOp { get; set; }
    }

    public class VideoEasingAudioToMix
    {
        public string ClipVarName { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public long FrameOffset { get; set; }
        //public bool IsAudioFile { get; set; }
        //public string CustomAudioFilename { get; set; }
    }
}
