using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Vidka.Core.Model;
using Vidka.Core.ExternalOps;
using Vidka.Core.Properties;

namespace Vidka.Core
{
	public class VidkaIO
	{
        private const string TEMPLATE_AVS = "App_data/template.avs";
		private const string COMPILE_DIR = ".vidkacompile";
        private const string AvsTmpFilename = "GeneratedTemp-AvsScript.avs";
        private const string VDubTmpFilename = "GeneratedTemp-VDubScript.vdscript";

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
			var sbFiles = new StringBuilder();
			var sbClips = new StringBuilder();
            var sbClipsSplice = new StringBuilder();
			var sbClipStats = new StringBuilder();
            var sbPostOp = new StringBuilder();
            var sbAudio = new StringBuilder();
            var renderableProj = Proj.GetVideoClipsForRendering();
            var renderableClips = renderableProj.Clips;
            var lastClip = renderableClips.LastOrDefault();
            foreach (var file in renderableProj.Files)
            {
                if (file.Type == RenderableMediaFileType.DirectShowSource)
                {
                    //sbFiles.Append($"{file.VarName} = DirectShowSource(\"{file.FileName}\", audio=True, fps=proj_frameRate, convertfps=true)");
                    var audioParam = file.HasAudio ? ", atrack=-1" : "";
                    sbFiles.Append($"{file.VarName} = FFmpegSource2(\"{file.FileName}\"{audioParam})");
                }
                else if (file.Type == RenderableMediaFileType.ImageSource)
                    sbFiles.Append($"{file.VarName} = ImageSource(\"{file.FileName}\", start=0, end={renderableProj.MaxLengthOfImageClip}, fps=proj_frameRate)");
                else if (file.Type == RenderableMediaFileType.AudioSource)
                    sbFiles.Append($"{file.VarName} = DirectShowSource(\"{file.FileName}\")");
                sbFiles.Append("\n");
            }
            foreach (var clip in renderableClips)
			{
                sbPostOp.Clear();
                if (clip.HasCustomAudio)
                {
                    sbPostOp.Append(String.Format(".AddCustomAudio({0}, {1}, fstart={2}, fend={3})",
                        clip.CustomAudioFile.VarName, clip.CustomAudioOffset, clip.FrameStart, clip.FrameEnd - 1));
                }
                if (clip.IsMuted)
                    sbPostOp.Append(".MuteThisClip()");
                sbPostOp.Append((clip.PostOp ?? "").Replace("\n", ""));
                if (clip.ClipType == VideoClipRenderableType.Video)
                {
	                sbClips.Append(String.Format("{0} = NeutralClip({1}, {2}, {3}){4}\n",
                        clip.VarName, clip.VideoFile.VarName, clip.FrameStart, clip.FrameEnd-1, sbPostOp.ToString()));
                }
                else if (clip.ClipType == VideoClipRenderableType.Image
                    || clip.ClipType == VideoClipRenderableType.Text)
                {
	                sbClips.Append(String.Format("{0} = NeutralClipImage({1}, {2}){3}\n",
                        clip.VarName, clip.VideoFile.VarName, clip.LengthFrameCalc, sbPostOp.ToString()));
                }
			}

            foreach (var clip in renderableClips)
            {
                var lineEnding = (clip != lastClip) ? ", \\\n" : " ";
                sbPostOp.Clear();
                if (clip.EasingLeft > 0 || clip.EasingRight > 0)
                    sbPostOp.Append(String.Format(".Trim({0}, {1})",
                        clip.EasingLeft, clip.LengthFrameCalc - clip.EasingRight));
                foreach (var mix in clip.MixesAudioFromVideo)
                {
                    sbPostOp.Append(String.Format(".MixAudioFromClip({0}, {1}, {2}, {3})",
                        mix.ClipVarName, mix.FrameStart, mix.FrameEnd, mix.FrameOffset));
                }
                sbClipsSplice.Append(String.Format("\t{0}{1}{2}",
                    clip.VarName, sbPostOp.ToString(), lineEnding));
            }

            // clip stats
            foreach (var clip in renderableProj.StatVideos)
            {
                sbClipStats.Append(String.Format("collectpixeltypestat({0}, {1})\n",
                    clip.VideoFile.VarName, clip.LengthFrameCalc));
            }

            foreach (var clip in renderableProj.AudioClips)
            {
                sbPostOp.Clear();
                sbPostOp.Append((clip.PostOp ?? "").Replace("\n", ""));
                sbAudio.Append(String.Format(@"
voiceover=BlankClip(last,{0}) ++BlankClip(last, {1}).AudioDub(DirectShowSource(""{2}"", fps=proj_frameRate, convertfps=true).ResampleAudio(44100)).Trim({3}, {1}){4}
MixAudio(last,voiceover, clip1_factor=1, clip2_factor=1)", clip.FrameOffset, clip.FrameEnd, clip.FileName, clip.FrameStart, sbPostOp.ToString()));
            }

			var templateFile = GetFileFromThisAppDirectory(TEMPLATE_AVS);
			var templateStr = File.ReadAllText(templateFile);
			var strVideoClipsSplice = (Proj.ClipsVideo.Count <= 1)
                ? sbClipsSplice.ToString()
                : "UnalignedSplice( \\\n" + sbClipsSplice.ToString() + "\\\n)";
			// TODO: inject project properties
			var outputStr = templateStr
				.Replace("{proj-fps}", "" + Proj.FrameRate)
				.Replace("{proj-width}", "" + Proj.Width)
				.Replace("{proj-height}", "" + Proj.Height)
                .Replace("{video-files}", sbFiles.ToString())
                .Replace("{video-clips}", sbClips.ToString() + "\n\n" + strVideoClipsSplice)
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

        public static void MakeVDubScriptForOpenTheseVideosAndStartRender(string[] aviFilenames, string outputAviFile, string outputVdubScriptFile)
        {
            // VirtualDub.Open(U"F:\_poligon\barumini-1.vidka.avi");
            // VirtualDub.Append(U"F:\_poligon\barumini-2.vidka.avi");
            // VirtualDub.SaveAVI(U"F:\_poligon\barumini.avi");

            var sbFiles = new StringBuilder();

            var isFirst = true;
            foreach (var file in aviFilenames)
            {
                sbFiles.Append(String.Format(isFirst
                    ? "VirtualDub.Open(U\"{0}\");\n"
                    : "VirtualDub.Append(U\"{0}\");\n", file));
                isFirst = false;
            }
            sbFiles.Append(String.Format("VirtualDub.SaveAVI(U\"{0}\");\n", outputAviFile));

            var templateFile = GetFileFromThisAppDirectory(Settings.Default.VDubRawConcatScriptTemplate);
            var templateStr = File.ReadAllText(templateFile);
            var outputStr = templateStr.Replace("{open-appends-and-save}", sbFiles.ToString());
            File.WriteAllText(outputVdubScriptFile, outputStr);
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
            return String.Format(template, MakeGuidWord());
		}

        public static string MakeGuidWord() {
			return Guid.NewGuid().ToString().Replace("-", "");
		}

		public static string MakeUniqueFilename_AuxSimpleText() {
			return MakeUniqueFilename("text-{0}.jpg");
		}

        public static string MakeUniqueFilename_Frame() {
			return MakeUniqueFilename("frame-{0}.jpg");
		}

        public static string GetGeneratedAvsTmpFilename() {
            return GetFileFromThisAppDirectory(AvsTmpFilename);
        }
        public static string GetGeneratedVDubTmpFilename() {
            return GetFileFromThisAppDirectory(VDubTmpFilename);
        }

		#endregion
    }
}
