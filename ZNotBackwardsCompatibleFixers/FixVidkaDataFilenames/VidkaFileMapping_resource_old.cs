using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Vidka.Core.ExternalOps;
using Vidka.Core.Ops;

namespace FixVidkaDataFilenames
{
	/// <summary>
    /// This is the old version of VidkaFileMapping_resource class used before dec 19, 2015.
    /// It strips away the extension, which the new one does not to avoid metadata filename clash bug.
	/// </summary>
	public class VidkaFileMapping_resource_old : VidkaFileMapping
	{
		public const string DATA_FOLDER = ".vidkadata";

        public VidkaFileMapping_resource_old()
		{
		}

		public override string AddGetMetaFilename(string filename)
		{
            return PostfixInDataFolder(filename, ".xml");
		}
		public override string AddGetThumbnailFilename(string filename)
		{
            return PostfixInDataFolder(filename, "_thumbs.jpg");
		}
		public override string AddGetWaveFilenameDat(string filename)
		{
            return PostfixInDataFolder(filename, "_wave.dat");
		}
		public override string AddGetWaveFilenameJpg(string filename)
		{
            return PostfixInDataFolder(filename, "_wave.jpg");
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
            return Path.Combine(dirname, DATA_FOLDER, justNameNoExt + postfix);
            //return Path.Combine(dirname, DATA_FOLDER, justName + postfix);
        }

	}
}
