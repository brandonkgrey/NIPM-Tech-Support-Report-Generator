using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NIPM_Tech_Support_Report_Generator
{
    public partial class MainWindow : Form
    {
        private FolderBrowserDialog folderBrowserDialog;
        private string tempDirPath;
        private Constants constants;

        public MainWindow()
        {
            InitializeComponent();


            string regKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\National Instruments\\NI Package Manager123\\CurrentVersion";
            string cliPath = Registry.GetValue(regKey, "CLIExecutableFullPath", "") as string;
            if (cliPath == "")
            {
                cliPath = Registry.GetValue(regKey, "Path", "") as string;
                cliPath = Path.Combine(cliPath, "nipkg.exe");
            }

            if (cliPath != null && cliPath != "nipkg.exe")
            {
                string nipmLocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                nipmLocalAppDataPath = Path.Combine(nipmLocalAppDataPath, "National Instruments\\NI Package Manager");
                Debug.WriteLine($"localAppDataPath - {nipmLocalAppDataPath}");
                Debug.Write($"CLI Path - {cliPath}");

                constants = new Constants(cliPath, nipmLocalAppDataPath);

                // Set attributes for folderBrowserDialog
                folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.DesktopDirectory;
                folderBrowserDialog.ShowNewFolderButton = false;
                folderBrowserDialog.Description = "Select the directory where you want the report generated.";

                //Set the textbox to the desktop
                pathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            else
            {
                InvalidNIPMException ex = new InvalidNIPMException();
                throw ex;
            }
        }

        //On click method that brings up the dialog to pick where you want the report generated
        private void browseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                pathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        //On click method that will kickoff the creation and collection of necessary files
        private void generateButton_Click(object sender, EventArgs e)
        {
            tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirPath);
            GenerateCliFiles(tempDirPath);
        }

        //Generates the CLI files and moves logs to generate the final zip support report
        private void GenerateCliFiles(string tempDirPath)
        {
            //Disable buttons and show loading cursor
            browseButton.Enabled = false;
            generateButton.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            string dest;
            if (pathTextBox.Text == "" || pathTextBox.Text == "<Report Destination Path>")
            {
                dest = Environment.SpecialFolder.Desktop.ToString();
            }
            else
            {
                dest = pathTextBox.Text;
            }

            //Generate Installed Packages text file
            System.Diagnostics.Process installedPackagesProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo installedPackagesStartInfo = new System.Diagnostics.ProcessStartInfo();
            installedPackagesStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            installedPackagesStartInfo.FileName = constants.CmdFullName;
            Debug.WriteLine($"/C \"\"{constants.NipkgPath}\" {constants.InstalledPkgsCmd} > \"{tempDirPath}\\{constants.InstalledPkgName}\"\"");
            installedPackagesStartInfo.Arguments = $"/C \"\"{constants.NipkgPath}\" {constants.InstalledPkgsCmd} > \"{tempDirPath}\\{constants.InstalledPkgName}\"\"";
            installedPackagesProcess.StartInfo = installedPackagesStartInfo;
            installedPackagesProcess.Start();

            //Generate Available Packages text file
            System.Diagnostics.Process availablePackagesProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo availablePackagesStartInfo = new System.Diagnostics.ProcessStartInfo();
            availablePackagesStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            availablePackagesStartInfo.FileName = constants.CmdFullName;
            Debug.WriteLine($"/C \"\"{constants.NipkgPath}\" {constants.AvailPkgsCmd} > \"{tempDirPath}\\{constants.AvailPkgsName}\"\"");
            availablePackagesStartInfo.Arguments = $"/C \"\"{constants.NipkgPath}\" {constants.AvailPkgsCmd} > \"{tempDirPath}\\{constants.AvailPkgsName}\"\"";
            availablePackagesProcess.StartInfo = availablePackagesStartInfo;
            availablePackagesProcess.Start();

            //Generate Registered Feeds text file
            System.Diagnostics.Process registeredFeedsProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo registeredFeedsStartInfo = new System.Diagnostics.ProcessStartInfo();
            registeredFeedsStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            registeredFeedsStartInfo.FileName = constants.CmdFullName;
            Debug.WriteLine($"/C \"\"{constants.NipkgPath}\" {constants.RegFeedsCmd} > \"{tempDirPath}\\{constants.RegFeedsName}\"\"");
            registeredFeedsStartInfo.Arguments = $"/C \"\"{constants.NipkgPath}\" {constants.RegFeedsCmd} > \"{tempDirPath}\\{constants.RegFeedsName}\"\"";
            registeredFeedsProcess.StartInfo = registeredFeedsStartInfo;
            registeredFeedsProcess.Start();

            //Generate Configurations text file
            System.Diagnostics.Process configurationsProcess = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo configurationsStartInfo = new System.Diagnostics.ProcessStartInfo();
            configurationsStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            configurationsStartInfo.FileName = constants.CmdFullName;
            Debug.WriteLine($"/C \"\"{constants.NipkgPath}\" {constants.ConfigCmd} > \"{tempDirPath}\\{constants.ConfigName}\"\"");
            configurationsStartInfo.Arguments = $"/C \"\"{constants.NipkgPath}\" {constants.ConfigCmd} > \"{tempDirPath}\\{constants.ConfigName}\"\"";
            configurationsProcess.StartInfo = configurationsStartInfo;
            configurationsProcess.Start();

            //if any processes are still running, wait for them to finish before creating the zip
            while (!installedPackagesProcess.HasExited
                   || !availablePackagesProcess.HasExited
                   || !registeredFeedsProcess.HasExited
                   || !configurationsProcess.HasExited)
            {
                System.Threading.Thread.Sleep(50);
            }

            //Copy NIPM Log directory to temp directory
            string nipmLogDir = $"{constants.NipmLocalAppDataPath}\\Logs";
            Debug.WriteLine($"NIPM Local App Data Dir: {nipmLogDir}");
            string logDir = Path.Combine(tempDirPath, "Logs");
            Directory.CreateDirectory(logDir);
            DirectoryCopy(nipmLogDir, logDir, true);

            //create report zip file
            ZipFile.CreateFromDirectory(tempDirPath, $"{dest}\\NIPM Report { DateTime.Now.ToString(@"yyyy\-MM\-dd\_hh.mm.ss")}.zip");


            //Notify User of successful generation
            MessageBox.Show("Your NI Package Manager Technical Support Report has been generated!", "Success!", MessageBoxButtons.OK);

            //Open folder containing the report
            Process.Start(dest);
            Application.Exit();
        }

        //Used to copy over the log directory
        private void DirectoryCopy(string sourceDirPath, string destDirPath, bool copySubDir)
        {
            //Get Directory and sub directories
            DirectoryInfo directory = new DirectoryInfo(sourceDirPath);
            DirectoryInfo[] subDirectories = directory.GetDirectories();

            if (!Directory.Exists(destDirPath))
            {
                Directory.CreateDirectory(destDirPath);
            }

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                string filePath = Path.Combine(destDirPath, file.Name);
                file.CopyTo(filePath, true);
            }

            if (copySubDir)
            {
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    string dirPath = Path.Combine(destDirPath, subDirectory.Name);
                    DirectoryCopy(subDirectory.FullName, dirPath, true);
                }
            }

        }
    }
}
