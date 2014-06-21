using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

//Add the style of music in the sort process
//Implement a class (serializable) with the XML infos
//Add a checksum for the comparaison

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private Thread MyThread;

        private static string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

        private FileSystemWatcher watcher;

        #endregion

        #region Delegates

        private delegate void set_Text(string txt, Object o);

        private delegate string get_Text(Object o);

        private delegate void set_ButtonState(bool val, Object o);

        private delegate void Append_Textbox(string txt);

        #endregion

        #region Gui

        public Form1()
        {
            InitializeComponent();

            InitializeWatcher();
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
                    XElement XResult = ProcessXml.GetDirectoryXml(Dir, FileExtensions);

                    ProcessXmlElement(XResult);
                }
                catch (Exception e)
                { Invoke(new set_Text(Append_Text), e.Message, textBox1); }

                Invoke(new set_ButtonState(Set_ButtonState), true, button1);
            }
        }

        private void ProcessXmlElement(XElement XResult)
        {
            /*foreach (var year in XResult.Descendants("File")
                                  .GroupBy(i => i.Attribute("MediaYear").Value)
                                  .OrderBy(g => g.Key)
                                  .Select(g => g.Key))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString());

                foreach (var artist in XResult.Descendants("File")
                          .Where(i => i.Attribute("MediaYear").Value == year.ToString())
                          .GroupBy(i => i.Attribute("MediaArtists").Value)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + Utils.Name_Cleanup(artist.ToString()));

                    foreach (var album in XResult.Descendants("File")
                          .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                              && i.Attribute("MediaArtists").Value == artist.ToString())
                          .GroupBy(i => i.Attribute("MediaAlbum").Value)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + Utils.Name_Cleanup(artist.ToString()) + "\\" + Utils.Name_Cleanup(album.ToString()));

                        foreach (var file in XResult.Descendants("File")
                          .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                              && i.Attribute("MediaArtists").Value == artist.ToString()
                              && i.Attribute("MediaAlbum").Value == album.ToString()))
                        {
                            string NewFile = Directory.GetCurrentDirectory() + "\\" +
                                             year.ToString() + "\\" +
                                             Utils.Name_Cleanup(artist.ToString()) + "\\" +
                                             Utils.Name_Cleanup(album.ToString()) + "\\" +
                                             file.Attribute("MediaTrack").Value +
                                             " - " +
                                             Utils.Name_Cleanup(file.Attribute("MediaTitle").Value +
                                             file.Attribute("MediaExtension").Value);

                            Invoke(new set_Text(Append_Text), "Copying to " + NewFile, textBox1);

                            Invoke(new set_Text(Append_Text), ProcessXml.XFInfos.Count.ToString(), textBox1);

                            //System.IO.File.Copy(file.Attribute("FilePath").Value, NewFile, true);

                            //FileInfo fileInfo = new FileInfo(NewFile);

                            //fileInfo.IsReadOnly = false;
                        }
                    }
                }
            }*/

            for (int i = 0; i < ProcessXml.XFInfos.Count; i++)
            {
                Invoke(new set_Text(Append_Text), ProcessXml.XFInfos[i].FilePath, textBox1);
            }

            // TODO: Implement foreach with le XmlFileInfos collection
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

            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;

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