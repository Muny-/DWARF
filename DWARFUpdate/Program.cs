using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32;

namespace DWARFUpdate
{

    static class Program
    {
        public static string GetInstallLocation()
        {
            string installloc = null;

            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false))
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
                        key = parent.OpenSubKey("DWARF", false);

                        if (key == null)
                        {
                            throw new Exception("Unable to get key");
                        }

                        installloc = (string)key.GetValue("InstallLocation");

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
                    System.Windows.Forms.MessageBox.Show("Exception: " + ex.Message);
                }
            }

            return installloc;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                string installLoc = GetInstallLocation();

                WebClient wc = new WebClient();
                string temp_zip_file = Path.GetTempFileName() + ".DWARF_upd";
                wc.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                wc.DownloadFile("http://cdn.azuru.me/apps/dwarf/program/install.zip", temp_zip_file);
                //wc.DownloadFile("http://192.168.1.8/install.zip", temp_zip_file);

                ZipArchive ziparch = ZipFile.OpenRead(temp_zip_file);
                kill_DWARF();
                if (File.Exists(installLoc + "\\installFiles.ndel"))
                {
                    string[] files = File.ReadAllLines(installLoc + "\\installFiles.ndel");

                    foreach (string file in files)
                    {
                        File.Delete(installLoc + "\\" + file);
                    }

                    File.Delete(installLoc + "\\installFiles.ndel");

                    ziparch.ExtractToDirectory(installLoc);
                    Process.Start(installLoc + "\\DWARFAppMan.exe", "updated \"" + Application.ExecutablePath + "\"");
                }
                else
                {
                    MessageBox.Show("Error updating DWARF:  Missing 'installFiles.ndel'\n\nContact Azuru support for help.");
                }
                ziparch.Dispose();
                wc.Dispose();
                File.Delete(temp_zip_file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating DWARF: " + ex.Message);
            }
        }

        private static void kill_DWARF()
        {
            Process[] p1 = Process.GetProcessesByName("DWARF");
            Process[] p2 = Process.GetProcessesByName("DWARFAppMan");

            foreach (Process p in p1)
            {
                p.Kill();
                p.WaitForExit();
            }

            foreach (Process p in p2)
            {
                p.Kill();
                p.WaitForExit();
            }
        }
    }
}
