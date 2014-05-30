using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FileManager
{
    internal static class ProcessXml
    {
        #region Calculate folder size

        internal static long DirectorySize(DirectoryInfo dInfo, string[] FileExtensions)
        {
            long totalSize = dInfo.EnumerateFiles().Where(file => FileExtensions.Contains(file.Extension.ToLower())).Sum(file => file.Length);

            totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir, FileExtensions));

            return totalSize;
        }

        #endregion

        #region Process Xml

        //interesting to use Reflexion here to define a list of dynamics paramaters
        internal static void CollectXmlFileInfo(ref XElement Xnode, FileInfo file)
        {
            TagLib.File filetag = TagLib.File.Create(file.FullName);

            Xnode.Add(new XElement("File", //File node
                //new XAttribute("Name", file.Name), //file name
                     new XAttribute("FilePath", file.FullName), //file path including the file name
                     new XAttribute("MediaTitle", Utils.Clean_String(filetag.Tag.Title)),//the title of the song
                     new XAttribute("MediaExtension", file.Extension), // the extension of the file
                     new XAttribute("MediaTrack", filetag.Tag.Track != 0 ? filetag.Tag.Track.ToString("D2") : Regex.Match(file.Name, @"\d+").Value.PadLeft(2, '0')), //the track number
                     new XAttribute("MediaAlbum", Utils.Clean_String(filetag.Tag.Album ?? "Undefined")), // the album of the song
                     new XAttribute("MediaYear", filetag.Tag.Year != 0 ? filetag.Tag.Year.ToString() : "Undefined"), // the year of the song
                     new XAttribute("MediaArtists", Utils.Clean_String(string.Join(",", filetag.Tag.Performers ?? filetag.Tag.AlbumArtists) ?? "Undefined")))); // the artists of the song
        }

        internal static void ComputeFileInfo(FileInfo[] Flist, ref XElement Xnode, List<FileInfo> FileEx, List<FileInfo> FileOk, string[] FileExtensions)
        {
            foreach (var file in Flist.Except(FileEx).Except(FileOk)) //Exclude bad files and already treated ones
            {
                try
                {
                    CollectXmlFileInfo(ref Xnode, file);

                    FileOk.Add(file);
                }
                catch
                {
                    FileEx.Add(file);

                    ComputeFileInfo(Flist, ref Xnode, FileEx, FileOk, FileExtensions);
                }
            }
        }

        //main function, returns the XML of a entire directory (subdirectories included)
        internal static XElement GetDirectoryXml(String dir, string[] FileExtensions)
        {
            DirectoryInfo Dir = new DirectoryInfo(dir);

            List<FileInfo> BadFiles = new List<FileInfo>();

            List<FileInfo> OkFiles = new List<FileInfo>();

            var info = new XElement("Root");

            ComputeFileInfo(Dir.EnumerateFiles().Where(f => FileExtensions.Contains(f.Extension.ToLower())).ToArray(),
                            ref info,
                            BadFiles,
                            OkFiles,
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