using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Properties;
using Vidka.Core.Model;

namespace Vidka.Core.Ops
{
    public class DeleteCurSelectedClip : _VidkaOp
    {
        public DeleteCurSelectedClip(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "DeleteCurSelectedClip";


        public override bool TriggerByKeyPress(KeyEventArgs e)
        {
            return (e.KeyCode == Keys.Delete);
        }

        public override void Run()
        {
            if (UiObjects.CurrentVideoClip != null)
            {
                var toRemove = UiObjects.CurrentVideoClip;
                var clipIndex = Proj.ClipsVideo.IndexOf(toRemove);
                Context.AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        cxzxc("delete vclip " + clipIndex);
                        Proj.ClipsVideo.Remove(toRemove);
                    },
                    Undo = () =>
                    {
                        cxzxc("UNDO delete vclip " + clipIndex);
                        Proj.ClipsVideo.Insert(clipIndex, toRemove);
                    },
                    PostAction = () =>
                    {
                        UiObjects.SetHoverVideo(null);
                        if (Proj.ClipsVideo.Count == 0)
                        {
                            UiObjects.SetActiveVideo(null, Proj);
                            Context.SetFrameMarker_ForceRepaint(0);
                        }
                        else
                        {
                            var highlightIndex = clipIndex;
                            if (highlightIndex >= Proj.ClipsVideo.Count)
                                highlightIndex = Proj.ClipsVideo.Count - 1;
                            var clipToSelect = Proj.ClipsVideo[highlightIndex];
                            var firstFrameOfSelected = Proj.GetVideoClipAbsFramePositionLeft(clipToSelect);
                            UiObjects.SetActiveVideo(clipToSelect, Proj);
                            //UiObjects.SetCurrentMarkerFrame(firstFrameOfSelected);
                            // TODO: don't repaint twice, rather keep track of whether to repaint or not
                            Context.SetFrameMarker_ShowFrameInPlayer(firstFrameOfSelected);
                            // ... deleting clips should have an impact on render partial menu, meaning when delete clip (or duplicate) projupdate should also be fired, yes?
                            if (toRemove.IsRenderBreakupPoint)
                                Context.Fire_ProjectUpdated_AsFarAsMenusAreConcerned();
                        }
                        Context.UpdateCanvasWidthFromProjAndDimdim();
                    }
                });
            }
            else if (UiObjects.CurrentAudioClip != null)
            {
                var toRemove = UiObjects.CurrentAudioClip;
                var allLinksQuery = Proj.ClipsVideo.Where(c => c.AudioClipLinks.Any(l => l.AudioClip == toRemove));
                var allLinks = (allLinksQuery.Any())
                    ? allLinksQuery.Select(c => new Tuple<VidkaClipVideoAbstract, VidkaAudioClipLink>(c, c.AudioClipLinks.FirstOrDefault(l => l.AudioClip == toRemove))).ToArray()
                    : null;
                Context.AddUndableAction_andFireRedo(new UndoableAction
                {
                    Redo = () =>
                    {
                        cxzxc("delete aclip");
                        Proj.ClipsAudio.Remove(toRemove);
                        if (allLinks != null)
                            foreach (var lll in allLinks)
                                lll.Item1.AudioClipLinks.Remove(lll.Item2);
                    },
                    Undo = () =>
                    {
                        cxzxc("UNDO delete aclip");
                        Proj.ClipsAudio.Add(toRemove);
                        if (allLinks != null)
                            foreach (var lll in allLinks)
                                lll.Item1.AudioClipLinks.Add(lll.Item2);
                    },
                    PostAction = () =>
                    {
                        UiObjects.SetHoverVideo(null);
                        UiObjects.SetHoverAudio(null);
                        UiObjects.SetActiveAudio(null);
                        Context.UpdateCanvasWidthFromProjAndDimdim();
                    }
                });
            }
        }
    }
}
