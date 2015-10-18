using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.Model;

namespace Vidka.Core
{
	public interface ISomeCommonEditorOperations
	{
		void AddUndableAction_andFireRedo(UndoableAction action);
		void ShowFrameInVideoPlayer(long frame);
		long SetFrameMarker_ShowFrameInPlayer(long frame);
		void SetFrameMarker_ForceRepaint(long frame);
		void SetCurrentVideoClip_ForceRepaint(VidkaClipVideoAbstract clip);
		void UpdateCanvasWidthFromProjAndDimdim();
		void AppendToConsole(VidkaConsoleLogLevel vidkaConsoleLogLevel, string p);
		void cxzxc(string text);
		string CurFileName { get; }
    }
}
