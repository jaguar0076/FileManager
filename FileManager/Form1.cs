using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

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

        private const int DefaultFolderSize = 0;

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
                catch (Exception ex) { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }

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

        /*private static File ProcessFileInformations(FileInfo fio, Folder fo)
        {
            TagLib.File f = TagLib.File.Create(fio.FullName);

            return new File(fio.Name, fio.FullName, fio.Length, fio.CreationTime, fio.LastWriteTime, fio.Extension, fo, f.Tag.Title, f.Tag.Album, f.Tag.Year, f.Tag.AlbumArtists);
        }*/

        private static XElement GetDirectoryXml(String dir, string[] FileExtensions)
        {
            DirectoryInfo Dir = new DirectoryInfo(dir);

            var info = new XElement("Directory",
                       new XAttribute("Name", Dir.Name),
                       new XAttribute("DirectorySize", DirectorySize(Dir)),
                       new XAttribute("DirectoryPath", Dir.FullName),
                       new XAttribute("CreationTime", Dir.CreationTime),
                       new XAttribute("LastWriteTime", Dir.LastWriteTime),
                       new XAttribute("ParentFolder", Dir.Parent.FullName));

            foreach (var file in Dir.GetFiles())

                if (FileExtensions.Any(str => str == file.Extension))
                {
                    info.Add(new XElement("File",
                             new XAttribute("Name", file.Name),
                             new XAttribute("Extension", file.Extension),
                             new XAttribute("Length", file.Length),
                             new XAttribute("CreationTime", file.CreationTime),
                             new XAttribute("LastWriteTime", file.LastWriteTime),
                             new XAttribute("FilePath", file.FullName),
                             new XAttribute("FolderParent", file.Directory.FullName)));
                }
            foreach (var subDir in Dir.GetDirectories())
                info.Add(GetDirectoryXml(subDir.FullName, FileExtensions));

            return info;
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
    }
}