using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private ObservableCollection<File> FilesList = new ObservableCollection<File>();

        private ObservableCollection<Folder> FoldersList = new ObservableCollection<Folder>();

        private Thread MyThread;

        private string[] AudioExtensions = { ".mp3", ".wma", ".m4a", ".flac" };

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

            if (state.ACLineStatus == ACLineStatus.Online || ((int)state.BatteryLifePercent) > 20)
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
                    { Invoke(new set_Text(Append_Text), "File: " + ((File)x).FilePath, textBox1); }
                    else if (x.GetType() == typeof(Folder))
                    { Invoke(new set_Text(Append_Text), "Folder: " + ((Folder)x).FolderPath, textBox1); }
                }
                catch (Exception ex)
                { Invoke(new set_Text(Append_Text), ex.Message, textBox1); }
            }
        }

        #endregion

        #region Thread

        public void Thread_Construct_Tree()
        {
            if (Thread.CurrentThread.IsAlive)
            {
                try
                {
                    DirSearch(Invoke(new get_Text(Get_Text), textBox2).ToString(), null);

                    ComputeFolderLength(FoldersList);
                }
                catch (Exception ex) { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }
            }
        }

        #endregion

        #region Utils

        private void DirSearch(string Dir, Folder Fold)
        {
            long FileSize = 0;

            try
            {
                DirectoryInfo ParentDir = new DirectoryInfo(Dir);

                FoldersList.Add(new Folder(ParentDir.Name, ParentDir.FullName, DefaultFolderSize, ParentDir.CreationTime, ParentDir.LastWriteTime, Fold));

                foreach (FileInfo fi in ParentDir.GetFiles().Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && (x.Attributes & FileAttributes.System) == 0))
                {
                    FilesList.Add(ProcessFileInformations(fi, FoldersList[FoldersList.Count - 1]));

                    FileSize += fi.Length;
                }

                FoldersList[FoldersList.Count - 1].FolderLength += FileSize;

                foreach (DirectoryInfo di in ParentDir.GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && (x.Attributes & FileAttributes.System) == 0))
                {
                    DirSearch(di.FullName, FoldersList[FoldersList.IndexOf(FoldersList.Single(o => o.FolderPath == di.Parent.FullName))]);
                }
            }
            catch (Exception ex)
            { Invoke(new set_Text(Append_Text), ex.StackTrace, textBox1); }
        }

        private void ComputeFolderLength(ObservableCollection<Folder> Flist)
        {
            foreach (Folder f in Flist.Where(o => o.FolderLength == DefaultFolderSize))
            {
                f.FolderLength = DirectorySize(f, Flist);
            }
        }

        static long DirectorySize(Folder f, ObservableCollection<Folder> Flist)
        {
            long totalSize = Flist.Where(o => o.FolderParent == f).Sum(o => o.FolderLength);

            totalSize += Flist.Where(o => o.FolderParent == f).Sum(o => DirectorySize(o, Flist));

            return totalSize;
        }

        private File ProcessFileInformations(FileInfo fio, Folder fo)
        {
            try
            {
                TagLib.File f = TagLib.File.Create(fio.FullName);
                return new File(fio.Name, fio.FullName, fio.Length, fio.CreationTime, fio.LastWriteTime, fio.Extension, fo, f.Tag.Title, f.Tag.Album, f.Tag.Year, f.Tag.AlbumArtists);
            }
            catch (Exception ex)
            {
                Append_Text("Error while getting file: " + ex.StackTrace, textBox1);
                return new File();
            }
        }

        #endregion

        #region Text Functions

        static void Append_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetMethodValue(o, "Text", Utils.CheckGetMethodValue(o, "Text") + Environment.NewLine + msg);
            }
        }

        static string Get_Text(Object o)
        {
            string RtrnString = String.Empty;

            try
            {
                RtrnString = Utils.CheckGetMethodValue(o, "Text");
            }
            catch (Exception ex)
            { Append_Text(ex.StackTrace, o); }

            return RtrnString;
        }

        #endregion

        #region Object Functions
        #endregion
    }
}