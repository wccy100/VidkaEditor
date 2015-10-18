using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Vidka.Core.Ops;
using Vidka.Core.Properties;
using Vidka.Core.VideoMeta;

namespace Vidka.Core.Ops
{
	public class MetaGeneratorInOtherThread
	{
		#region events
		public delegate void HereIsSomeTextForConsoleHandler(VidkaConsoleLogLevel level, string text);
		public event HereIsSomeTextForConsoleHandler HereIsSomeTextForConsole;

		public delegate void MetaReadyHandler(string filename, VideoMetadataUseful meta);
		public event MetaReadyHandler MetaReady;

		public delegate void ThumbnailsReadyHandler(string filename, string fileThumbs);
		public event ThumbnailsReadyHandler ThumbnailsReady;

		public delegate void WaveformReadyHandler(string filename, string fileWave, string fileWaveJpg);
		public event WaveformReadyHandler WaveformReady;

		public delegate void PleaseUnlockThisFileH(string filename);
		public event PleaseUnlockThisFileH PleaseUnlockThisFile;
		#endregion

		private VidkaFileMapping fileMapping;
		private TaskQueueInOtherThread taskThread;

		public MetaGeneratorInOtherThread(VidkaFileMapping fileMapping)
		{
			this.fileMapping = fileMapping;
			taskThread = new TaskQueueInOtherThread();
		}


		public void RequestMeta(
			string filename,
			Action<string, VideoMetadataUseful> customCallback = null)
		{
			taskThread.QueueThisUpPlease(() =>
			{
				var filenameMeta = fileMapping.AddGetMetaFilename(filename);
				fileMapping.MakeSureDataFolderExists(filenameMeta);

				if (!File.Exists(filenameMeta))
				{
					// generates the thumbnails
					UiConsolePush(VidkaConsoleLogLevel.Info, "generating meta " + Path.GetFileName(filenameMeta));
					var op1 = new MetadataExtraction(filename, filenameMeta);
					UiPushResult(op1);
					if (MetaReady != null && op1.MetaXml != null)
						MetaReady(filename, op1.MetaXml);
                    if (customCallback != null)
                        customCallback(filename, op1.MetaXml);
				}
				else
				{
					var metaXml = MetadataExtraction.LoadMetaFromXml(filenameMeta);
					if (MetaReady != null)
						MetaReady(filename, metaXml);
					if (customCallback != null)
						customCallback(filename, metaXml);
				}
				
			});
		}

		public void RequestThumbsAndWave(string filename)
		{
			taskThread.QueueThisUpPlease(() =>
			{
				var filenameThumb = fileMapping.AddGetThumbnailFilename(filename);
				var filenameWave = fileMapping.AddGetWaveFilenameDat(filename);
				var filenameWaveJpg = fileMapping.AddGetWaveFilenameJpg(filename);
				fileMapping.MakeSureDataFolderExists(filenameThumb);

				if (!File.Exists(filenameThumb)) {
					// generates the thumbnails
					UiConsolePush(VidkaConsoleLogLevel.Info, "generating thumbs " + Path.GetFileName(filenameThumb));
					var op2 = new ThumbnailExtraction(filename, filenameThumb);
					op2.PleaseUnlockThisFile += dispatch_pleaseUnlockThisFile;
					op2.run();
					UiPushResult(op2);
				}
				if (ThumbnailsReady != null)
					ThumbnailsReady(filename, filenameThumb);

				if (!File.Exists(filenameWaveJpg)) {
					// generates the waveform
					UiConsolePush(VidkaConsoleLogLevel.Info, "generating wave " + Path.GetFileName(filenameWaveJpg));
					var op3 = new WaveformExtraction(filename, filenameWave, filenameWaveJpg, true);
					UiPushResult(op3);
				}
				if (WaveformReady != null)
					WaveformReady(filename, filenameWave, filenameWaveJpg);
			});
			
		}

		public void RequestThumbsOnly(
			string filename,
			bool forceRegenerate = false)
		{
			taskThread.QueueThisUpPlease(() =>
			{
				var filenameThumb = fileMapping.AddGetThumbnailFilename(filename);
				fileMapping.MakeSureDataFolderExists(filenameThumb);

				if (!File.Exists(filenameThumb) || forceRegenerate)
				{
					// generates the thumbnails
					UiConsolePush(VidkaConsoleLogLevel.Info, "generating thumbs " + Path.GetFileName(filenameThumb));
					var op2 = new ThumbnailExtraction(filename, filenameThumb);
					op2.PleaseUnlockThisFile += dispatch_pleaseUnlockThisFile;
					op2.run();
					UiPushResult(op2);
				}
				if (ThumbnailsReady != null)
					ThumbnailsReady(filename, filenameThumb);
			});
		}

		public void RequestWaveOnly(
			string filename,
			bool forceRegenerate = false,
			Action<string, string, string> customCallback = null)
		{
			taskThread.QueueThisUpPlease(() =>
			{
				var filenameWave = fileMapping.AddGetWaveFilenameDat(filename);
				var filenameWaveJpg = fileMapping.AddGetWaveFilenameJpg(filename);
				fileMapping.MakeSureDataFolderExists(filenameWave);

				if (!File.Exists(filenameWaveJpg))
				{
					// generates the waveform
					UiConsolePush(VidkaConsoleLogLevel.Info, "generating wave " + Path.GetFileName(filenameWaveJpg));
					var op3 = new WaveformExtraction(filename, filenameWave, filenameWaveJpg, true);
					UiPushResult(op3);
				}
				if (WaveformReady != null)
					WaveformReady(filename, filenameWave, filenameWaveJpg);
				if (customCallback != null)
					customCallback(filename, filenameWave, filenameWaveJpg);
			});
		}

		#region event pushes

		private void UiPushResult(OpBaseClass op)
		{
			if (op.ResultCode == OpResultCode.FileNotFound)
				UiConsolePush(VidkaConsoleLogLevel.Error, "Error: please make sure ffmpeg is in your PATH!");
			else if (op.ResultCode == OpResultCode.OtherError)
				UiConsolePush(VidkaConsoleLogLevel.Error, "Error: " + op.ErrorMessage);
			else if (op.ResultCode == OpResultCode.OK)
				UiConsolePush(VidkaConsoleLogLevel.Info, "Done.");
			if (!String.IsNullOrEmpty(op.ErrorMessage2))
				UiConsolePush(VidkaConsoleLogLevel.Error, op.ErrorMessage2);
		}

		private void UiConsolePush(VidkaConsoleLogLevel level, string text)
		{
			if (HereIsSomeTextForConsole != null)
				HereIsSomeTextForConsole(level, text);
		}

		private void dispatch_pleaseUnlockThisFile(string filename)
		{
			if (PleaseUnlockThisFile != null)
				PleaseUnlockThisFile(filename);
		}

		#endregion
	}
}
