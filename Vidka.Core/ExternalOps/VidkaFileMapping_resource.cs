using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Vidka.Core.ExternalOps
{
	/// <summary>
	/// Will place a folder .vidkadata into every folder with media files and will generate
	/// all meta and thumbs in there
	/// </summary>
	public class VidkaFileMapping_resource : VidkaFileMapping
	{
		public const string DATA_FOLDER = ".vidkadata";

		public VidkaFileMapping_resource()
		{
		}

		public override string AddGetMetaFilename(string filename)
		{
            return PostfixInDataFolder(filename, ".xml");
		}
		public override string AddGetThumbnailFilename(string filename)
		{
            return PostfixInDataFolder(filename, "-thumbs.jpg");
		}
		public override string AddGetWaveFilenameDat(string filename)
		{
            return PostfixInDataFolder(filename, "-wave.dat");
		}
		public override string AddGetWaveFilenameJpg(string filename)
		{
            return PostfixInDataFolder(filename, "-wave.jpg");
		}
		public override void MakeSureDataFolderExists(string filename)
        {
			var dataFolder = Path.GetDirectoryName(filename);
			if (!Directory.Exists(dataFolder))
				Directory.CreateDirectory(dataFolder);
		}

        private string PostfixInDataFolder(string filename, string postfix)
        {
            if (filename == null)
                return null;
            var justName = Path.GetFileName(filename);
            var justNameNoExt = Path.GetFileNameWithoutExtension(filename);
            var dirname = Path.GetDirectoryName(filename);
            //return Path.Combine(dirname, DATA_FOLDER, justNameNoExt + postfix);
            return Path.Combine(dirname, DATA_FOLDER, justName + postfix);
        }

	}
}
