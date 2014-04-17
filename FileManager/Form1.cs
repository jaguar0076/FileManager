using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

/*
 * prochaines étape:
 * 
 * - Créer un XML Reader lire le document et y appliquer le traitement XSLT
 * - ajouter un watcher sur les folder pour monitorer les changements, vérifier les changements détecté par le Watcher et 
 *   comparer le XML stocké et les folder ayant changés, ceci sera plus facile de stocker les modifications dans les folders
 *   en utilisant le XML et les nodes pour gérer les modifications dans l'arborescence. Toutes les informations devront être
 *   sérialisées facilement.
 * 
 *  Deux solutions techniques sont possibles:
 * - Création de plusieurs documents xml et comparaison entre ceux-ci pour détecter les changements entre deux versions et ne réanalyser que la portion qui nous intéresse
 * - Création d'un watcher qui va analyser le folder si un changement a été effectué et à quel endroit il a été fait
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
                    StringBuilder sb = new StringBuilder(DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xml");

                    XmlWriterSettings xws = new XmlWriterSettings();

                    xws.Indent = true;

                    xws.NewLineOnAttributes = false;

                    xws.OmitXmlDeclaration = true;

                    XmlWriter writer = XmlWriter.Create(sb.ToString(), xws);

                    ProcessXml.GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString(), FileExtensions).Save(writer);

                    writer.Flush();

                    writer.Close();

                    var myXslTrans = new XslCompiledTransform();

                    myXslTrans.Load("StyleSheet_2.xslt");

                    myXslTrans.Transform(sb.ToString(), "Xtrans_" + sb.ToString());
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

        #endregion

        private void ProcessEvent(EventArgs e)
        {
            //string a = e.GetType().ToString();

            //((RenamedEventArgs)e).ChangeType.GetType().Name()

            //Invoke(new set_Text(Append_Text), Utils.CheckGetPropertyValue(e, "Name") + Utils.CheckGetPropertyValue(e, "ChangeType"), textBox1);
        }
    }
}