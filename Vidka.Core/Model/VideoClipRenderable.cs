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
        public IEnumerable<RenderableVideoFile> Files { get; set; }
        public IEnumerable<VideoClipRenderable> Clips { get; set; }
        public long MaxLengthOfImageClip { get; set; }
    }

    /// <summary>
    /// The VarName of this one has to all be unique in RenderableProject.Files
    /// </summary>
    public enum RenderableVideoFileType
    {
        WTF = 0,
        DirectShowSource = 1,
        ImageSource = 2,
        AudioSource = 2,
    }
    public class RenderableVideoFile
    {
        public string FileName { get; set; }
        public string VarName { get; set; }
        public RenderableVideoFileType Type { get; set; }
    }

    public class VideoClipRenderable
    {
        public RenderableVideoFile VideoFile { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public bool IsMuted { get; set; }
        public string PostOp { get; set; }

        public bool HasCustomAudio { get; set; }
        public string CustomAudioFilename { get; set; }
        public float CustomAudioOffset { get; set; }

        public VideoClipRenderableType ClipType { get; set; }

        public List<VideoEasingAudioToMix> MixesAudioFromVideo { get; private set; }

        //--------------- calculated --------------

        public long LengthFrameCalc { get { return FrameEnd - FrameStart; } }

        //--------------- constructor --------------

        public VideoClipRenderable()
        {
            MixesAudioFromVideo = new List<VideoEasingAudioToMix>();
        }
    }

    public class VideoEasingAudioToMix
    {
        public RenderableVideoFile VideoFile { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public long FrameOffset { get; set; }
        public bool IsAudioFile { get; set; }
        //public string CustomAudioFilename { get; set; }
    }
}
