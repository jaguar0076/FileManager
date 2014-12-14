using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

//Add the style of music in the sorting process

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private static string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

        private static string[] LevelList = { "MediaYear", "MediaGenres", "MediaArtists", "MediaAlbum" };

        private Thread MyThread;

        private FileSystemWatcher Watcher;

        #endregion

        #region Delegates

        private delegate void SetText(string txt, Object o);

        private delegate string GetText(Object o);

        private delegate void SetButtonState(bool val, Object o);

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
                MyThread = new Thread(() => ThreadConstructTree(Invoke(new GetText(Get_Text), textBox2).ToString()));

                MyThread.Start();
            }
            else
            { Invoke(new SetText(AppendText), "Need powaaa maaaan!!", textBox1); }
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

        private void ThreadConstructTree(string dir)
        {
            if (Thread.CurrentThread.IsAlive)
            {
                Invoke(new SetButtonState(Set_ButtonState), false, button1);

                try
                {
                    ProcessXmlElement(ProcessXml.GetDirectoryXml(dir, FileExtensions));
                }
                catch (Exception e)
                { Utils.SaveLogFile(MethodBase.GetCurrentMethod(), e); }

                Invoke(new SetButtonState(Set_ButtonState), true, button1);
            }
        }

        private void ProcessXmlElement(XElement xResult)
        {
            //Interesting to see if we can extract the years, Artists, Albums before

            //When we run the analysis more than once the results seems dupplicated

            DbUtils.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS File (file_id INTEGER PRIMARY KEY AUTOINCREMENT, path VARCHAR(200), title VARCHAR(100), extension VARCHAR(4), track VARCHAR(4), album VARCHAR(100), year VARCHAR(4), artists VARCHAR(200), genres VARCHAR(50), hash VARCHAR(200) UNIQUE)");

            DbUtils.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx1_File_hash ON File(hash)");

            DbUtils.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx2_File_year ON File(year)");

            Utils.ComputeLevel(0, 0, "", ProcessXml.XFInfoList, LevelList);

            DbUtils.CloseConnection();

            /*DbUtils.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS File (file_id INTEGER PRIMARY KEY AUTOINCREMENT, path VARCHAR(200), title VARCHAR(100), extension VARCHAR(4), track VARCHAR(4), album VARCHAR(100), year VARCHAR(4), artists VARCHAR(200), genres VARCHAR(50), hash VARCHAR(200) UNIQUE)");

            DbUtils.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx1_File_hash ON File(hash)");

            DbUtils.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx2_File_year ON File(year)");

            string CurrentDirectory = Directory.GetCurrentDirectory();

            foreach (var CurrMediaYear in ProcessXml.XFInfoList
                                   .GroupBy(i => i.MediaYear)
                                  .OrderBy(g => g.Key)
                                  .Select(g => g.Key))
            {
                //Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear);

                foreach (var CurrMediaArtists in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear)
                          .GroupBy(i => i.MediaArtists)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                {
                    //Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear + "\\" + Utils.NameCleanup(CurrMediaArtists));

                    foreach (var CurrAlbum in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear
                              && i.MediaArtists == CurrMediaArtists)
                          .GroupBy(i => i.MediaAlbum)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                    {
                        //Directory.CreateDirectory(CurrentDirectory + "\\" + CurrMediaYear + "\\" + Utils.NameCleanup(CurrMediaArtists) + "\\" + Utils.NameCleanup(CurrAlbum));

                        foreach (var CurrFile in ProcessXml.XFInfoList
                          .Where(i => i.MediaYear == CurrMediaYear
                              && i.MediaArtists == CurrMediaArtists
                              && i.MediaAlbum == CurrAlbum)
                              .OrderBy(g => g.MediaTrack))
                        {
                            string NewFile = CurrentDirectory + "\\" +
                                             CurrMediaYear + "\\" +
                                             Utils.NameCleanup(CurrMediaArtists) + "\\" +
                                             Utils.NameCleanup(CurrAlbum) + "\\" +
                                             CurrFile.MediaTrack +
                                             " - " +
                                             Utils.NameCleanup(CurrFile.MediaTitle) +
                                             CurrFile.MediaExtension;

                            DbUtils.ExecuteNonQuery("INSERT INTO File(path, title, extension, track, album, year, artists, genres, hash) values (?,?,?,?,?,?,?,?,?)",
                                                    CurrFile.FilePath, CurrFile.MediaTitle, CurrFile.MediaExtension, CurrFile.MediaTrack, CurrFile.MediaAlbum, CurrFile.MediaYear, CurrFile.MediaArtists, CurrFile.MediaGenres, CurrFile.MD5Hash);

                            //Invoke(new SetText(AppendText), "Copying to " + NewFile + " (" + CurrFile.MediaGenres + ")", textBox1);

                            //Exception occurs when a file is renamed

                            //System.IO.File.Copy(CurrFile.FilePath, NewFile, true);

                            //FileInfo fileInfo = new FileInfo(NewFile);

                            //fileInfo.IsReadOnly = false;
                        }
                    }
                }
            }

            DbUtils.CloseConnection();*/
        }

        #endregion

        #region Misc Functions

        private static void AppendText(string msg, Object o)
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
            Watcher = new FileSystemWatcher();

            string _path = "D:\\Ma musique\\";

            Watcher.Path = _path;

            Watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite;

            Watcher.Filter = "*.*";

            Watcher.IncludeSubdirectories = true;

            Watcher.Created += new FileSystemEventHandler(watcher_Created);

            Watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            Watcher.Changed += new FileSystemEventHandler(watcher_Changed);

            Watcher.Renamed += new RenamedEventHandler(watcher_Renamed);

            Watcher.EnableRaisingEvents = true;
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