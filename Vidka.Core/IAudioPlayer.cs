using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.VideoMeta;

namespace Vidka.Core
{
	public interface IAudioPlayer
	{
        // ... setup
        void AddAudioClip(double secOffset, double secIn, double secOut, string filename, object clipObj);
        void Clear();
		// ... playback
        void WeAreHereStartPlaying(double curSec);
        void SynchCurrentAudioClips(double curSec);
        void PauseAll();
		void StopWhateverYouArePlaying();
		//void PlayAudioClip(string filename, double clipSecStart, double clipSecEnd);

    }
}
