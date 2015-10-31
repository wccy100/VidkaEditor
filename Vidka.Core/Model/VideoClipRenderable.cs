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

    public class VideoClipRenderable
    {
        public string FileName { get; set; }
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
        public string FileName { get; set; }
        public long FrameStart { get; set; }
        public long FrameEnd { get; set; }
        public long FrameOffset { get; set; }
        public bool IsAudioFile { get; set; }
    }
}
