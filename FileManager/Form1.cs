using System;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using System.Text;

/*
 * prochaines étape:
 * 
 * - Créer un XML writer pour stockerle document, le document va être historisé à la création de chaque version du document
 * - Créer un XML Reader lire le document et y appliquer le traitement XSLT
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

        private void showErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set_Text(debug, textBox1);
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
                    //Invoke(new set_Text(Append_Text), ProcessXml.GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString(), FileExtensions).ToString(), textBox1);

                    StringBuilder sb = new StringBuilder(DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xml");

                    XmlWriterSettings xws = new XmlWriterSettings();

                    xws.Indent = true;

                    xws.NewLineOnAttributes = false;

                    xws.OmitXmlDeclaration = true;

                    XmlWriter writer = XmlWriter.Create(sb.ToString(), xws);

                    ProcessXml.GetDirectoryXml(Invoke(new get_Text(Get_Text), textBox2).ToString(), FileExtensions).Save(writer);

                    writer.Close();

                    writer.Flush();

                    //var myXslTrans = new XslCompiledTransform();

                    //myXslTrans.Load("");

                    //myXslTrans.Transform("", "");
                }
                catch (Exception e)
                { Invoke(new set_Text(Append_Text), e.Message, textBox1); }

                Invoke(new set_ButtonState(Set_ButtonState), true, button1);
            }
        }

        #endregion

        #region Text Functions

        private static void Append_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetMethodValue(o, "AppendText", msg + Environment.NewLine);
            }
        }

        private static void Set_Text(string msg, Object o)
        {
            if (msg != String.Empty && msg != null)
            {
                Utils.CheckSetPropertyValue(o, "Text", msg);
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
    }
}