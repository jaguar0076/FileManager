using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Data.SQLite;

//Add the style of music in the sorting process

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private static string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

        private Thread MyThread;

        private FileSystemWatcher watcher;

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

            //InitializeWatcher();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = @"D:\Ma musique";
        }

        #endregion

        #region Event

        private void button1_Click(object sender, EventArgs e)
        {
            PowerState state = PowerState.GetPowerState();
            //Just a simple check to see if the computer is plugged or if there is more than 15% left, because...you know....it uses lotsa power on my laptop :)
            if (state.ACLineStatus == ACLineStatus.Online || ((int)state.BatteryLifePercent) > 15)
            {
                MyThread = new Thread(() => Thread_Construct_Tree(Invoke(new get_Text(Get_Text), textBox2).ToString()));

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

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        #endregion

        #region Thread

        private void Thread_Construct_Tree(string Dir)
        {
            if (Thread.CurrentThread.IsAlive)
            {
                Invoke(new set_ButtonState(Set_ButtonState), false, button1);

                try
                {
                    ProcessXmlElement(ProcessXml.GetDirectoryXml(Dir, FileExtensions));
                }
                catch (Exception e)
                { Invoke(new set_Text(Append_Text), e.Message, textBox1); }

                Invoke(new set_ButtonState(Set_ButtonState), true, button1);
            }
        }

        private void ProcessXmlElement(XElement XResult)
        {
            //Interesting to see if we can extract the years, Artists, Albums before

            string CurrentDirectory = Directory.GetCurrentDirectory();

            //SQLiteConnection m_dbConnection;
            //m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            //m_dbConnection.Open();

            foreach (var CurrMediaYear in ProcessXml.XFInfoList
                                   .GroupBy(i => i.MediaYear)
                                  .OrderBy(g => g.Key)
                                  .Select(g => g.Key))
            {
                Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear);

                foreach (var CurrMediaArtists in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear)
                          .GroupBy(i => i.MediaArtists)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                {
                    Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear + "\\" + Utils.Name_Cleanup(CurrMediaArtists));

                    foreach (var CurrAlbum in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear
                              && i.MediaArtists == CurrMediaArtists)
                          .GroupBy(i => i.MediaAlbum)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                    {
                        Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear + "\\" + Utils.Name_Cleanup(CurrMediaArtists) + "\\" + Utils.Name_Cleanup(CurrAlbum));

                        foreach (var CurrFile in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear
                              && i.MediaArtists == CurrMediaArtists
                              && i.MediaAlbum == CurrAlbum)
                              .OrderBy(g => g.MediaTrack))
                        {
                            string NewFile = CurrentDirectory + "\\" +
                                             CurrMediaYear + "\\" +
                                             Utils.Name_Cleanup(CurrMediaArtists) + "\\" +
                                             Utils.Name_Cleanup(CurrAlbum) + "\\" +
                                             CurrFile.MediaTrack +
                                             " - " +
                                             Utils.Name_Cleanup(CurrFile.MediaTitle) +
                                             CurrFile.MediaExtension;

                            Invoke(new set_Text(Append_Text), "Copying to " + NewFile + " (" + CurrFile.MD5Hash + ")", textBox1);

                            System.IO.File.Copy(CurrFile.FilePath, NewFile, true);

                            FileInfo fileInfo = new FileInfo(NewFile);

                            fileInfo.IsReadOnly = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Misc Functions

        private static void Append_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetMethodValue(o, "AppendText", msg + Environment.NewLine);
            }
        }

        private static string Get_Text(Object o)
        {
            return Utils.CheckGetPropertyValue(o, "Text");
        }

        private static void Set_ButtonState(bool state, Object o)
        {
            Utils.CheckSetPropertyValue(o, "Enabled", state);
        }

        #endregion

        #region Watcher

        private void InitializeWatcher()
        {
            watcher = new FileSystemWatcher();

            string _path = "D:\\Ma musique\\";

            watcher.Path = _path;

            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite;

            watcher.Filter = "*.*";

            watcher.IncludeSubdirectories = true;

            watcher.Created += new FileSystemEventHandler(watcher_Created);

            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            watcher.Changed += new FileSystemEventHandler(watcher_Changed);

            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);

            watcher.EnableRaisingEvents = true;
        }

        private void ProcessEvent(object source, FileSystemEventArgs e)
        {
            XElement Xel = new XElement("Root");

            string Ext = Path.GetExtension(e.FullPath);

            if (!Ext.Equals("") && FileExtensions.Any(Ext.Equals))
            {
                ProcessXml.CollectXmlFileInfo(ref Xel, new FileInfo(e.FullPath));

                ProcessXmlElement(Xel);
            }
        }

        #endregion
    }
}