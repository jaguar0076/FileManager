using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;

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

        private ObservableCollection<File> FilesList = new ObservableCollection<File>();

        private ObservableCollection<Folder> FoldersList = new ObservableCollection<Folder>();

        private Thread MyThread;

        private string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

        private const int DefaultFolderSize = 0;

        #endregion

        #region Delegates

        private delegate void set_Text(string txt, Object o);

        private delegate string get_Text(Object o);

        #endregion

        #region Gui

        public Form1()
        {
            InitializeComponent();

            FilesList.CollectionChanged += HandleChange;

            FoldersList.CollectionChanged += HandleChange;
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

        #region Events

        private void HandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var x in e.NewItems)
            {
                try
                {
                    if (x.GetType() == typeof(File))
                    {
                        Invoke(new set_Text(Append_Text), "File: " + ((File)x).FilePath, textBox1);
                    }
                    else if (x.GetType() == typeof(Folder))
                    {
                        Invoke(new set_Text(Append_Text), "Folder: " + ((Folder)x).FolderPath, textBox1);
                    }
                }
                catch (Exception ex)
                { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }
            }
        }

        #endregion

        #region Thread

        private void Thread_Construct_Tree()
        {
            if (Thread.CurrentThread.IsAlive)
            {
                try
                {
                    //DirSearch(Invoke(new get_Text(Get_Text), textBox2).ToString(), null);

                    Invoke(new set_Text(Append_Text), GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString()).ToString(), textBox1);

                    //ComputeFolderLength(FoldersList);
                }
                catch (Exception ex) { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }
            }
        }

        #endregion

        #region Utils

        /*private void DirSearch(string Dir, Folder Fold)
        {
            long FileSize = 0;

            try
            {
                DirectoryInfo ParentDir = new DirectoryInfo(Dir);

                FoldersList.Add(new Folder(ParentDir.Name, ParentDir.FullName, DefaultFolderSize, ParentDir.CreationTime, ParentDir.LastWriteTime, Fold));

                foreach (FileInfo fi in ParentDir.GetFiles())
                {
                    if (FileExtensions.Any(str => str == fi.Extension))
                    {
                        FilesList.Add(ProcessFileInformations(fi, FoldersList[FoldersList.Count - 1]));

                        FileSize += fi.Length;
                    }
                }

                FoldersList[FoldersList.Count - 1].FolderLength += FileSize;

                foreach (DirectoryInfo di in ParentDir.GetDirectories())
                {
                    DirSearch(di.FullName, FoldersList[FoldersList.IndexOf(FoldersList.Single(o => o.FolderPath == di.Parent.FullName))]);
                }
            }
            catch (Exception ex)
            { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }
        }

        private static void ComputeFolderLength(ObservableCollection<Folder> Flist)
        {
            foreach (Folder f in Flist.Where(o => o.FolderLength == DefaultFolderSize))
            {
                f.FolderLength = DirectorySize(f, Flist);
            }
        }

        private static long DirectorySize(Folder f, ObservableCollection<Folder> Flist)
        {
            long totalSize = Flist.Where(o => o.FolderParent == f).Sum(o => o.FolderLength);

            totalSize += Flist.Where(o => o.FolderParent == f).Sum(o => DirectorySize(o, Flist));

            return totalSize;
        }*/

        private static long DirectorySize(DirectoryInfo Dir, FileInfo[] FileList)
        {
            long totalSize = FileList.Where(o => o.Directory == Dir).Sum(o => o.Length);

            totalSize += FileList.Where(o => o.Directory == Dir).Sum(o => DirectorySize(o.Directory, FileList));

            return totalSize;
        }

        static long CalculateDirectorySize(DirectoryInfo directory, bool includeSubdirectories)
        {
            long totalSize = 0;
            // Examine all contained files.
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                totalSize += file.Length;
            }
            // Examine all contained directories.
            if (includeSubdirectories)
            {
                foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                {
                    totalSize += CalculateDirectorySize(dir, true);
                }
            }
            return totalSize;
        }

        /*private static File ProcessFileInformations(FileInfo fio, Folder fo)
        {
            TagLib.File f = TagLib.File.Create(fio.FullName);

            return new File(fio.Name, fio.FullName, fio.Length, fio.CreationTime, fio.LastWriteTime, fio.Extension, fo, f.Tag.Title, f.Tag.Album, f.Tag.Year, f.Tag.AlbumArtists);
        }*/

        private static XElement GetDirectoryXml(String dir)
        {
            DirectoryInfo Dir = new DirectoryInfo(dir);

            var info = new XElement("Directory",
                       new XAttribute("Name", Dir.Name),
                       new XAttribute("DirectorySize", CalculateDirectorySize(Dir, true)),
                       new XAttribute("DirectoryPath", Dir.FullName),
                       new XAttribute("CreationTime", Dir.CreationTime),
                       new XAttribute("LastWriteTime", Dir.LastWriteTime),
                       new XAttribute("ParentFolder", Dir.Parent.FullName));

            foreach (var file in Dir.GetFiles())
                info.Add(new XElement("File",
                         new XAttribute("Name", file.Name),
                         new XAttribute("Extension", file.Extension),
                         new XAttribute("Length", file.Length),
                         new XAttribute("CreationTime", file.CreationTime),
                         new XAttribute("LastWriteTime", file.LastWriteTime),
                         new XAttribute("FilePath", file.FullName),
                         new XAttribute("FolderParent", file.Directory.FullName)));

            foreach (var subDir in Dir.GetDirectories())
                info.Add(GetDirectoryXml(subDir.FullName));

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

        #endregion
    }
}