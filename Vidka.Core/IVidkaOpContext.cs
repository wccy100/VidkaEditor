using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.ExternalOps;
using Vidka.Core.Model;

namespace Vidka.Core
{
    public interface IVidkaOpContext
    {
        string CurFileName { get; }
        VidkaProj Proj { get; }
        VidkaUiStateObjects UiObjects { get; }
        MetaGeneratorInOtherThread MetaGenerator { get; }
        PreviewThreadLauncher PreviewLauncher { get; }
        ProjectDimensions Dimdim { get; }
        VidkaFileMapping FileMapping { get; }

        void AppendToConsole(VidkaConsoleLogLevel vidkaConsoleLogLevel, string p);
        void AddUndableAction_andFireRedo(UndoableAction action);
        void InvokeOpByName(string name);
        void ShowFrameInVideoPlayer(long frame);
        long SetFrameMarker_ShowFrameInPlayer(long frame);
        void SetFrameMarker_ForceRepaint(long frame);
        void SetCurrentVideoClip_ForceRepaint(VidkaClipVideoAbstract clip);
        void UpdateCanvasWidthFromProjAndDimdim();
        bool DialogConfirm(string title, string question);
        void Fire_ProjectUpdated_AsFarAsMenusAreConcerned();
        void Fire_PleaseTogglePreviewMode();
        void Fire_PleaseToggleConsoleVisibility();

        /// <summary>Used by ExportToAvi</summary>
        //string GetRawVideoSegmentOutputPath(int index);
        /// <summary>Used by ExportToAvi</summary>
        //string GetVdubScriptOutputPathForConcatRawSegments();
        /// <summary>Used by ExportToAvi_Segment</summary>
        //string GetSegmentVideoSegmentOutputPath(int index);

        /// <summary>Used by ExportToAvi</summary>
        string CheckRawDumpFolderIsOkAndGiveItToMe();
    }

    public static class IVidkaOpContextExtensionsAndHelpers
    {
        public static void cxzxc(this IVidkaOpContext context, string text) {
            context.AppendToConsole(VidkaConsoleLogLevel.Debug, text);
        }
        public static void iiii(this IVidkaOpContext context, string text) {
            context.AppendToConsole(VidkaConsoleLogLevel.Info, text);
        }
        public static void eeee(this IVidkaOpContext context, string text) {
            context.AppendToConsole(VidkaConsoleLogLevel.Error, text);
        }
    }
}
