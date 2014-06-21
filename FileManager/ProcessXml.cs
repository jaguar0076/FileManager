using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FileManager
{
    internal static class ProcessXml
    {
        #region Variables

        public static XmlFileInfos XFInfos = new XmlFileInfos();

        #endregion

        #region Calculate folder size

        //Calculate folder size
        internal static long DirectorySize(DirectoryInfo dInfo, string[] FileExtensions)
        {
            long totalSize = dInfo.EnumerateFiles().Where(file => FileExtensions.Contains(file.Extension.ToLower())).Sum(file => file.Length);

            totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir, FileExtensions));

            return totalSize;
        }

        #endregion

        #region Process Xml

        // TODO: interesting to use Reflexion here to define a list of dynamics paramaters
        internal static void CollectXmlFileInfo(ref XElement Xnode, FileInfo file)
        {
            TagLib.File filetag = TagLib.File.Create(file.FullName);

            XElement XEl = (new XElement("File", //File node
                     new XAttribute("FilePath", file.FullName), //file path including the file name
                     new XAttribute("MediaTitle", Utils.Clean_String(filetag.Tag.Title)),//the title of the song
                     new XAttribute("MediaExtension", file.Extension), // the extension of the file
                     new XAttribute("MediaTrack", filetag.Tag.Track != 0 ? filetag.Tag.Track.ToString("D2") : Utils.FormatStringBasedOnRegex(file.Name, @"\d+", '0')), //the track number
                     new XAttribute("MediaAlbum", Utils.Clean_String(filetag.Tag.Album ?? "Undefined")), // the album of the song
                     new XAttribute("MediaYear", filetag.Tag.Year != 0 ? filetag.Tag.Year.ToString() : "Undefined"), // the year of the song
                     new XAttribute("MediaArtists", Utils.Clean_String(string.Join(",", filetag.Tag.Performers ?? filetag.Tag.AlbumArtists) ?? "Undefined")))); // the artists of the song

            XFInfos.Add(FromXElement(XEl));

            Xnode.Add(XEl);
        }

        //Serialization to an XmlFIleInfo object based on a XElement
        internal static XmlFileInfo FromXElement(XElement xElement)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xElement.ToString())))
            {
                var xmlSerializer = new XmlSerializer(typeof(XmlFileInfo));
                return (XmlFileInfo)xmlSerializer.Deserialize(memoryStream);
            }
        }

        internal static void ComputeFileInfo(FileInfo[] Flist, ref XElement Xnode, List<FileInfo> FileEx, List<FileInfo> AlreadyProcessedFiles, string[] FileExtensions)
        {
            foreach (var file in Flist.Except(AlreadyProcessedFiles).Except(FileEx)) //Exclude already treated ones and bad files
            {
                try
                {
                    CollectXmlFileInfo(ref Xnode, file);

                    AlreadyProcessedFiles.Add(file);
                }
                catch
                {
                    FileEx.Add(file);

                    ComputeFileInfo(Flist, ref Xnode, FileEx, AlreadyProcessedFiles, FileExtensions);
                }
            }
        }

        //main function, returns the XML of a entire directory (subdirectories included)
        internal static XElement GetDirectoryXml(String dir, string[] FileExtensions)
        {
            //Create DirectoryInfo
            DirectoryInfo Dir = new DirectoryInfo(dir);

            //Will be used to store the bad files
            List<FileInfo> BadFiles = new List<FileInfo>();

            //Will be used to store the processed files
            List<FileInfo> AlreadyProcessedFiles = new List<FileInfo>();

            //Mandatory to avoid invalid xml
            var info = new XElement("Root");

            //Gather file information
            ComputeFileInfo(Dir.EnumerateFiles().Where(f => FileExtensions.Contains(f.Extension.ToLower())).ToArray(),
                            ref info,
                            BadFiles,
                            AlreadyProcessedFiles,
                            FileExtensions);

            foreach (var subDir in Dir.EnumerateDirectories())
            {
                info.Add(GetDirectoryXml(subDir.FullName, FileExtensions));
            }

            return info;
        }

        #endregion
    }
}