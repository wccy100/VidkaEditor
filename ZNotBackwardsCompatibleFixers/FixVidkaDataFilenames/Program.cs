using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vidka.Core;
using Vidka.Core.Ops;

namespace FixVidkaDataFilenames
{
    class Program
    {
        private VidkaFileMapping_resource fileMapping;
        private VidkaFileMapping_resource_old fileMappingOld;

        public Program()
        {
            fileMapping = new VidkaFileMapping_resource();
            fileMappingOld = new VidkaFileMapping_resource_old();
            foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            {
                RecursiveSearchForVidkaData(arg);
            }

            //Console.ReadKey();
        }

        public void RecursiveSearchForVidkaData(string folder)
        {
            foreach (var dirname in Directory.GetDirectories(folder))
            {
                if (Path.GetFileName(dirname) == VidkaFileMapping_resource.DATA_FOLDER)
                    ProcessVidkaDataFolder(dirname);
                // recurse, bitch
                RecursiveSearchForVidkaData(dirname);
            }
        }

        public void ProcessVidkaDataFolder(string folder)
        {
            var parentFolder = Path.GetDirectoryName(folder);
            foreach (var filename in Directory.GetFiles(parentFolder))
            {
                if (String.Equals(Path.GetFileName(filename), "Thumbs.db", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (DragAndDropManager.IsFilenameVideo(filename) ||
                    DragAndDropManager.IsFilenameImage(filename) ||
                    DragAndDropManager.IsFilenameAudio(filename)) //TODO: audio
                {
                    var old_filenameMeta = fileMappingOld.AddGetMetaFilename(filename);
                    var old_filenameThumb = fileMappingOld.AddGetThumbnailFilename(filename);
                    var old_filenameWave = fileMappingOld.AddGetWaveFilenameDat(filename);
                    var old_filenameWaveJpg = fileMappingOld.AddGetWaveFilenameJpg(filename);

                    var filenameMeta = fileMapping.AddGetMetaFilename(filename);
                    var filenameThumb = fileMapping.AddGetThumbnailFilename(filename);
				    var filenameWave = fileMapping.AddGetWaveFilenameDat(filename);
				    var filenameWaveJpg = fileMapping.AddGetWaveFilenameJpg(filename);

                    if (DragAndDropManager.IsFilenameVideo(filename))
                    {
                        UpdateMetadataFilename(old_filenameMeta, filenameMeta, filename, MetaDataType.MetaXml);
                        UpdateMetadataFilename(old_filenameThumb, filenameThumb, filename, MetaDataType.Thumbs);
                        //UpdateMetadataFilename(old_filenameWave, filenameWave, filename, MetaDataType.WaveDat);
                        UpdateMetadataFilename(old_filenameWaveJpg, filenameWaveJpg, filename, MetaDataType.WaveJpg);
                    }
                    else if (DragAndDropManager.IsFilenameImage(filename))
                    {
                        UpdateMetadataFilename(old_filenameThumb, filenameThumb, filename, MetaDataType.Thumbs);
                    }
                    else if (DragAndDropManager.IsFilenameAudio(filename))
                    {
                        UpdateMetadataFilename(old_filenameMeta, filenameMeta, filename, MetaDataType.MetaXml);
                        //UpdateMetadataFilename(old_filenameWave, filenameWave, filename, MetaDataType.WaveDat);
                        UpdateMetadataFilename(old_filenameWaveJpg, filenameWaveJpg, filename, MetaDataType.WaveJpg);
                    }
                }
            }
        }

        private void UpdateMetadataFilename(string filenameMetaOld, string filenameMetaNew, string mediaFile, MetaDataType type)
        {
            if (!File.Exists(filenameMetaNew))
            {
                if (File.Exists(filenameMetaOld))
                {
                    cxzxc("Renamed: " + filenameMetaOld + " -> " + filenameMetaNew);
                    System.IO.File.Move(filenameMetaOld, filenameMetaNew);
                }
                else
                    cxzxc("!!! No meta for " + mediaFile + " (" + type.ToString() + ")");
            }
            else
                cxzxc("ALREADY EXISTS " + filenameMetaNew);
        }

        private void cxzxc(string p)
        {
            Console.WriteLine(p);
        }






        //====================================
        static void Main(string[] args)
        {
            new Program();
        }

        public enum MetaDataType
        {
            MetaXml = 1,
            Thumbs = 2,
            WaveDat = 3,
            WaveJpg = 4,
        }
    }
}
