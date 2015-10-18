using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vidka.Core.Model;
using Vidka.Core.Ops;
using Vidka.Core.Properties;
using Vidka.Core.VideoMeta;

namespace Vidka.Core
{
	public class DragAndDropManager
	{
		#region events
		public delegate void MetaReadyForDraggyH(string filename, VideoMetadataUseful meta);
		public event MetaReadyForDraggyH MetaReadyForDraggy;

		public delegate void MetaReadyForOutstandingVideoH(VidkaClipVideoAbstract vclip, VideoMetadataUseful meta);
		public event MetaReadyForOutstandingVideoH MetaReadyForOutstandingVideo;

		public delegate void MetaReadyForOutstandingAudioH(VidkaClipAudio aclip, VideoMetadataUseful meta);
		public event MetaReadyForOutstandingAudioH MetaReadyForOutstandingAudio;

		public delegate void ThumbOrWaveReadyH();
		public event ThumbOrWaveReadyH ThumbOrWaveReady;

		public delegate void PleaseUnlockThisFileH(string filename);
		public event PleaseUnlockThisFileH PleaseUnlockThisFile;
		#endregion

		private IVideoShitbox editor;
		private static string[] EXT_video, EXT_audio, EXT_image;
		private MetaGeneratorInOtherThread metaGenerator;
		private List<DragAndDropMediaFile> _draggies;
		private List<VidkaClipVideoAbstract> outstandingVideo;
		private List<VidkaClipAudio> outstandingAudio;

		// public
		public VidkaProj Proj { get; private set; }
		public DragAndDropManagerMode Mode { get; private set; }
		public string OriginalFile { get; private set; }
		public string[] OriginalFiles { get; private set; }
		public string DraggyText { get; private set; }
		public IEnumerable<DragAndDropMediaFile> Draggies { get { return _draggies; } }
		public MetaGeneratorInOtherThread MetaGenerator { get { return metaGenerator; } }

		//helpers
		public EditorDraggyMode ModeForUiDraggy { get {
			if (Mode == DragAndDropManagerMode.Video ||
				Mode == DragAndDropManagerMode.Image)
				return EditorDraggyMode.VideoTimeline;
			if (Mode == DragAndDropManagerMode.Audio)
				return EditorDraggyMode.AudioTimeline;
			if (Mode == DragAndDropManagerMode.Folder)
				return EditorDraggyMode.DraggingFolder;
			return EditorDraggyMode.None;
		} }

		public DragAndDropManager(IVideoShitbox editor, VidkaProj proj, VidkaFileMapping fileMapping)
		{
			this.editor = editor;
			Proj = proj;
			Mode = DragAndDropManagerMode.None;
			_draggies = new List<DragAndDropMediaFile>();
			outstandingVideo = new List<VidkaClipVideoAbstract>();
			outstandingAudio = new List<VidkaClipAudio>();
			EXT_video = Settings.Default.FileExtensionsVideo.Split('|');
			EXT_audio = Settings.Default.FileExtensionsAudio.Split('|');
			EXT_image = Settings.Default.FileExtensionsImage.Split('|');
			metaGenerator = new MetaGeneratorInOtherThread(fileMapping);
			//metaGenerator.OneItemFinished += metaGenerator_OneItemFinished;
			//metaGenerator.MetaGeneratorDone += metaGenerator_MetaGeneratorDone;
			metaGenerator.HereIsSomeTextForConsole += genericListener_AppendToConsole;
			metaGenerator.MetaReady += metaGenerator_MetaReady;
			metaGenerator.ThumbnailsReady += metaGenerator_ThumbReady;
			metaGenerator.WaveformReady += metaGenerator_WaveReady;
			metaGenerator.PleaseUnlockThisFile += metaGenerator_PleaseUnlockThisFile;
		}

		public void SetProj(VidkaProj proj) {
			Proj = proj;
		}

		public void NewFilesDragged(string[] filenames, long nFakeFrames)
		{
			var relevantFiles = GetRelevantFilenames(filenames);
			var sampleFirst = relevantFiles.FirstOrDefault();
			OriginalFiles = relevantFiles;
			OriginalFile = sampleFirst;
			if (IsFilenameVideo(sampleFirst))
			{
				Mode = DragAndDropManagerMode.Video;
				DraggyText = "Analyzing...";
				foreach (var filename in relevantFiles)
				{
					_draggies.Add(new DragAndDropMediaFile(Proj) {
						Filename = filename,
						NFakeFrames = nFakeFrames,
					});
					metaGenerator.RequestMeta(filename);
					metaGenerator.RequestThumbsAndWave(filename);
				}
			}
			else if (IsFilenameAudio(sampleFirst))
			{
				Mode = DragAndDropManagerMode.Audio;
				DraggyText = "Analyzing...";
                foreach (var filename in relevantFiles)
                {
                    _draggies.Add(new DragAndDropMediaFile(Proj)
                    {
                        Filename = filename,
                        NFakeFrames = (long)(Proj.FrameRate * Settings.Default.ImageClipLengthSeconds),
                        HasAudio = true,
                    });
                    metaGenerator.RequestMeta(filename);
                    metaGenerator.RequestWaveOnly(filename);
                }
			}
			else if (IsFilenameImage(sampleFirst))
			{
				Mode = DragAndDropManagerMode.Image;
				DraggyText = "Analyzing...";
				foreach (var filename in relevantFiles)
				{
					_draggies.Add(new DragAndDropMediaFile(Proj)
					{
						Filename = filename,
						NFakeFrames = (long)(Proj.FrameRate * Settings.Default.ImageClipLengthSeconds),
						HasAudio = false,
					});
					//metaGenerator.RequestMeta(filename);
					metaGenerator.RequestThumbsOnly(filename);
				}
			}
			else if (Directory.Exists(sampleFirst))
			{
				Mode = DragAndDropManagerMode.Folder;
				DraggyText = Path.GetFileName(sampleFirst);
				_draggies.Add(new DragAndDropMediaFile(Proj)
				{
					Filename = sampleFirst,
				});
			}
			foreach (var ddd in _draggies) {
				ddd.Mode = Mode;
			}
		}

		public VidkaClipVideoAbstract[] FinalizeDragAndMakeVideoClips()
		{
			lock (this)
			{
				IEnumerable<VidkaClipVideoAbstract> clips = null;
				if (Mode == DragAndDropManagerMode.Video)
				{
					//TODO: asdqdscqwwq Take(1) is to be removed when we support multiple draggies
					clips = _draggies.Take(1).Select(x => new VidkaClipVideo
					{
						FileName = x.Filename,
						FileLengthSec = Proj.FrameToSec(x.LengthInFrames),
						FrameStart = 0,
						FrameEnd = x.LengthInFrames, //Proj.SecToFrame(dragMeta.VideoDurationSec) // its ok because SecToFrame floors it
						IsNotYetAnalyzed = (x.Meta == null),
						HasAudioXml = (x.Meta != null) ? x.Meta.HasAudio : false,
					}).ToList();
					outstandingVideo.AddRange(clips.Where(x => x.IsNotYetAnalyzed));
				}
				else if (Mode == DragAndDropManagerMode.Image)
				{
					clips = _draggies.Take(1).Select(x => new VidkaClipImage
					{
						FileName = x.Filename,
						FileLengthSec = Proj.FrameToSec(x.LengthInFrames),
						//HasAudioXml = false,
						FrameStart = 0,
						FrameEnd = x.LengthInFrames,
						IsNotYetAnalyzed = false
					}).ToList();
				}
				_draggies.Clear();
				Mode = DragAndDropManagerMode.None;
				return (clips == null)
					? null
					: clips.ToArray();
			}
		}
		public VidkaClipAudio[] FinalizeDragAndMakeAudioClips(long firstFrameFromDraggy)
		{
			if (Mode != DragAndDropManagerMode.Audio)
				return null;
			lock (this)
			{
				//TODO: Take(1) is to be removed when we support multiple draggies
				var clips = _draggies.Take(1).Select(x => new VidkaClipAudio {
					FileName = x.Filename,
					FileLengthSec = Proj.FrameToSec(x.LengthInFrames),
					FrameStart = 0,
					FrameEnd = x.LengthInFrames, //Proj.SecToFrame(dragMeta.VideoDurationSec) // its ok because SecToFrame floors it
					IsNotYetAnalyzed = (x.Meta == null),
				}).ToList();
                var curFrame = firstFrameFromDraggy;
                foreach (var clip in clips)
                {
                    clip.FrameOffset = curFrame;
                    curFrame += clip.LengthFrameCalc;
                }
				outstandingAudio.AddRange(clips.Where(x => x.IsNotYetAnalyzed));
				_draggies.Clear();
				Mode = DragAndDropManagerMode.None;
				return clips.ToArray();
			}
		}

		internal void CancelDragDrop()
		{
			_draggies.Clear();
			Mode = DragAndDropManagerMode.None;
		}

		#region event handlers

		private void genericListener_AppendToConsole(VidkaConsoleLogLevel level, string text)
		{
			editor.AppendToConsole(level, text);
		}
		private void metaGenerator_MetaReady(string filename, VideoMetadataUseful meta)
		{
			lock (this)
			{
				// 2 cases: meta is ready for one of the draggies, or for one of the outstanding media
				var draggyMaybe = _draggies.FirstOrDefault(x => x.Filename == filename);
				if (draggyMaybe != null)
				{
					draggyMaybe.Meta = meta;
					if (MetaReadyForDraggy != null)
						MetaReadyForDraggy(filename, meta);
					return;
				}
				// at this point it could be one of outstanding media (video or audio)
				var outstandingMaybeVid = outstandingVideo.FirstOrDefault(x => x.FileName == filename);
				if (outstandingMaybeVid != null)
				{
					// TODO: handle variable fps, fps == proj.fps and counted frames for PENTAX avis
					outstandingMaybeVid.FileLengthSec = meta.GetVideoDurationSec(Proj.FrameRate);
					// remember, this clip could be different fps, we need proj's fps
					var projFramesThisOne = Proj.SecToFrame(outstandingMaybeVid.FileLengthSec ?? 0); 
					outstandingMaybeVid.FileLengthFrames = projFramesThisOne;
					outstandingMaybeVid.FrameEnd = projFramesThisOne;
					if (outstandingMaybeVid is VidkaClipVideo)
					{
						var outstandingMaybeVidVVV = (VidkaClipVideo)outstandingMaybeVid;
						outstandingMaybeVidVVV.HasAudioXml = meta.HasAudio;
					}
					outstandingMaybeVid.IsNotYetAnalyzed = false;
					outstandingVideo.Remove(outstandingMaybeVid);
					if (MetaReadyForOutstandingVideo != null)
						MetaReadyForOutstandingVideo(outstandingMaybeVid, meta);
					return;
				}
				var outstandingMaybeAud = outstandingAudio.FirstOrDefault(x => x.FileName == filename);
				if (outstandingMaybeAud != null)
				{
					outstandingMaybeAud.FileLengthSec = meta.AudioDurationSec;
					var projFramesThisOne = Proj.SecToFrame(outstandingMaybeAud.FileLengthSec ?? 0);
					outstandingMaybeAud.FileLengthFrames = projFramesThisOne;
					outstandingMaybeAud.FrameEnd = projFramesThisOne;
					outstandingAudio.Remove(outstandingMaybeAud);
					if (MetaReadyForOutstandingAudio != null)
						MetaReadyForOutstandingAudio(outstandingMaybeAud, meta);
					return;
				}
			}
		}
		private void metaGenerator_WaveReady(string filename, string fileWave, string fileWaveJpg)
		{
			if (ThumbOrWaveReady != null)
				ThumbOrWaveReady();
		}
		private void metaGenerator_ThumbReady(string filename, string fileThumbs)
		{
			if (ThumbOrWaveReady != null)
				ThumbOrWaveReady();
		}

		private void metaGenerator_PleaseUnlockThisFile(string filename)
		{
			if (PleaseUnlockThisFile != null)
				PleaseUnlockThisFile(filename);
		}

		//private void metaGenerator_OneItemFinished(MetaGeneratorInOtherThread_QueueRequest item)
		//{
		//	// just repaint the damn thing
		//	editor.PleaseRepaint();
		//}
		//private void metaGenerator_MetaGeneratorDone(MetaGeneratorInOtherThread_QueueRequest item, VideoMetadataUseful meta)
		//{
		//	UiObjects.SetDraggyCoordinates(frameLength: meta.VideoDurationFrames);
		//	editor.PleaseRepaint();
		//}

		#endregion

		
		#region helpers

		public static bool IsFilenameVideo(string filename) {
			return IsFilenameOneOfThese(filename, EXT_video);
		}
		public static bool IsFilenameAudio(string filename) {
			return IsFilenameOneOfThese(filename, EXT_audio);
		}
		public static bool IsFilenameImage(string filename) {
			return IsFilenameOneOfThese(filename, EXT_image);
		}

		private static bool IsFilenameOneOfThese(string filename, string[] extensions)
		{
			if (String.IsNullOrEmpty(filename))
				return false;
			var ext = Path.GetExtension(filename).ToLower();
			return extensions.Any(e => String.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Removes all non-video and non-audio filenames (by extension)
		/// Returns only video if given a mix of video and audio files.
		/// </summary>
		private string[] GetRelevantFilenames(string[] filenames)
		{
			// TODO: implement this filter please, otherwise our draggy will be fucked
			return filenames;
		}

		#endregion


		public void QueueUpTheWholeFolder(string folder)
		{
			foreach (var filename in Directory.GetFiles(folder))
			{
				if (String.Equals(Path.GetFileName(filename), "Thumbs.db", StringComparison.OrdinalIgnoreCase))
					continue;
				if (IsFilenameVideo(filename) ||
					IsFilenameImage(filename) ||
					IsFilenameAudio(filename)) //TODO: audio
				{
					metaGenerator.RequestMeta(filename);
					metaGenerator.RequestThumbsAndWave(filename);
				}
			}
			foreach (var dirname in Directory.GetDirectories(folder))
			{
				if (Path.GetFileName(dirname) == VidkaFileMapping_resource.DATA_FOLDER)
					continue;
				// recurse, bitch
				QueueUpTheWholeFolder(dirname);
			}
		}
	}

	public enum DragAndDropManagerMode {
		None = 0,
		Video = 1,
		Audio = 2,
		Image = 3,
		Folder = 10,
	}

	public class DragAndDropMediaFile
	{
		private VidkaProj proj;
		public DragAndDropMediaFile(VidkaProj proj)
		{
			this.proj = proj;
		}
		public string Filename { get; set; }
		public DragAndDropManagerMode Mode { get; set; }
		public long NFakeFrames { get; set; }
		public bool HasAudio { get; set; }
		public VideoMetadataUseful Meta { get; set; }
		public long LengthInFrames { get {
			if (Meta == null)
				return NFakeFrames;
			double durationSec = 0;
			if (Mode == DragAndDropManagerMode.Video)
				durationSec = Meta.GetVideoDurationSec(proj.FrameRate);
			else if (Mode == DragAndDropManagerMode.Audio)
				durationSec = Meta.AudioDurationSec;
			else if (Mode == DragAndDropManagerMode.Image)
				durationSec = Settings.Default.ImageClipLengthSeconds;
			return proj.SecToFrame(durationSec);
		} }
		//public double LengthInSec { get {
		//	return (Meta != null) ? Meta.VideoDurationSec : proj.FrameToSec(NFakeFrames);
		//} }

	}

}
