using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using JumpListHelpers;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace JumpListManagerExample
{
    public partial class MainForm : JumpListMainFormBase
    {
        public MainForm()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            InitializeComponent();

            this.JumpListCommandReceived += new EventHandler<CommandEventArgs>(MainForm_JumpListCommandReceived);
        }

        private void MainForm_JumpListCommandReceived(object sender, CommandEventArgs e)
        {

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            string[] args = Environment.GetCommandLineArgs();

            // get the file name in shortcut parameter
            string shortcutParameter = args[1];

            Update(args[1]);
        }

        private void Update(string shortcutParameter)
        {
            try
            {
                // changes extension to .txt
                shortcutParameter = shortcutParameter + ".txt";

                // if the .txt file does not exist in the directory then it will be created
                if (!File.Exists(shortcutParameter))
                {
                    File.Create(shortcutParameter).Dispose();
                    // stores the value ' ' in the .txt file 
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(shortcutParameter, true))
                    {
                        file.WriteLine(" ");
                    }
                }

                // Read the value stored in the .txt file 
                string valor_txt = File.ReadAllText(shortcutParameter);

                // stores in the variable 'result' the option selected in the message box: Yes, No or Cancel
                var result = MessageBox.Show("Create Jumplist list with executables or directories? " +
                    "\n \nYes = Executables List \nNo = Directory List", "Atention", MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation);

                // if selected value is equal to 'Cancel' then perform this check
                if (result == DialogResult.Cancel)
                {
                    // If the stored value is 'Yes' then the variable 'result' becomes 'DialogResult.Yes'
                    if (valor_txt.Substring(0, 3) == "Yes")
                    {
                        result = DialogResult.Yes;
                    }
                    // If the stored value is 'No' then the variable 'result' becomes 'DialogResult.No'
                    else if (valor_txt.Substring(0, 2) == "No")
                    {
                        result = DialogResult.No;
                    }
                    // otherwise end the application
                    else
                    {
                        Application.Exit();
                    }
                }

                // changes .txt to .xml
                shortcutParameter = shortcutParameter.Replace(".txt", ".xml");

                // if the .xml file does not exist in the directory then the user is
                // alerted and the application is terminated
                if (!File.Exists(shortcutParameter))
                {
                    MessageBox.Show("The file '" + shortcutParameter + "' doesn't exist", "ERROR: Atention",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                // if the .xml file exists it will be loaded to get its values                        
                var xdoc = XDocument.Load(shortcutParameter);

                var items = from i in xdoc.Descendants("Settings")
                            select new
                            {
                                Category = (string)i.Attribute("Category"),
                                Name = (string)i.Attribute("Name"),
                                Path = (string)i.Attribute("Path")
                            };


                // changes .xml to .txt
                shortcutParameter = shortcutParameter.Replace(".xml", ".txt");

                // if user selects the option 'Yes', then the Jumplist list will be
                // loaded with the executables, but if the user selects the option 'No'
                // the list will be loaded with the folders where these executables are
                if (result == DialogResult.Yes)
                {
                    foreach (var item in items)
                    {
                        JumpListManager.AddCategoryLink(item.Category, item.Name, item.Path, item.Path);
                    }

                    // delete the text in the .txt file
                    File.WriteAllText(shortcutParameter, String.Empty);

                    // save the value 'Yes' nin the .txt file
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(shortcutParameter, true))
                    {
                        file.WriteLine("Yes");
                    }
                }
                else if (result == DialogResult.No)
                {
                    foreach (var item in items)
                    {
                        if (item.Path == "shell:AppsFolder")
                        {
                            JumpListManager.AddCategoryLink(item.Category, item.Name, item.Path, item.Path);
                        }
                        else
                        {
                            JumpListManager.AddCategoryLink(item.Category, item.Name, Path.GetDirectoryName(item.Path), Path.GetDirectoryName(item.Path));
                        }

                        // delete the text in the .txt file
                        File.WriteAllText(shortcutParameter, String.Empty);

                        // save the value 'No' nin the .txt file
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(shortcutParameter, true))
                        {
                            file.WriteLine("No");
                        }
                    }
                }

                // The XmlDocument class does not implement IDisposable,
                // so there's no way to force it to release it's resources at will. 
                // If you need to free that memory the only way to do that would be
                // xmlDocument = null; and garbage collection will handle the rest.
                xdoc = null;

                // Finaliza o aplicativo
                JumpListManager.Refresh();
                Application.Exit();

            }

            catch
            {
                Application.Exit();
            }
        }
    }
}