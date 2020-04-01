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
using System.ServiceProcess;

namespace AutoPatcher {
    public partial class mainWindow : Form {

        string GTAVLocation = null;
        Boolean isSteam;
        Boolean hadFatalError = false;

        FileStream logFileLocation = null;
        StreamWriter logFile = null; 

        public mainWindow() {
            InitializeComponent();

            //create log file, overwriting old one if necessary; used by log() function
            logFileLocation = new FileStream("log.txt", FileMode.Create);
            logFile = new StreamWriter(logFileLocation, Encoding.Default);

            //register event handlers
            btnFindGTAV.Click += new EventHandler(btnFindGTAV_Click);
            btnStart.Click += new EventHandler(btnStart_Click);
            tbGTAVPath.Click += new EventHandler(tbGTAVPath_Click);
            tbGTAVPath.KeyDown += new KeyEventHandler(tbGTAVPath_KeyDown);
            rbRestore.CheckedChanged += new EventHandler(rbRestore_CheckedChanged);
            this.Shown += new EventHandler(mainWindow_Shown);

            //used for checking registry for GTA V path
            string tempGTAVLocation = null;

            //Check if autopatcher files are accessible, and if not, throw an error
            if (File.Exists("rockstar.txt") && Directory.Exists("Backup") &&
                Directory.Exists("Common") && !isDirectoryEmpty("Common") &&
                Directory.Exists("Rockstar") && !isDirectoryEmpty("Rockstar") &&
                Directory.Exists("Steam") && !isDirectoryEmpty("Steam")) {
                    log("Found autopatcher files.");
            } else {
                showError("[01] Could not access autopatcher files. Please ensure you have not moved this executable.", true);
            }

            //Open Rockstar registry location
            RegistryKey GTAVRegistry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto V", true);
            if (GTAVRegistry != null) {
                tempGTAVLocation = GTAVRegistry.GetValue("InstallFolder").ToString();
            } else {
                //Open Steam registry location
                GTAVRegistry = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Rockstar Games\GTAV", true);
                if (GTAVRegistry != null) {
                    tempGTAVLocation = GTAVRegistry.GetValue("InstallFolderSteam").ToString();
                    //NOTE: this registry key only seems to exist on older versions, and I have never seen it point to the correct location either
                    //nonetheless, we check it just in case
                }
            }

            //check for null because Path.Combine can throw errors without it
            if (tempGTAVLocation != null && isValidGTAVPath(tempGTAVLocation)) {
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
                //set text box color to black; because it is gray to start
                tbGTAVPath.ForeColor = Color.Black;
                foundGTAV();
            } else {
                log("Could not find GTA V location automatically. Please enter it manually.");
            }
        }

        //method to check if GTA V is installed at a given folder location
        Boolean isValidGTAVPath(string GTAVLocation) {
            //we use GTAVLanguageSelect.exe because it exists on both versions, and has not changed with updates over the years
            //thus, it is extremely unlikely that an install will not have it
            string path = Path.Combine(GTAVLocation, "GTAVLanguageSelect.exe");
            if (File.Exists(path)) return true;
            return false;
        }

        //method to check if the game version at a given folder location is 1.27
        Boolean isOldPatch(string GTAVLocation) {
            string path = Path.Combine(GTAVLocation, @"update\update.rpf");
            try {
                long fileSize = new System.IO.FileInfo(path).Length;
                if (fileSize == 352569344) return true;
            } catch (System.IO.FileNotFoundException) {
                //no need to do anything here
            }
            return false;
        }

        //method to check if the game version at a given folder location is Steam or Rockstar
        Boolean isSteamVersion(string GTAVLocation) {
            //Steam version should have this file
            string path = Path.Combine(GTAVLocation, "steam_api64.dll");
            if (File.Exists(path)) return true;
            //Rockstar version should have this file
            path = Path.Combine(GTAVLocation, "GPUPerfAPIDX11-x64.dll");
            return !File.Exists(path);
        }

        //method to tell if a directory is empty (is a function for readability)
        public bool isDirectoryEmpty(string path) {
            //NOTE: will return false even if there are non-empty subfolders
            return !Directory.EnumerateFiles(path).Any();
        }

        //method to copy a file
        void copyFile(string fileName, string sourcePath="", string targetPath=null, int desiredSize=0) {
            //can't have GTAVLocation as a default parameter so this is what we're reduced to
            if (targetPath == null) targetPath = GTAVLocation;

            string sourceFile = Path.Combine(sourcePath, fileName);
            string targetFile = Path.Combine(targetPath, fileName);
            if (File.Exists(sourceFile)) {
                //this piece of code is in case we want to check file sizes to make backups more reliable
                if (desiredSize > 0) {
                    long fileSize = new System.IO.FileInfo(sourceFile).Length;
                    if (fileSize != desiredSize) {
                        log("Did not copy " + fileName + ". File is not the correct version (this may break backups).");
                    }
                }
                File.Copy(sourceFile, targetFile, true);
                log("Copied " + fileName + ".");
            } else {
                log("Could not copy " + fileName + ". File does not exist.");
            }
        }

        //method to display text in our output box, and then log it to a file
        void log(string output) {
            tbOutput.AppendText(output + Environment.NewLine);
            logFile.WriteLine(output);
            //StreamWriter is buffered, so we have to flush it
            logFile.Flush();
        }

        //method to show/log errors, and gracefully disable the program if they are fatal
        void showError(string msg, Boolean isFatal = false) {
            if (isFatal) {
                log("A fatal error has occurred and the program cannot continue." + Environment.NewLine);
                log("Error description:");
                log(msg);

                //disable all entry fields and buttons
                grpSelectPatch.Enabled = false;
                tbGTAVPath.Enabled = false;
                btnFindGTAV.Enabled = false;
                btnStart.Enabled = false;

                //tell other things to not enable them again
                hadFatalError = true;
            } else {
                log("A non-fatal error has occured, but the program will continue." + Environment.NewLine);
                log("Error description:");
                log(msg);
            }
        }

        //method to execute version checking and set variables
        //should only be run after GTAVLocation is verified to be valid
        void foundGTAV() {
            btnFindGTAV.Enabled = false;
            tbGTAVPath.Enabled = false;

            //avoids buttons enabling and text being output, if the autopatcher is in a non-functional state
            if (hadFatalError) return;

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
            //if you just show the dialog you will not ensure you get a result, so we check for the OK
            if (fbFindGTAV.ShowDialog() == DialogResult.OK) {
                if (isValidGTAVPath(fbFindGTAV.SelectedPath)) {
                    GTAVLocation = fbFindGTAV.SelectedPath;
                    tbGTAVPath.Text = GTAVLocation;
                    //set text box color to black; because it is gray to start
                    tbGTAVPath.ForeColor = Color.Black;
                    foundGTAV();
                } else {
                    tbGTAVPath.Text = fbFindGTAV.SelectedPath;
                    //set text box color to black; because it is gray to start
                    tbGTAVPath.ForeColor = Color.Black;
                    ToolTip tt = new ToolTip();
                    //note that the y value is positive, but that moves the tooltip DOWN
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
                //exception is called ex because eventargs are already e
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

                foreach (Process gtavProcess in Process.GetProcessesByName("GTA5")) {
                    string gtavFile = gtavProcess.MainModule.FileName;
                    FileVersionInfo gtavInfo = FileVersionInfo.GetVersionInfo(gtavFile);
                    if (gtavInfo.CompanyName.Contains("Rockstar")) {
                        log("Please ensure you have closed the game before continuing. This program will pause until you do so.");
                        gtavProcess.WaitForExit();
                    }
                }

                log("Closing Rockstar Launcher and Social Club...");

                try {
                    foreach (Process launcher in Process.GetProcessesByName("Launcher")) {
                        string launcherFile = launcher.MainModule.FileName;
                        FileVersionInfo launcherInfo = FileVersionInfo.GetVersionInfo(launcherFile);
                        if (launcherInfo.CompanyName.Contains("Rockstar")) {
                            launcher.Kill();
                            launcher.WaitForExit();
                        }
                    }
                    foreach (Process launcherPatcher in Process.GetProcessesByName("LauncherPatcher")) {
                        string launcherPatcherFile = launcherPatcher.MainModule.FileName;
                        FileVersionInfo launcherPatcherInfo = FileVersionInfo.GetVersionInfo(launcherPatcherFile);
                        if (launcherPatcherInfo.CompanyName.Contains("Rockstar")) {
                            launcherPatcher.Kill();
                            launcherPatcher.WaitForExit();
                        }
                    }
                    foreach (Process rockstarSvc in Process.GetProcessesByName("RockstarService")) {
                        rockstarSvc.Kill();
                        rockstarSvc.WaitForExit();
                    }
                    foreach (Process rockstarHelper in Process.GetProcessesByName("RockstarSteamHelper")) {
                        rockstarHelper.Kill();
                        rockstarHelper.WaitForExit();
                    }
                    foreach (Process socialclubHelper in Process.GetProcessesByName("SocialClubHelper")) {
                        socialclubHelper.Kill();
                        socialclubHelper.WaitForExit();
                    }
                } catch (Exception ex) {
                    //do nothing
                    //this is because the above processes can exit before we reference them
                    //as none of that is thread-safe (and probably can't be)
                    //so we can get errors
                    //probably I could test for a while and find all the exact exceptions that throws
                    //but I'm lazy, and this is simple
                }
                progress.Value = 5;
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
                progress.Value = 30;

                //downgrade
                log("Starting downgrade...");

                log("Launching Rockstar Launcher uninstaller (program will pause until setup is complete)...");
                using (Process launcherUninstaller = Process.Start(@"Common\uninstall.exe")) {
                    launcherUninstaller.WaitForExit();
                    Thread.Sleep(100);
                    while (Process.GetProcessesByName("Un_A").Length > 0) {
                        Thread.Sleep(100);
                    }
                }
                progress.Value = 40;
                if (isSteam) {
                    copyFile("GTA5.exe", "Steam", GTAVLocation);
                    copyFile("steam_api64.dll", "Steam", GTAVLocation);
                    copyFile("update.rpf", "Common", Path.Combine(GTAVLocation, "update"));
                    File.Copy(@"Steam\GTAVLauncher.exe", Path.Combine(GTAVLocation, "PlayGTAV.exe"), true);
                    progress.Value = 60;

                    //NOTE: With every update of the Steam version these registry locations must be checked
                    //As the key names change with the versions of Rockstar Launcher and Social Club that Steam installs
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\\Rockstar Games\\GTAV", "Launcher1019234", 1, RegistryValueKind.DWord);
                    Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam\Apps\271590", "SocialClub2052", 1, RegistryValueKind.DWord);
                    log("Steam registry modified to prevent first-time setups.");
                    progress.Value = 70;
                    
                    log("Installing Visual C++ 2008 SP1 Redistributable (program will pause until setup is complete)...");
                    //need command line option of /q to run install in background so user can't screw with it like we all know they would
                    ProcessStartInfo vcredistInfo = new ProcessStartInfo(@"common\vcredist_x64.exe", "/q");
                    using (Process vcredist = new Process()) {
                        vcredist.StartInfo = vcredistInfo;
                        vcredist.Start();
                        vcredist.WaitForExit();
                    }
                    progress.Value = 80;

                    log("Launching Social Club installer (program will pause until setup is complete)...");
                    using (Process socialClubInstaller = Process.Start(@"Steam\Social-Club-v1.1.7.8-Setup.exe")) {
                        socialClubInstaller.WaitForExit();
                    }
                } else {
                    copyFile("GTA5.exe", "Rockstar", GTAVLocation);
                    copyFile("GTAVLauncher.exe", "Rockstar", GTAVLocation);
                    copyFile("GFSDK_ShadowLib.win64.dll", "Rockstar", GTAVLocation);
                    copyFile("x64a.rpf", "Rockstar", GTAVLocation);
                    copyFile("update.rpf", "Common", Path.Combine(GTAVLocation, "update"));
                    progress.Value = 60;

                    log("Launching Social Club installer (program will pause until setup is complete)...");
                    using (Process socialClubInstaller = Process.Start(@"rockstar\Social-Club-v1.1.6.0-Setup.exe")) {
                        socialClubInstaller.WaitForExit();
                    }
                    progress.Value = 90;

                    log("Creating Offline shortcut on desktop...");
                    //yes, we have to use Windows Script Host to create shortcuts
                    //no, I don't like it either
                    IWshRuntimeLibrary.WshShell scriptHost = new IWshRuntimeLibrary.WshShell();
                    string desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    IWshRuntimeLibrary.IWshShortcut onlineShortcut = scriptHost.CreateShortcut(Path.Combine(desktopLocation, "GTA V Offline.lnk")) as IWshRuntimeLibrary.IWshShortcut;
                    onlineShortcut.TargetPath = Path.Combine(GTAVLocation, "GTAVLauncher.exe");
                    //I'm told setting WorkingDirectory is necessary, but I didn't actually bother testing that
                    onlineShortcut.WorkingDirectory = GTAVLocation;
                    //IconLocation expects an absolute path, so we have to get our current directory
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
                log("Launching Rockstar Launcher installer (program will pause until setup is complete)...");
                using (Process launcherInstaller = Process.Start(@"Common\Rockstar-Games-Launcher.exe")) {
                    launcherInstaller.WaitForExit();
                }
                progress.Value = 60;
                log("Launching Social Club installer (program will pause until setup is complete)...");
                using (Process socialClubInstaller = Process.Start(@"Common\Social-Club-Setup.exe")) {
                    socialClubInstaller.WaitForExit();
                }
            }
            progress.Value = 100;

            log(Environment.NewLine + "All done!");
        }

        void tbGTAVPath_Click(object sender, System.EventArgs e) {
            //if text is the default text, we clear it
            if (tbGTAVPath.Text == "Enter GTA V folder location or click Find") {
                tbGTAVPath.Text = "";
                tbGTAVPath.ForeColor = Color.Black;
            }
            //otherwise just let everything happen
        }

        void tbGTAVPath_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                //double if, because we need the else clause to trigger only if Enter was pressed
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
            //if Restore is checked, make sure we actually have things to restore
            if (rbRestore.Checked == true) {
                if (isDirectoryEmpty(@"Backup\Rockstar") && isDirectoryEmpty(@"Backup\Steam")) {
                    ToolTip tt = new ToolTip();
                    //yes, grpSelectPatch is 69 pixels tall; no, it was not actually intentional
                    tt.Show("Backup folders are empty.", grpSelectPatch, 0, 69, 3000);
                    //I could have unselected both radiobuttons, but I felt it was more clear if I forced the Downgrade button to select
                    rbDowngrade.Checked = true;
                    //beeping at people is the most important part of programming
                    //SystemSounds.Beep.Play();
                }
            }
        }

        void mainWindow_Shown(object sender, System.EventArgs e) {
            /*
            If we output too much stuff for tbOutput in the initialization phase
            Then it won't auto-scroll like it does everywhere else
            So this code will scroll it manually
            */
            tbOutput.Focus();
            tbOutput.SelectionStart = tbOutput.TextLength;
            tbOutput.ScrollToCaret();

            //remove focus on textbox
            lbGTAVPath.Focus();
        }
    }
}
