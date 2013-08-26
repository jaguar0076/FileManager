using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace FileManager
{
    static class ProcessXml
    {

        #region Calculate folder size

        private static long DirectorySize(DirectoryInfo dInfo)
        {
            long totalSize = dInfo.EnumerateFiles().Sum(file => file.Length);

            totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir));

            return totalSize;
        }

        #endregion

        #region Process Xml

        private static void ProcessFileInfo(ref XElement Xnode, FileInfo file)
        {
            TagLib.File filetag = TagLib.File.Create(file.FullName);

            Xnode.Add(new XElement("File",
                     new XAttribute("Name", file.Name),
                     new XAttribute("Extension", file.Extension),
                     new XAttribute("Length", file.Length),
                     new XAttribute("CreationTime", file.CreationTime),
                     new XAttribute("LastWriteTime", file.LastWriteTime),
                     new XAttribute("FilePath", file.FullName),
                     new XAttribute("FolderParent", file.Directory.FullName),
                     new XAttribute("MediaTitle", Clean_String(filetag.Tag.Title)),
                     new XAttribute("MediaAlbum", Clean_String(filetag.Tag.Album)),
                     new XAttribute("MediaYear", filetag.Tag.Year),
                     new XAttribute("MediaArtists", Clean_String(string.Join(",", filetag.Tag.AlbumArtists)))));
        }

        private static void ComputeFileInfo(FileInfo[] Flist, ref XElement Xnode, List<FileInfo> FileEx, string[] FileExtensions)
        {
            foreach (var file in Flist.Except(FileEx))
            {
                if (!FileExtensions.Any(str => str == file.Extension))
                { continue; }
                else
                {
                    try
                    {
                        ProcessFileInfo(ref Xnode, file);
                    }
                    catch
                    {
                        FileEx.Add(file);

                        ComputeFileInfo(Flist, ref Xnode, FileEx, FileExtensions);
                    }
                }
            }
        }

        internal static XElement GetDirectoryXml(String dir, string[] FileExtensions)
        {
            DirectoryInfo Dir = new DirectoryInfo(dir);

            List<FileInfo> FileEx = new List<FileInfo>();

            var info = new XElement("Directory",
                       new XAttribute("Name", Dir.Name),
                       new XAttribute("DirectorySize", DirectorySize(Dir)),
                       new XAttribute("DirectoryPath", Dir.FullName),
                       new XAttribute("CreationTime", Dir.CreationTime),
                       new XAttribute("LastWriteTime", Dir.LastWriteTime),
                       new XAttribute("ParentFolder", Dir.Parent.FullName));

            ComputeFileInfo(Dir.GetFiles(), ref info, FileEx, FileExtensions);

            foreach (var subDir in Dir.GetDirectories())
            {
                info.Add(GetDirectoryXml(subDir.FullName, FileExtensions));
            }

            return info;
        }

        #endregion

        #region String Cleanup

        private static string Clean_String(string txt)
        {
            StringBuilder sb = new StringBuilder(txt);

            return sb.Replace("\0", string.Empty).ToString();
        }

        #endregion
    }
}