using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Elysium;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
using IWshRuntimeLibrary;

namespace DWARFSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Elysium.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (DWARFSetup.Properties.Settings.Default.isUninstalling)
            {
                agreementsCanvas.Visibility = System.Windows.Visibility.Hidden;
                uninstallCanvas.Visibility = System.Windows.Visibility.Visible;
                uninstallationTextBox.Text = DWARFSetup.Properties.Settings.Default.installationLocation;
            }
            //this.VisualTextRenderingMode = TextRenderingMode.Grayscale;
        }

        Thread installThread;

        public void SetImages(BitmapImage image)
        {
            
        }

        public delegate void SetImagesDelegate(BitmapImage img);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private static IntPtr LogonUser()
        {
            IntPtr accountToken = WindowsIdentity.GetCurrent().Token;

            return accountToken;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            agreementsCanvas.Visibility = System.Windows.Visibility.Hidden;
            installingCanvas.Visibility = System.Windows.Visibility.Visible;

            installThread = new Thread(Install);
            installThread.Start();
        }

        string temp_zip_file = "";

        WebClient installWC = new WebClient();

        private void Install()
        {
            installWC.DownloadProgressChanged += wc_DownloadProgressChanged;
            installWC.DownloadFileCompleted += wc_DownloadFileCompleted;
            temp_zip_file = System.IO.Path.GetTempFileName() + ".DWARF_install";
            installWC.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            installWC.DownloadFileAsync(new Uri("http://cdn.azuru.me/apps/dwarf/program/install.zip"), temp_zip_file);
            //installWC.DownloadFileAsync(new Uri("http://192.168.1.8/install.zip"), temp_zip_file);
            
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                string folder = "";
                this.Dispatcher.Invoke(delegate()
                {
                    folder = installationTextBox.Text;
                    currentStatusLabel.Content = "Creating installation folder...";
                });
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                this.Dispatcher.Invoke(delegate()
                {
                    currentStatusLabel.Content = "Extracting...";
                });
                try
                {
                    ZipFile.ExtractToDirectory(temp_zip_file, folder);
                }
                catch { }
                this.Dispatcher.Invoke(delegate()
                {
                    currentStatusLabel.Content = "Deleting temporary files...";
                });
                System.IO.File.Delete(temp_zip_file);

                this.Dispatcher.Invoke(delegate()
                {
                    folder = installationTextBox.Text;
                    currentStatusLabel.Content = "Creating application data folders...";
                });

                string initpath = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Azuru\\DWARF\\";

                if (!Directory.Exists(initpath + "\\Applications"))
                    Directory.CreateDirectory(initpath + "\\Applications");

                if (!Directory.Exists(initpath + "\\Applications\\Foundry"))
                    Directory.CreateDirectory(initpath + "\\Applications\\Foundry");

                if (!Directory.Exists(initpath + "\\Applications\\Foundry\\forms"))
                    Directory.CreateDirectory(initpath + "\\Applications\\Foundry\\forms");

                if (!Directory.Exists(initpath + "\\DWARFAppMan"))
                    Directory.CreateDirectory(initpath + "\\DWARFAppMan");

                if (!Directory.Exists(initpath + "\\Plugins"))
                    Directory.CreateDirectory(initpath + "\\Plugins");

                if (!Directory.Exists(initpath + "\\Temp"))
                    Directory.CreateDirectory(initpath + "\\Temp");

                if (!System.IO.File.Exists(initpath + "\\DWARFAppMan\\Applications.json"))
                    System.IO.File.WriteAllText(initpath + "\\DWARFAppMan\\Applications.json", "[]");

                if (!System.IO.File.Exists(initpath + "\\DWARFAppMan\\Plugins.json"))
                    System.IO.File.WriteAllText(initpath + "\\DWARFAppMan\\Plugins.json", "[]");


                if (!System.IO.File.Exists(initpath + "\\Applications\\Foundry\\app.js"))
                    System.IO.File.WriteAllText(initpath + "\\Applications\\Foundry\\app.js", DWARFSetup.Properties.Resources.app);

                if (!System.IO.File.Exists(initpath + "\\Applications\\Foundry\\app_info.dwarf"))
                    System.IO.File.WriteAllText(initpath + "\\Applications\\Foundry\\app_info.dwarf", DWARFSetup.Properties.Resources.app_info);

                if (!System.IO.File.Exists(initpath + "\\Applications\\Foundry\\icon.ico"))
                    DWARFSetup.Properties.Resources.icon.ToBitmap().Save(initpath + "\\Applications\\Foundry\\icon.ico");

                if (!System.IO.File.Exists(initpath + "\\Applications\\Foundry\\forms\\index.html"))
                    System.IO.File.WriteAllText(initpath + "\\Applications\\Foundry\\forms\\index.html", DWARFSetup.Properties.Resources.index);

                bool doCreateShortcut = false;
                this.Dispatcher.Invoke(delegate()
                {
                    if (shortCutCheckBox.IsChecked.Value)
                    {
                        currentStatusLabel.Content = "Creating desktop shortcut...";
                        doCreateShortcut = true;
                    }
                });
                if (doCreateShortcut)
                    createShortcut(folder + "\\DWARF.exe", "\"" + initpath + "\\Applications\\Foundry\\app_info.dwarf\"", initpath + "\\Applications\\Foundry\\icon.ico");
                this.Dispatcher.Invoke(delegate()
                {
                    currentStatusLabel.Content = "Creating uninstaller...";
                });
                CreateUninstaller(doCreateShortcut);

                this.Dispatcher.Invoke(delegate()
                {
                    currentStatusLabel.Content = "Creating startup registry key....";
                });
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                Key.SetValue("DWARFApplicationManager", folder + "\\DWARFAppMan.exe");
                this.Dispatcher.Invoke(delegate()
                {
                    currentStatusLabel.Content = "Finishing...";
                    installingCanvas.Visibility = System.Windows.Visibility.Hidden;
                    finishedCanvas.Visibility = System.Windows.Visibility.Visible;
                });
            }
            catch { }
        }

        private void createShortcut(string app, string args, string path_to_icon)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            WshShell shell = (WshShell)Activator.CreateInstance(System.Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(deskDir + "\\DWARF Foundry.lnk");
            shortcut.TargetPath = app;
            shortcut.Arguments = args;
            string icon = path_to_icon.Replace('\\', '/');
            shortcut.Description = "Launch DWARF Foundry";
            shortcut.IconLocation = icon;
            shortcut.Save();
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(delegate()
                {
                    downloadProgress.Value = e.ProgressPercentage;
                });
            }
            catch {  }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder to install DWARF to:";
                dlg.SelectedPath = installationTextBox.Text;
                dlg.ShowNewFolderButton = true;
                DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    installationTextBox.Text = dlg.SelectedPath;
                }
            }
        }

        private void LicenseTermsLabel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)sender;
            label.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 20, 171, 255));
        }

        private void LicenseTermsLabel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)sender;
            label.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 151, 255));
        }

        private void LicenseTermsLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)sender;
            label.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 121, 235));
        }

        private void LicenseTermsLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)sender;
            label.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 151, 255));
            System.Diagnostics.Process.Start("http://azuru.me/terms/DWARF");
        }

        private void PrivacyStatementLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Label label = (System.Windows.Controls.Label)sender;
            label.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 151, 255));
            System.Diagnostics.Process.Start("http://azuru.me/privacy/DWARF");
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            installButton.IsEnabled = true;
        }

        private void agreeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            installButton.IsEnabled = false;
        }

        private void shieldImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            runDWARFCheckBox.IsChecked = true;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(installationTextBox.Text + "\\DWARFAppMan.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Error starting DWARF Application Manager.  Please restart your computer to solve this issue.", "DWARF Setup", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (runDWARFCheckBox.IsChecked == true)
            { 
                string initpath = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Azuru\\DWARF\\";

                try
                {
                    System.Diagnostics.Process.Start(installationTextBox.Text + "\\DWARF.exe", "\"" + initpath + "\\Applications\\Foundry\\app_info.dwarf\"");
                }
                catch { }
            }

            installThread.Abort();
            installThread.Join();
            canClose = true;
            Environment.Exit(0);
            this.Close();
        }

        bool canClose = false;
        Canvas cancelledCanvas;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canClose)
            {
                try
                {
                    e.Cancel = true;
                    try
                    {
                        if (installThread != null)
                            installThread.Suspend();

                        if (agreementsCanvas.Visibility == System.Windows.Visibility.Visible)
                            cancelledCanvas = agreementsCanvas;
                        else if (installingCanvas.Visibility == System.Windows.Visibility.Visible)
                            cancelledCanvas = installingCanvas;
                        else if (finishedCanvas.Visibility == System.Windows.Visibility.Visible)
                            cancelledCanvas = finishedCanvas;

                        cancelledCanvas.Visibility = System.Windows.Visibility.Hidden;

                        askCancelCanvas.Visibility = System.Windows.Visibility.Visible;
                    }
                    catch {
                        e.Cancel = false;
                    }
                }
                catch
                {
                    canClose = true;
                    Environment.Exit(0);
                    this.Close();
                }
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (installThread != null)
            {
                installThread.Abort();
                installThread.Join();
            }
            DWARFSetup.Properties.Settings.Default.installationLocation = installationTextBox.Text;
            DWARFSetup.Properties.Settings.Default.Save();
            yes.IsEnabled = false;
            no.IsEnabled = false;
            new Thread(delegate()
            {
                Uninstall();
                canClose = true;
                this.Dispatcher.Invoke(delegate()
                {
                    Environment.Exit(0);
                    this.Close();
                });
            }).Start();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            askCancelCanvas.Visibility = System.Windows.Visibility.Hidden;
            cancelledCanvas.Visibility = System.Windows.Visibility.Visible;
            if (installThread != null)
                installThread.Resume();
        }

        private void CreateUninstaller(bool doCreateShortcut)
        {
            string folder = "";
            this.Dispatcher.Invoke(delegate()
            {
                folder = installationTextBox.Text;
            });

            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        key = parent.OpenSubKey("DWARF", true) ??
                              parent.CreateSubKey("DWARF");

                        if (key == null)
                        {
                            throw new Exception("Unable to create uninstaller");
                        }
                        WebClient wc = new WebClient();
                        string version = wc.DownloadString("http://cdn.azuru.me/apps/dwarf/program/version");
                        key.SetValue("DisplayName", "DWARF");
                        key.SetValue("ApplicationVersion", version);
                        key.SetValue("Publisher", "Azuru Networks");
                        key.SetValue("DisplayIcon", folder + "\\DWARF.exe");
                        key.SetValue("DisplayVersion", version);
                        key.SetValue("URLInfoAbout", "http://azuru.me/dwarf");
                        key.SetValue("Contact", "support@azuru.me");
                        key.SetValue("InstallLocation", folder);
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("CreatedShortcut", doCreateShortcut);
                        Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Azuru\\DWARFUninstaller");
                        string location = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Azuru\\DWARFUninstaller\\DWARFSetup.exe";
                        if (System.IO.File.Exists(location))
                            System.IO.File.Delete(location);
                        try
                        {
                            System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, location);
                            key.SetValue("UninstallString", "\"" + location + "\" uninstall \"" + folder + "\"");
                        }
                        catch {
                            this.Dispatcher.Invoke(delegate()
                            {
                                currentStatusLabel.Content = "Failed to create uninstaller!";
                            });
                            Thread.Sleep(2000);
                        }
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Exception: " + ex.Message);
                    /*throw new Exception(
                        "An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the command line.",
                        ex);*/
                }
            }
        }

        Thread uninstallThread;

        private void uninstallButton_Click(object sender, RoutedEventArgs e)
        {
            uninstallCanvas.Visibility = System.Windows.Visibility.Hidden;
            uninstallingCanvas.Visibility = System.Windows.Visibility.Visible;
            uninstallThread = new Thread(Uninstall);
            uninstallThread.Start();
        }

        private void Uninstall()
        {
            this.Dispatcher.Invoke(delegate()
            {
                downloadProgress.Value = 100;
            });
            if (System.Diagnostics.Process.GetProcessesByName("DWARF").Length > 0)
            {
                this.Dispatcher.Invoke(delegate()
                {
                    uninstallCurrentStatusLabel.Content = "Stopping DWARF instances...";
                });
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("DWARF"))
                {
                    proc.CloseMainWindow();
                    proc.Kill();
                    proc.WaitForExit();
                }
            }
            if (System.Diagnostics.Process.GetProcessesByName("DWARFAppMan").Length > 0)
            {
                this.Dispatcher.Invoke(delegate()
                {
                    uninstallCurrentStatusLabel.Content = "Stopping DWARFAppMan.exe instance...";
                });
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("DWARFAppMan"))
                {
                    proc.CloseMainWindow();
                    proc.Kill();
                    proc.WaitForExit();
                }
            }
            this.Dispatcher.Invoke(delegate()
            {
                uninstallCurrentStatusLabel.Content = "Deleting files...";
            });

            bool tryDeleteAgain = false;

            if (System.IO.File.Exists(DWARFSetup.Properties.Settings.Default.installationLocation + "\\installFiles.ndel"))
            {
                string[] filesystemitems = System.IO.File.ReadAllLines(DWARFSetup.Properties.Settings.Default.installationLocation + "\\installFiles.ndel");

                foreach (string filesystemitem in filesystemitems)
                {
                    if (System.IO.File.Exists(DWARFSetup.Properties.Settings.Default.installationLocation + "\\" + filesystemitem))
                    {
                        System.IO.File.Delete(DWARFSetup.Properties.Settings.Default.installationLocation + "\\" + filesystemitem);
                    }
                    else if (System.IO.Directory.Exists(DWARFSetup.Properties.Settings.Default.installationLocation + "\\" + filesystemitem))
                    {
                        Directory.Delete(DWARFSetup.Properties.Settings.Default.installationLocation + "\\" + filesystemitem);
                    }
                }

                System.IO.File.Delete(DWARFSetup.Properties.Settings.Default.installationLocation + "\\installFiles.ndel");
            }

            // What if they accidentally chose their Desktop as the install location...not a good idea

            //Directory.Delete(DWARFSetup.Properties.Settings.Default.installationLocation, true);

            try
            {
                Directory.Delete(Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Azuru\\DWARF\\", true);
            }
            catch { tryDeleteAgain = true; }
            this.Dispatcher.Invoke(delegate()
            {
                downloadProgress.Value = 85.5;
                uninstallCurrentStatusLabel.Content = "Deleting registry keys...";
            });
            bool deleteShortcut = false;
            try
            {
                using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
                {
                    deleteShortcut = Convert.ToBoolean(parent.OpenSubKey("DWARF").GetValue("CreatedShortcut", false));
                    this.Dispatcher.Invoke(delegate()
                    {
                        downloadProgress.Value = 71;
                    });
                    parent.DeleteSubKey("DWARF", false);
                    this.Dispatcher.Invoke(delegate()
                    {
                        downloadProgress.Value = 56.5;
                    });
                }
                RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                Key.DeleteValue("DWARFApplicationManager", false);
                this.Dispatcher.Invoke(delegate()
                {
                    downloadProgress.Value = 42;
                });
            }
            catch { }
            if (deleteShortcut)
            {
                this.Dispatcher.Invoke(delegate()
                {
                    uninstallCurrentStatusLabel.Content = "Deleting desktop shortcut...";
                });
                try
                {
                    System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\DWARF Foundry.lnk");
                }
                catch { }
                this.Dispatcher.Invoke(delegate()
                {
                    downloadProgress.Value = 27.5;
                });
            }
            if (tryDeleteAgain)
            {
                Thread.Sleep(1000);
                try
                {
                    Directory.Delete(Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Azuru\\DWARF\\", true);
                    this.Dispatcher.Invoke(delegate()
                    {
                        downloadProgress.Value = 19;
                    });
                }
                catch { }
            }
            this.Dispatcher.Invoke(delegate()
            {
                downloadProgress.Value = 0;
                uninstallCurrentStatusLabel.Content = "Finishing...";
                uninstallingCanvas.Visibility = System.Windows.Visibility.Hidden;
                installingCanvas.Visibility = System.Windows.Visibility.Hidden;
                finishedCanvas.Visibility = System.Windows.Visibility.Hidden;
                uninstalledCanvas.Visibility = System.Windows.Visibility.Visible;
            });
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            canClose = true;
            Environment.Exit(0);
            this.Close();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (installThread != null)
            {
                installThread.Abort();
                installThread.Join();
                installThread = null;
            }
            installWC.CancelAsync();
            installWC.Dispose();
            installWC = null;
            finishedCanvas.Width = 0;
            finishedCanvas.Height = 0;
            finishedCanvas.IsEnabled = false;
            finishedCanvas.Opacity = 0;
            DWARFSetup.Properties.Settings.Default.installationLocation = installationTextBox.Text;
            DWARFSetup.Properties.Settings.Default.Save();
            uninstallationCompleteLabel.Content = "Installation Cancelled";
            uninstallationDescriptionLabel.Content = "The DWARF setup process has been cancelled and all changes made have\r\nbeen rolled back.";
            new Thread(delegate()
            {
                Uninstall();
                canClose = true;
            }).Start();
        }
    }
}
