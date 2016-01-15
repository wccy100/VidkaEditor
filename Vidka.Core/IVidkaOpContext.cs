using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vidka.Core.ExternalOps;
using Vidka.Core.Model;
using Vidka.Core.UiObj;

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
        void DialogError(string title, string message);
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

        public static void AddUndoableAction_insertClipAtMarkerPosition(this IVidkaOpContext context, VidkaClipVideoAbstract newClip)
        {
            var proj = context.Proj;
            var uiObjects = context.UiObjects;
            int insertIndex = 0;
            long frameOffset = 0;
            var oldMarkerPos = context.UiObjects.CurrentMarkerFrame;
            var targetIndex = proj.GetVideoClipIndexAtFrame_forceOnLastClip(oldMarkerPos, out frameOffset);
            VidkaClipVideoAbstract targetClip = null;
            if (targetIndex != -1)
            {
                insertIndex = targetIndex;
                targetClip = proj.ClipsVideo[targetIndex];
                if (frameOffset - targetClip.FrameStartNoEase >= targetClip.LengthFrameCalc / 2) // which half of the clip is the marker on?
                    insertIndex = targetIndex + 1;
            }
            context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Redo = () =>
                {
                    proj.ClipsVideo.Insert(insertIndex, newClip);
                    uiObjects.SetActiveVideo(newClip, proj);
                    var newMarkerPos = proj.GetVideoClipAbsFramePositionLeft(newClip);
                    uiObjects.SetCurrentMarkerFrame(newMarkerPos);
                    if (newClip is VidkaClipTextSimple)
                    {
                        newClip.FileName = VidkaIO.GetAuxillaryProjFile(context.CurFileName, VidkaIO.MakeUniqueFilename_AuxSimpleText());
                        VidkaIO.RebuildAuxillaryFile_SimpleText((VidkaClipTextSimple)newClip, proj, context.MetaGenerator);
                    }
                },
                Undo = () =>
                {
                    proj.ClipsVideo.Remove(newClip);
                    uiObjects.SetCurrentMarkerFrame(oldMarkerPos);
                    if (targetClip != null)
                        uiObjects.SetActiveVideo(targetClip, proj);
                },
            });
        }
    }
}
