using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DWARFLib
{
    public static class Consts
    {
        public static string AppData = Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\Azuru\\DWARF\\";

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
                        key = parent.OpenSubKey("DWARF", false) ??
                              parent.CreateSubKey("DWARF");

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

        public static class URLs
        {
            private const string BASE = "http://dwarf.azuru.me/api/1.0/foundry/";
            private const string BASE_DL = "http://cdn.azuru.me/apps/dwarf/foundry/";

            public const string LIST_APPS = BASE + "apps/list";
            public const string LIST_PLUGINS = BASE + "plugins/list";

            public const string LIST_APPS_ALL = LIST_APPS + "/all";
            public const string LIST_PLUGINS_ALL = LIST_PLUGINS + "/all";

            public static string APP_INFO(int ID)
            {
                return BASE + "app/" + ID;
            }

            public static string PLUGIN_INFO(int ID)
            {
                return BASE + "plugin/" + ID;
            }

            public static string PLUGIN_INFO_BASIC(int ID)
            {
                return PLUGIN_INFO(ID) + "/basic";
            }

            public static string APP_DOWNLOAD(int ID)
            {
                return BASE_DL + "app/" + ID + ".zip";
            }

            public static string PLUGIN_DOWNLOAD(int ID)
            {
                return BASE_DL + "plugin/" + ID + ".zip";
            }
        }

        public static class Commands
        {
            public const string EVENT = "0";

            public const string INSTALL_APPLICATION = "1";

            public const string INSTALL_PLUGIN = "2";

            public const string REMOVE_APPLICATION = "3";

            public const string REMOVE_PLUGIN = "4";

            public const string CHECK_FOR_UPDATES = "5";

            public const string UPDATE_APPLICATION = "6";

            public const string UPDATE_PLUGIN = "7";
        }

        public static class Events
        {
            public const string APPLICATION_INSTALL_PROGRESS_UPDATE = "0";
            public const string APPLICATION_INSTALLED = "1";
            public const string APPLICATION_REMOVED = "2";

            public const string PLUGIN_INSTALLED = "3";
            public const string PLUGIN_REMOVED = "4";
            public const string PLUGIN_UPDATED = "5";
            public const string PLUGIN_UPDATE_AVAILABLE = "6";

            public const string APPLICATION_UPDATE_AVAILABLE = "7";
            public const string APPLICATION_UPDATE_PROGRESS_UPDATE = "8";
            public const string APPLICATION_UPDATED = "9";
            public const string PLUGIN_UPDATE_PROGRESS_UPDATE = "10";
            public const string PLUGIN_INSTALL_PROGRESS_UPDATE = "11";

            public const string APPLICATION_INSTALL_CANCELLED = "12";
            public const string APPLICATION_REMOVE_CANCELLED = "13";
            public const string APPLICATION_UPDATE_CANCELLED = "14";

            public const string PLUGIN_INSTALL_CANCELLED = "15";
            public const string PLUGIN_REMOVE_CANCELLED = "16";
            public const string PLUGIN_UPDATE_CANCELLED = "17";
        }
    }
}
