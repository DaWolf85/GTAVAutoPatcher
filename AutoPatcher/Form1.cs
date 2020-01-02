using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AutoPatcher {
    public partial class mainWindow : Form {

        string GTAVLocation = null;
        Boolean isSteam;

        FileStream logFileLocation = null;
        StreamWriter logFile = null; 

        public mainWindow() {
            InitializeComponent();

            logFileLocation = new FileStream("log.txt", FileMode.Create);
            logFile = new StreamWriter(logFileLocation, Encoding.Default);

            FolderBrowserDialog folderBrowserDialog1;
            folderBrowserDialog1 = new FolderBrowserDialog();

            btnFindGTAV.Click += new EventHandler(btnFindGTAV_Click);
            btnStart.Click += new EventHandler(btnStart_Click);
            tbGTAVPath.Click += new EventHandler(tbGTAVPath_Click);
            tbGTAVPath.KeyDown += new KeyEventHandler(tbGTAVPath_KeyDown);
            rbRestore.CheckedChanged += new EventHandler(rbRestore_CheckedChanged);
            this.Shown += new EventHandler(mainWindow_Shown);
            string tempGTAVLocation = null;

            RegistryKey GTAVRegistry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V", true);
            if (GTAVRegistry != null) {
                tempGTAVLocation = GTAVRegistry.GetValue("InstallFolder").ToString();
            } else {
                GTAVRegistry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\GTAV", true);
                if (GTAVRegistry != null) {
                    tempGTAVLocation = GTAVRegistry.GetValue("InstallFolderSteam").ToString();
                }
            }

            if (isValidGTAVPath(tempGTAVLocation)) {
                GTAVLocation = tempGTAVLocation;
            } else {
                if (isValidGTAVPath(@"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V")) {
                    GTAVLocation = @"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V";
                } else if (isValidGTAVPath(@"C:\Program Files\Rockstar Games\Grand Theft Auto")) {
                    GTAVLocation = @"C:\Program Files\Rockstar Games\Grand Theft Auto";
                }
            }

            if (GTAVLocation != null) {
                tbGTAVPath.Text = GTAVLocation;
                tbGTAVPath.ForeColor = Color.Black;
                foundGTAV();
            } else {
                log("Could not find GTA V location automatically. Please enter it manually.");
            }
        }

        Boolean isValidGTAVPath(string GTAVLocation) {
            string path = GTAVLocation + @"\GTAVLanguageSelect.exe";
            if (File.Exists(path)) return true;
            return false;
        }

        Boolean isOldPatch(string GTAVLocation) {
            string path = GTAVLocation + @"\update\update.rpf";
            try {
                long fileSize = new System.IO.FileInfo(path).Length;
                //Console.WriteLine(fileSize);
                if (fileSize == 352569344) return true;
            } catch (System.IO.FileNotFoundException) {
                //no need to do anything here
            }
            return false;
        }

        Boolean isSteamVersion(string GTAVLocation) {
            string path = GTAVLocation + @"\steam_api64.dll";
            if (File.Exists(path)) return true;
            return false;
        }

        public bool isDirectoryEmpty(string path) {
            //NOTE: will return false even if there are non-empty subfolders
            return !Directory.EnumerateFiles(path).Any();
        }

        void copyFile(string fileName, string sourcePath="", string targetPath=null) {
            //can't have GTAVLocation as a default parameter so this is what we're reduced to
            if (targetPath == null) targetPath = GTAVLocation;

            string sourceFile = Path.Combine(sourcePath, fileName);
            string targetFile = Path.Combine(targetPath, fileName);
            File.Copy(sourceFile, targetFile, true);
            log("Copied " + fileName + ".");
        }

        void log(string output) {
            tbOutput.AppendText(output + Environment.NewLine);
            logFile.WriteLine(output);
            logFile.Flush();
        }

        void foundGTAV() {
            btnFindGTAV.Enabled = false;
            tbGTAVPath.Enabled = false;

            if (isOldPatch(GTAVLocation)) {
                log("Found GTA V and the version is 1.27.");
                rbRestore.Checked = true;
                isSteam = isSteamVersion(GTAVLocation);
            } else {
                log("Found GTA V and the version is NOT 1.27.");
                rbDowngrade.Checked = true;
                isSteam = isSteamVersion(GTAVLocation);
            }

            if (isSteam) log("Found Steam version of GTA V.");
            else log("Found Rockstar version of GTA V.");

            log(Environment.NewLine + "Ready to start!" + Environment.NewLine);

            btnStart.Enabled = true;
        }

        void btnFindGTAV_Click(object sender, System.EventArgs e) {
            if (fbFindGTAV.ShowDialog() == DialogResult.OK) {
                if (isValidGTAVPath(fbFindGTAV.SelectedPath)) {
                    GTAVLocation = fbFindGTAV.SelectedPath;
                    tbGTAVPath.Text = GTAVLocation;
                    tbGTAVPath.ForeColor = Color.Black;
                    foundGTAV();
                } else {
                    tbGTAVPath.Text = fbFindGTAV.SelectedPath;
                    tbGTAVPath.ForeColor = Color.Black;
                    ToolTip tt = new ToolTip();
                    tt.Show("This directory does not contain GTA V.", tbGTAVPath, 0, 20, 3000);
                }
            }
        }

        void btnStart_Click(object sender, System.EventArgs e) {
            btnStart.Enabled = false;
            grpSelectPatch.Enabled = false;

            //delete social club
            try {
                Directory.Delete(@"C:\Program Files\Rockstar Games\Social Club", true);
                Directory.Delete(@"C:\Program Files (x86)\Rockstar Games\Social Club", true);
                log("Social Club files deleted.");
            } catch (DirectoryNotFoundException ex) {
                log("Social Club files already deleted!");
            }
            //open registry with write access
            RegistryKey rockstarRegistry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games", true);
            //delete social club entries
            rockstarRegistry.DeleteSubKeyTree("Rockstar Games Social Club", false);
            log("Social Club registry keys removed.");
            progress.Value = 5;
            
            if (rbDowngrade.Checked == true) {
                //backup
                log("Starting backup...");
                if (isOldPatch(GTAVLocation)) {
                    log("Skipping backup, GTA V install is 1.27 (backup will be useless).");
                } else {
                    if (isSteam) {
                        copyFile("GTA5.exe", GTAVLocation, @"Backup\Steam");
                        copyFile("PlayGTAV.exe", GTAVLocation, @"Backup\Steam");
                        copyFile("GTAVLauncher.exe", GTAVLocation, @"Backup\Steam");
                        copyFile("steam_api64.dll", GTAVLocation, @"Backup\Steam");
                        copyFile("update.rpf", Path.Combine(GTAVLocation, "update"), @"Backup\Common");
                    } else {
                        copyFile("GTA5.exe", GTAVLocation, @"Backup\Rockstar");
                        copyFile("GTAVLauncher.exe", GTAVLocation, @"Backup\Rockstar");
                        copyFile("x64a.rpf", GTAVLocation, @"Backup\Rockstar");
                        copyFile("update.rpf", Path.Combine(GTAVLocation, "update"), @"Backup\Common");
                    }
                }
                progress.Value = 10;

                //downgrade
                log("Starting downgrade...");
                log("Launching Rockstar Launcher uninstaller (script will pause until setup is complete)...");
                using (Process launcherUninstaller = Process.Start(@"Common\uninstall.exe")) {
                    launcherUninstaller.WaitForExit();
                    Thread.Sleep(100);
                    while (Process.GetProcessesByName("Un_A").Length > 0) {
                        Thread.Sleep(100);
                    }
                }
                progress.Value = 25;
                if (isSteam) {
                    copyFile("GTA5.exe", "Steam", GTAVLocation);
                    copyFile("steam_api64.dll", "Steam", GTAVLocation);
                    copyFile("update.rpf", "Common", Path.Combine(GTAVLocation, "update"));
                    File.Copy(@"Steam\GTAVLauncher.exe", Path.Combine(GTAVLocation, "PlayGTAV.exe"), true);
                    progress.Value = 60;

                    log("Launching Social Club installer (script will pause until setup is complete)...");
                    using (Process socialClubInstaller = Process.Start(@"Steam\Social-Club-v1.1.7.8-Setup.exe")) {
                        socialClubInstaller.WaitForExit();
                    }
                } else {
                    copyFile("GTA5.exe", "Rockstar", GTAVLocation);
                    copyFile("GTAVLauncher.exe", "Rockstar", GTAVLocation);
                    copyFile("GFSDK_ShadowLib.win64.dll", "Rockstar", GTAVLocation);
                    copyFile("x64a.rpf", "Rockstar", GTAVLocation);
                    copyFile("update.rpf", "Common", Path.Combine(GTAVLocation, "update"));
                    progress.Value = 50;

                    log("Launching Social Club installer (script will pause until setup is complete)...");
                    using (Process socialClubInstaller = Process.Start(@"rockstar\Social-Club-v1.1.6.0-Setup.exe")) {
                        socialClubInstaller.WaitForExit();
                    }
                    progress.Value = 75;

                    log("Creating Offline shortcut on desktop...");
                    IWshRuntimeLibrary.WshShell scriptHost = new IWshRuntimeLibrary.WshShell();
                    string desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    IWshRuntimeLibrary.IWshShortcut onlineShortcut = scriptHost.CreateShortcut(Path.Combine(desktopLocation, "GTA V Offline.lnk")) as IWshRuntimeLibrary.IWshShortcut;
                    onlineShortcut.TargetPath = Path.Combine(GTAVLocation, "GTAVLauncher.exe");
                    onlineShortcut.WorkingDirectory = GTAVLocation;
                    onlineShortcut.IconLocation = Path.Combine(Directory.GetCurrentDirectory(), @"Rockstar\gta_v_icon.ico");
                    onlineShortcut.Arguments = "-scOfflineOnly";
                    onlineShortcut.Save();
                    
                    log("Opening file with further instructions...");
                    Process.Start("rockstar.txt");
                }
                progress.Value = 100;
            } else {
                log("Starting restore...");
                if (isSteam) {
                    copyFile("GTA5.exe", @"Backup\Steam", GTAVLocation);
                    copyFile("PlayGTAV.exe", @"Backup\Steam", GTAVLocation);
                    copyFile("steam_api64.dll", @"Backup\Steam", GTAVLocation);
                    copyFile("GTAVLauncher.exe", @"Backup\Steam", GTAVLocation);
                    copyFile("update.rpf", @"Backup\Common", Path.Combine(GTAVLocation, "update"));
                } else {
                    copyFile("GTA5.exe", @"Backup\Rockstar", GTAVLocation);
                    copyFile("GTAVLauncher.exe", @"Backup\Rockstar", GTAVLocation);
                    copyFile("x64a.rpf", @"Backup\Rockstar", GTAVLocation);
                    copyFile("update.rpf", @"Backup\Common", Path.Combine(GTAVLocation, "update"));
                }
                progress.Value = 25;
                log("Launching Rockstar Launcher installer (script will pause until setup is complete)...");
                using (Process launcherInstaller = Process.Start(@"Common\Rockstar-Games-Launcher.exe")) {
                    launcherInstaller.WaitForExit();
                }
                progress.Value = 60;
                log("Launching Social Club installer (script will pause until setup is complete)...");
                using (Process socialClubInstaller = Process.Start(@"Common\Social-Club-Setup.exe")) {
                    socialClubInstaller.WaitForExit();
                }
            }
            progress.Value = 100;

            log(Environment.NewLine + "All done!");
        }

        void tbGTAVPath_Click(object sender, System.EventArgs e) {
            if (tbGTAVPath.Text == "Enter GTA V folder location or click Find") {
                tbGTAVPath.Text = "";
                tbGTAVPath.ForeColor = Color.Black;
            }
        }

        void tbGTAVPath_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                //double if, because I need the else clause to trigger only if Enter was pressed
                if (isValidGTAVPath(tbGTAVPath.Text)) {
                    GTAVLocation = tbGTAVPath.Text;
                    foundGTAV();
                } else {
                    ToolTip tt = new ToolTip();
                    tt.Show("This directory does not contain GTA V.", tbGTAVPath, 0, 20, 3000);
                }
            }
        }

        private void rbRestore_CheckedChanged(Object sender, EventArgs e) {
            if (rbRestore.Checked == true) {
                if (isDirectoryEmpty(@"Backup\Steam") && isDirectoryEmpty(@"Backup\Steam")) {
                    ToolTip tt = new ToolTip();
                    tt.Show("Backup folders are empty.", grpSelectPatch, 0, 69, 3000);
                    rbDowngrade.Checked = true;
                    SystemSounds.Beep.Play();
                }
            }
        }

        void mainWindow_Shown(object sender, System.EventArgs e) {
            tbOutput.Focus();
            tbOutput.SelectionStart = tbOutput.TextLength;
            tbOutput.ScrollToCaret();

            //remove focus on textbox
            lbGTAVPath.Focus();
        }

        void applicationExit(object sender, EventArgs e) {
            try {
                logFile.Flush();
                logFile.Dispose();
                logFileLocation.Dispose();
            } catch { }
        }
    }
}
