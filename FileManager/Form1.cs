using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

/*
 * prochaines étape:
 * 
 * - implémentation de l'arborescence récupérée en XML
 * - ajouter un watcher sur les folder pour monitorer les changements, vérifier les changements détecté par le Watcher et 
 *   comparer le XML stocké et les folder ayant changés, ceci sera plus facile de stocker les modifications dans les folders
 *   en utilisant le XML et les nodes pour gérer les modifications dans l'arborescence. Toutes les informations devront être
 *   sérialisées facilement.
 * 
 */

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private Thread MyThread;

        private static string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

        private static string debug = "";

        #endregion

        #region Delegates

        private delegate void set_Text(string txt, Object o);

        private delegate string get_Text(Object o);

        private delegate void set_ButtonState(bool val, Object o);

        #endregion

        #region Gui

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PowerState state = PowerState.GetPowerState();

            if (state.ACLineStatus == ACLineStatus.Online || ((int)state.BatteryLifePercent) > 15)
            {
                MyThread = new Thread(new ThreadStart(Thread_Construct_Tree));
                MyThread.Start();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;

                button1.Enabled = true;

                button2.Text = "Modify";
            }
        }

        #endregion

        #region Thread

        private void Thread_Construct_Tree()
        {
            if (Thread.CurrentThread.IsAlive)
            {
                Invoke(new set_ButtonState(Set_ButtonState), false, button1);

                try
                {
                    Invoke(new set_Text(Append_Text), GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString(), FileExtensions).ToString(), textBox1);
                }
                catch (Exception e)
                { Invoke(new set_Text(Append_Text), e.Message, textBox1); }

                Invoke(new set_ButtonState(Set_ButtonState), true, button1);
            }
        }

        #endregion

        #region Utils

        private static long DirectorySize(DirectoryInfo dInfo)
        {
            long totalSize = dInfo.EnumerateFiles().Sum(file => file.Length);

            totalSize += dInfo.EnumerateDirectories().Sum(dir => DirectorySize(dir));

            return totalSize;
        }

        private static XElement ProcessFileInfo(XElement Xnode, FileInfo file)
        {
            try
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
                         new XAttribute("MediaTitle", filetag.Tag.Title ?? String.Empty),
                         new XAttribute("MediaAlbum", filetag.Tag.Album ?? String.Empty),
                         new XAttribute("MediaYear", filetag.Tag.Year),
                         new XAttribute("MediaArtists", string.Join(",", filetag.Tag.AlbumArtists) ?? String.Empty)));
            }
            catch (Exception e)
            { throw e; }

            return Xnode;
        }

        private static XElement GetDirectoryXml(String dir, string[] FileExtensions)
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

            info = ComputeFileInfo(Dir.GetFiles(), info, FileEx);

            foreach (var subDir in Dir.GetDirectories())
                info.Add(GetDirectoryXml(subDir.FullName, FileExtensions));

            return info;
        }

        private static XElement ComputeFileInfo(FileInfo[] Flist, XElement Xnode, List<FileInfo> FileEx)
        {
            foreach (FileInfo file in Flist.Where(f => !FileEx.Contains(f)))
            {
                if (FileExtensions.Any(str => str == file.Extension)) //&& FileEx.Any(str => str != file.FullName))
                {
                    try
                    {
                        Xnode = ProcessFileInfo(Xnode, file);
                    }
                    catch (Exception e)
                    {
                        debug += file.FullName + "\n";//" file:" + file.FullName;

                        FileEx.Add(file);

                        ComputeFileInfo(Flist, Xnode, FileEx);
                    }
                }
            }

            return Xnode;
        }

        #endregion

        #region Text Functions

        private static void Append_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetMethodValue(o, "Text", Utils.CheckGetMethodValue(o, "Text") + Environment.NewLine + msg);
            }
        }

        private static void Set_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetMethodValue(o, "Text", msg);
            }
        }

        private static string Get_Text(Object o)
        {
            return Utils.CheckGetMethodValue(o, "Text");
        }

        private static void Set_ButtonState(bool state, Object o)
        {
            Utils.CheckSetMethodValue(o, "Enabled", state);
        }

        #endregion

        private void showErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Set_Text(debug, textBox1);
        }
    }
}