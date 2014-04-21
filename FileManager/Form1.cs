using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

/*
 * prochaines étape:
 * 
 * - ajouter un watcher sur les folder pour monitorer les changements, vérifier les changements détecté par le Watcher et 
 *   comparer le XML stocké et les folder ayant changés, ceci sera plus facile de stocker les modifications dans les folders
 *   en utilisant le XML et les nodes pour gérer les modifications dans l'arborescence. Toutes les informations devront être
 *   sérialisées facilement.
 * 
 *  Deux solutions techniques sont possibles:
 * - Création d'un watcher qui va analyser le folder si un changement a été effectué et à quel endroit il a été fait
 * 
 * Stocker les tags dans un fichier de configuration
 * 
 * File name can't contain \/:*?<>|
 * 
 * 
 */

namespace FileManager
{
    public partial class Form1 : Form
    {
        #region Variables

        private Thread MyThread;

        private static string[] FileExtensions = { ".mp3", ".wma", ".m4a", ".flac", ".ogg", ".alac", ".aiff" };

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

        #region Event

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

        /*static*/
        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            ProcessEvent(e);
        }

        /*static*/
        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(e);
        }

        /*static*/
        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(e);
        }

        /*static*/
        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            ProcessEvent(e);
        }

        #endregion

        #endregion

        #region Thread

        private void Thread_Construct_Tree()
        {
            if (Thread.CurrentThread.IsAlive)
            {
                Invoke(new set_ButtonState(Set_ButtonState), false, button1);

                try
                {
                    XElement XResult = ProcessXml.GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString(), FileExtensions);

                    foreach (var year in XResult.Descendants("File")
                                  .GroupBy(i => i.Attribute("MediaYear").Value)
                                  .Select(g => g.Key))
                    {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString());

                        foreach (var artist in XResult.Descendants("File")
                                  .Where(i => i.Attribute("MediaYear").Value == year.ToString())
                                  .GroupBy(i => i.Attribute("MediaArtists").Value)
                                  .Select(g => g.Key))
                        {
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()));

                            foreach (var album in XResult.Descendants("File")
                                  .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                                      && i.Attribute("MediaArtists").Value == artist.ToString())
                                  .GroupBy(i => i.Attribute("MediaAlbum").Value)
                                  .Select(g => g.Key))
                            {
                                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()));

                                foreach (var file in XResult.Descendants("File")
                                  .Where(i => i.Attribute("MediaYear").Value == year.ToString()
                                      && i.Attribute("MediaArtists").Value == artist.ToString()
                                      && i.Attribute("MediaAlbum").Value == album.ToString()))
                                {
                                    Invoke(new set_Text(Append_Text), "Copying " + file.Attribute("FilePath").Value, textBox1);

                                    System.IO.File.Copy(file.Attribute("FilePath").Value, (Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()) + "\\" + file.Attribute("Name").Value), true);

                                    FileInfo fileInfo = new FileInfo(Directory.GetCurrentDirectory() + "\\" + year.ToString() + "\\" + NameCleanup(artist.ToString()) + "\\" + NameCleanup(album.ToString()) + "\\" + file.Attribute("Name").Value);

                                    fileInfo.IsReadOnly = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                { Invoke(new set_Text(Append_Text), e.Message, textBox1); }

                Invoke(new set_ButtonState(Set_ButtonState), true, button1);
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
            FileSystemWatcher watcher = new FileSystemWatcher();

            int index = Assembly.GetExecutingAssembly().Location.LastIndexOf("\\");

            string _path = Assembly.GetExecutingAssembly().Location.Substring(0, index);

            watcher.Path = _path;

            watcher.EnableRaisingEvents = true;

            watcher.Created += new FileSystemEventHandler(watcher_Created);

            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            watcher.Changed += new FileSystemEventHandler(watcher_Changed);

            watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
        }

        private void ProcessEvent(EventArgs e)
        {
            //string a = e.GetType().ToString();

            //((RenamedEventArgs)e).ChangeType.GetType().Name()

            //Invoke(new set_Text(Append_Text), Utils.CheckGetPropertyValue(e, "Name") + Utils.CheckGetPropertyValue(e, "ChangeType"), textBox1);
        }

        #endregion
    }
}