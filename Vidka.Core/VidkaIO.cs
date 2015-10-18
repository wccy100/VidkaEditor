using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Vidka.Core.Model;
using Vidka.Core.Ops;

namespace Vidka.Core
{
	public class VidkaIO
	{
		private const string TEMPLATE_AVS = "App_data/template.avs";
		private const string COMPILE_DIR = ".vidkacompile";

		internal VidkaProj LoadProjFromFile(string filename)
		{
			XmlSerializer x = new XmlSerializer(typeof(VidkaProj));
			var fs = new FileStream(filename, FileMode.Open);
			var proj = (VidkaProj)x.Deserialize(fs);
			fs.Close();
			return proj;
		}

		internal void SaveProjToFile(VidkaProj Proj, string filename)
		{
			XmlSerializer x = new XmlSerializer(typeof(VidkaProj));
			var fs = new FileStream(filename, FileMode.Create);
			x.Serialize(fs, Proj);
			fs.Close();
		}

		internal static void ExportToAvs(VidkaProj Proj, string fileOut)
		{
			var sbClips = new StringBuilder();
			var sbClipStats = new StringBuilder();
            var sbPostOp = new StringBuilder();
            var sbAudio = new StringBuilder();
			var lastClip = Proj.ClipsVideo.LastOrDefault();
			foreach (var clip in Proj.ClipsVideo)
			{
                var lineEnding = (clip != lastClip) ? ", \\\n" : " ";
                sbPostOp.Clear();
                if (clip.HasCustomAudio)
                {
                    sbPostOp.Append(String.Format(".AddCustomAudio(\"{0}\", {1}, fstart={2}, fend={3})",
                        clip.CustomAudioFilename, clip.CustomAudioOffset, clip.FrameStart, clip.FrameEnd-1));
                }
                if (clip.IsMuted)
                    sbPostOp.Append(".MuteThisClip()");
                sbPostOp.Append((clip.PostOp ?? "").Replace("\n", ""));
                if (clip is VidkaClipVideo)
                {
	                sbClips.Append(String.Format("\tNeutralClip(\"{0}\", {1}, {2}){3}{4}",
                        clip.FileName, clip.FrameStart, clip.FrameEnd-1, sbPostOp.ToString(), lineEnding));
	                sbClipStats.Append(String.Format("collectpixeltypestat(\"{0}\", {1})\n",
		                clip.FileName, clip.LengthFrameCalc));
                }
                else if (clip is VidkaClipImage
	                || clip is VidkaClipTextSimple)
                {
	                sbClips.Append(String.Format("\tNeutralClipImage(\"{0}\", {1}){2}{3}",
                        clip.FileName, clip.LengthFrameCalc, sbPostOp.ToString(), lineEnding));
                }
			}

            foreach (var clip in Proj.ClipsAudio)
            {
                sbAudio.Append(String.Format(@"
voiceover=BlankClip(last,{0}) ++BlankClip(last, {1}).AudioDub(DirectShowSource(""{2}"").ResampleAudio(44100)).Trim({3}, {4})
MixAudio(last,voiceover)", clip.FrameOffset, clip.FrameEnd, clip.FileName, clip.FrameStart, clip.FrameEnd));
            }

			// TODO: calc abs path based on exe
			var templateFile = GetFileFromThisAppDirectory(TEMPLATE_AVS);
			var templateStr = File.ReadAllText(templateFile);
			var strVideoClips = (Proj.ClipsVideo.Count <= 1)
				? sbClips.ToString()
				: "UnalignedSplice( \\\n" + sbClips.ToString() + "\\\n)";
			// TODO: inject project properties
			var outputStr = templateStr
				.Replace("{proj-fps}", "" + Proj.FrameRate)
				.Replace("{proj-width}", "" + Proj.Width)
				.Replace("{proj-height}", "" + Proj.Height)
				.Replace("{video-clips}", strVideoClips)
				.Replace("{collectpixeltypestat-videos}", sbClipStats.ToString())
				.Replace("{audio-clips}", sbAudio.ToString())
			;

			File.WriteAllText(fileOut, outputStr);
		}

		public static void RebuildAuxillaryFiles(VidkaProj proj, string projFilename, MetaGeneratorInOtherThread metaGenerator, bool newFilenames)
		{
			foreach (var clip in proj.ClipsVideo)
			{
				if (clip is VidkaClipTextSimple)
				{
					var vclip = (VidkaClipTextSimple)clip;
					if (newFilenames)
						vclip.FileName = GetAuxillaryProjFile(projFilename, MakeUniqueFilename_AuxSimpleText());
					RebuildAuxillaryFile_SimpleText(vclip, proj, metaGenerator);
				}
			}
		}

		public static void RebuildAuxillaryFile_SimpleText(VidkaClipTextSimple vclip, VidkaProj Proj, MetaGeneratorInOtherThread metaGenerator)
		{
			var filename = vclip.FileName;
			VidkaIO.MakeSureFolderExistsForFile(filename);
			VidkaImaging.RenderSimpleTextVideoClipToFile(vclip, Proj, filename);
			metaGenerator.RequestThumbsOnly(filename, true);
		}

		#region ------------------- filenames and paths --------------------------

		public static string GetFileFromThisAppDirectory(string subpath) {
			return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), subpath);
		}

		public static string GetAuxillaryProjFile(string projFilename, string filename)
		{
			var dirSara = (projFilename == null)
				? GetFileFromThisAppDirectory("tmp")
				: Path.Combine(Path.GetDirectoryName(projFilename), COMPILE_DIR);
			return Path.Combine(dirSara, filename);
		}

		public static void MakeSureFolderExistsForFile(string filename)
		{
			var dirname = Path.GetDirectoryName(filename);
			if (Directory.Exists(dirname))
				return;
			Directory.CreateDirectory(dirname);
		}

		public static string MakeUniqueFilename(string template) {
			return String.Format(template, Guid.NewGuid().ToString().Replace("-", ""));
		}

		public static string MakeUniqueFilename_AuxSimpleText() {
			return MakeUniqueFilename("text-{0}.jpg");
		}

        public static string MakeUniqueFilename_Frame() {
			return MakeUniqueFilename("frame-{0}.jpg");
		}

		#endregion

	}
}
