using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vidka.Core.Model;
using Vidka.Core.Properties;
using Miktemk;
using Miktemk.Editor;

namespace Vidka.Core.Ops
{
    public class LinearShuffleByFilename : _VidkaOp
    {
        public LinearShuffleByFilename(IVidkaOpContext context) : base(context) { }
        public override string CommandName { get { return Name; } }
        public const string Name = "LinearShuffleByFilename";

        public override void Run()
        {
            var uiObjects = Context.UiObjects;
            var proj = Context.Proj;

            long frameOffset;
            var beginIndex = proj.GetVideoClipIndexAtFrame(Context.UiObjects.CurrentMarkerFrame, out frameOffset);
            if (beginIndex == -1)
            {
                cxzxc("This command only affects clips to the right of marker. Marker outside all possible clips!");
                return;
            }
            var clipsBefore = proj.ClipsVideo.Take(beginIndex);
            var clipsAfter = proj.ClipsVideo.Skip(beginIndex);
            var clipsAfterGroups = clipsAfter.GroupBy(x => x.FileName);
            var areAllSame = clipsAfterGroups.Select(x => x.Count()).AreAllTheSame((x, y) => (x == y));
            if (!areAllSame)
            {
                Context.DialogError("Uneven splits", "Not all videos were split into equal number of segments!\nPlease view console for details, undo, fix the problem and perform linear shuffle again.");
                cxzxc("--- linear shuffle ---\n" + clipsAfterGroups.Select(x => Path.GetFileName(x.Key) + ": " + x.Count()).StringJoin("\n") + "\n------");
            }

            var maxLength = clipsAfterGroups.Select(x => x.Count()).Max();
            var clipsAfterShuffled = new List<VidkaClipVideoAbstract>();
            for (int i = 0; i < maxLength; i++)
            {
                foreach (var group in clipsAfterGroups)
                {
                    var clip = group.Skip(i).FirstOrDefault();
                    if (clip == null)
                        continue;
                    clipsAfterShuffled.Add(clip);
                }
            }

            var newClips = clipsBefore.Union(clipsAfterShuffled).ToList();
            var oldClips = proj.ClipsVideo;
            Context.AddUndableAction_andFireRedo(new UndoableAction
            {
                Redo = () =>
                {
                    cxzxc("Delete all non-locked clips");
                    proj.ClipsVideo = newClips;
                },
                Undo = () =>
                {
                    cxzxc("UNDO Delete all non-locked clips");
                    proj.ClipsVideo = oldClips;
                },
            });
        }

    }
}
