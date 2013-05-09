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

        private Thread myThread;

        #endregion

        #region Delegates

        private delegate void set_Text(string txt);

        private delegate string get_Text(object o);

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
                myThread = new Thread(new ThreadStart(Thread_Construct_Tree));
                myThread.Start();
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
                    { Invoke(new set_Text(Append_Text), "File: " + ((File)x).FilePath); }
                    else if (x.GetType() == typeof(Folder))
                    { Invoke(new set_Text(Append_Text), "Folder: " + ((Folder)x).FolderPath); }
                }
                catch (Exception ex)
                { Invoke(new set_Text(Append_Text), ex.Message); }
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
                }
                catch (Exception expt) { Invoke(new set_Text(Append_Text), expt.Message); }
            }
        }

        #endregion

        private void DirSearch(string Dir, Folder Fold)
        {
            try
            {
                DirectoryInfo ParentDir = new DirectoryInfo(Dir);

                FoldersList.Add(new Folder(ParentDir.Name, ParentDir.FullName, 1, ParentDir.CreationTime, ParentDir.LastWriteTime, Fold));

                foreach (FileInfo fi in ParentDir.GetFiles().Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && (x.Attributes & FileAttributes.System) == 0))
                {
                    FilesList.Add(new File(fi.Name, fi.FullName, fi.Length, fi.CreationTime, fi.LastWriteTime, fi.Extension, FoldersList[FoldersList.Count - 1]));
                }

                FoldersList[FoldersList.Count - 1].FolderLength = FilesList.Where(o => o.FileParentFolder == FoldersList[FoldersList.Count - 1]).Sum(o => o.FileLength);

                foreach (DirectoryInfo di in ParentDir.GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0 && (x.Attributes & FileAttributes.System) == 0))
                {
                    DirSearch(di.FullName, FoldersList[FoldersList.Count - 1]);
                }
            }
            catch (Exception excpt)
            { Append_Text(excpt.Message); }
        }

        #region Utils

        private void Append_Text(string msg)
        {
            textBox1.Text = textBox1.Text + Environment.NewLine + msg;
        }

        private string Get_Text(Object o)
        {
            string RtrnString = "";

            try
            {
                RtrnString = ((TextBox)o).Text;
            }
            catch (Exception ex)
            { Append_Text(ex.Message); }

            return RtrnString;
        }

        #endregion
    }
}