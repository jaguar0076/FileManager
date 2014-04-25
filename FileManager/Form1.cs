using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

/*
 * prochaines étape:
 * 
 * - ajouter un watcher sur les folder pour monitorer les changements, vérifier les changements détectés par le Watcher et 
 *   comparer le XML stocké et les folder ayant changés, ceci sera plus facile de stocker les modifications dans les folders
 *   en utilisant le XML et les nodes pour gérer les modifications dans l'arborescence (toutes les informations devront être
 *   sérialisées facilement).
 *   
 * Stocker les tags dans un fichier de configuration
 * 
 */

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

        #endregion

        #region Event

        private void button1_Click(object sender, EventArgs e)
        {
            PowerState state = PowerState.GetPowerState();
            //Just a simple check to see if the computer is plugged or if there is more than 15% left, because...you know....it sucks power on my laptop :)
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

        private static void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private static void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(sender, e);
        }

        private static void watcher_Created(object sender, FileSystemEventArgs e)
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

        private static void ProcessXmlElement(XElement XResult)
        {
            foreach (var year in XResult.Descendants("File")
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
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()));

                    foreach (var album in XResult.Descendants("File")
                          .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                              && i.Attribute("MediaArtists").Value == artist.ToString())
                          .GroupBy(i => i.Attribute("MediaAlbum").Value)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Key))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()));

                        foreach (var file in XResult.Descendants("File")
                          .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                              && i.Attribute("MediaArtists").Value == artist.ToString()
                              && i.Attribute("MediaAlbum").Value == album.ToString()))
                        {
                            //Invoke(new set_Text(Append_Text), "Copying " + file.Attribute("FilePath").Value, textBox1);

                            System.IO.File.Copy(file.Attribute("FilePath").Value, (Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()) + "\\" + file.Attribute("Name").Value), true);

                            FileInfo fileInfo = new FileInfo(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()) + "\\" + file.Attribute("Name").Value);

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

        private static string NameCleanup(string FileName)
        {
            return Regex.Replace(FileName, @"[\/?:*""><|]+", "-", RegexOptions.Compiled);
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

        private static void ProcessEvent(object source, FileSystemEventArgs e)
        {
            XElement Xel = new XElement("Root");

            bool iSDirectory = Path.GetExtension(e.FullPath).Equals("");

            if (iSDirectory)
            {
                Xel = ProcessXml.GetDirectoryXml(e.FullPath, FileExtensions);
            }
            else
            {
                ProcessXml.CollectXmlFileInfo(ref Xel, new FileInfo(e.FullPath));
            }

            ProcessXmlElement(Xel);
        }

        #endregion
    }
}