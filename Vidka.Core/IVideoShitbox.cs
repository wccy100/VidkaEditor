using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.VideoMeta;

namespace Vidka.Core
{
	public enum VidkaConsoleLogLevel
	{
		Info = 1,
		Debug = 2,
		Error = 3,
	}

	public enum PreviewPlayerAbsoluteLocation
	{
		TopRight = 1,
		BottomRight = 2,
	}

	public interface IVideoShitbox : IVidkaConsole
	{
		int Width { get; }
		int Height { get; }

		//void SetDraggy(VideoMetadataUseful meta);
		void PleaseRepaint();
		void UpdateCanvasWidth(int w);
		void UpdateCanvasHorizontalScroll(int scrollX);
		string OpenProjectSaveDialog();
		string OpenProjectOpenDialog();
        bool ShouldIProceedIfProjectChanged();
		void ShowErrorMessage(string title, string message);
        bool ShowConfirmMessage(string title, string message);
        bool ShowInputMessage(string title, string message, string oldAnswer, out string answer);
		void PleaseUnlockThisFile(string filename);

        void OpenClipProperties(Model.VidkaClip vidkaClip);

        int GetHorizontalScrollBarHeight();
    }
}
