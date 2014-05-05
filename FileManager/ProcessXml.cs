using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        internal static void CollectXmlFileInfo(ref XElement Xnode, FileInfo file)
        {
            TagLib.File filetag = TagLib.File.Create(file.FullName);

            Xnode.Add(new XElement("File",
                     new XAttribute("Name", file.Name),
                     new XAttribute("FilePath", file.FullName),
                     new XAttribute("MediaTitle", Clean_String(filetag.Tag.Title)),
                     new XAttribute("MediaAlbum", Clean_String(filetag.Tag.Album ?? "Undefined")),
                     new XAttribute("MediaYear", filetag.Tag.Year),
                     new XAttribute("MediaArtists", Clean_String(string.Join(",", filetag.Tag.Performers ?? filetag.Tag.AlbumArtists) ?? "Undefined"))));
        }

        internal static void ComputeFileInfo(FileInfo[] Flist, ref XElement Xnode, List<FileInfo> FileEx, string[] FileExtensions)
        {
            foreach (var file in Flist.Except(FileEx))
            {
                try
                {
                    CollectXmlFileInfo(ref Xnode, file);
                }
                catch
                {//Maybe it would be interesting to store all the corrects files too and not only the bad ones to avoid computing all the XML again
                    FileEx.Add(file);

                    ComputeFileInfo(Flist, ref Xnode, FileEx, FileExtensions);
                }
            }
        }

        //main function, returns the XML of a entire directory (subdirectories included)
        internal static XElement GetDirectoryXml(String dir, string[] FileExtensions)
        {
            DirectoryInfo Dir = new DirectoryInfo(dir);

            List<FileInfo> FileEx = new List<FileInfo>();

            var info = new XElement("Root");

            ComputeFileInfo(Dir.EnumerateFiles().Where(f => FileExtensions.Contains(f.Extension.ToLower())).ToArray(),
                            ref info,
                            FileEx,
                            FileExtensions);

            foreach (var subDir in Dir.EnumerateDirectories())
            {
                info.Add(GetDirectoryXml(subDir.FullName, FileExtensions));
            }

            return info;
        }

        #endregion

        #region String Cleanup

        internal static string Clean_String(string txt)
        {
            StringBuilder sb = new StringBuilder(txt);

            return sb.Replace("\0", string.Empty).ToString();
        }

        #endregion
    }
}