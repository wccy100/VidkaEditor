using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core.Model;

namespace Vidka.Core
{
	[Serializable]
	public class ClipboardObjectHolder
	{
        public ClipboardObjectType Type;
		public VidkaClipVideoAbstract VideoClip;
		public VidkaClipAudio AudioClip;
	}

	public enum ClipboardObjectType {
		None = 0,
		VideoClip = 1,
		AudioClip = 2,
	}
}
