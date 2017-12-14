using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWARFLib;
using System.IO;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;
using IWshRuntimeLibrary;

namespace DWARFAppMan
{
    public class ApplicationManager
    {
        public static ApplicationManager instance;

        public Dictionary<int, Application> InstalledApplications = new Dictionary<int, Application>();
        public Dictionary<int, Plugin> InstalledPlugins = new Dictionary<int, Plugin>();

        public Dictionary<int, Application> NotInstalledApplications = new Dictionary<int, Application>();
        public Dictionary<int, Plugin> NotInstalledPlugins = new Dictionary<int, Plugin>();

        System.Windows.Forms.Timer FullInfoUpdater = new System.Windows.Forms.Timer();

        public ApplicationManager()
        {
            instance = this;
            FullInfoUpdater.Interval = 1800000;
            FullInfoUpdater.Tick += FullInfoUpdater_Tick;
            FullInfoUpdater.Start();
            FullInfoUpdater_Tick(null, null);
            LoadApps();
        }

        void FullInfoUpdater_Tick(object sender, EventArgs e)
        {
            var notinstalledapps = new Dictionary<int, Application>();
            var notinstalledplugins = new Dictionary<int, Plugin>();

            WebClient wc = new WebClient();

            dynamic pluginslist = DynamicJson.Parse(wc.DownloadString(Consts.URLs.LIST_PLUGINS_ALL));

            foreach (dynamic __plugin in pluginslist)
            {
                notinstalledplugins.Add((int)__plugin.id, new Plugin((string)__plugin.name, (string)__plugin.author, (int)__plugin.id, (double)__plugin.version));
            }

            NotInstalledPlugins = notinstalledplugins;

            dynamic appslist = DynamicJson.Parse(wc.DownloadString(Consts.URLs.LIST_APPS_ALL));

            foreach (dynamic __app in appslist)
            {
                notinstalledapps.Add((int)__app.id, new Application((string)__app.name, (string)__app.author, (int)__app.id, (double)__app.version, GetNotInstalledApplicationPlugins(__app.plugins), false));
            }

            NotInstalledApplications = notinstalledapps;
        }

        void LoadApps()
        {
            dynamic apps = null;
            dynamic plugins = null;
            try
            {
                apps = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\DWARFAppMan\\Applications.json"));
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error!  Unable to load Application list, shutting down...");
                Environment.Exit(0);
            }

            try
            {
                plugins = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\DWARFAppMan\\Plugins.json"));
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error!  Unable to load Plugin list, shutting down...");
                Environment.Exit(0);
            }

            foreach (dynamic plugin in plugins)
            {
                dynamic _plugininfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Plugins\\" + plugin.name + "\\plugin_info.json"));

                InstalledPlugins.Add((int)plugin.id, new Plugin((string)_plugininfo.name, (string)_plugininfo.author, (int)plugin.id, (double)_plugininfo.version));
            }

            foreach (dynamic app in apps)
            {
                dynamic _appinfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Applications\\" + app.name + "\\app_info.dwarf"));

                InstalledApplications.Add((int)app.id, new Application((string)_appinfo.name, (string)_appinfo.author, (int)app.id, (double)_appinfo.version, GetInstalledApplicationPlugins((string)_appinfo.name, true), _appinfo.IsDefined("desktopicon")));

                if (_appinfo.IsDefined("runonstartup") && (bool)_appinfo.runonstartup)
                {
                    System.Diagnostics.Process.Start(Consts.GetInstallLocation() + "\\DWARF.exe", "\"" + Consts.AppData + "\\Applications\\" + app.name + "\\app_info.dwarf" + "\"");
                }
            }
        }

        public void SaveApplicationsJSON()
        {
            System.IO.File.WriteAllText(Consts.AppData + "DWARFAppMan\\Applications.json", new DJSValue(InstalledApplications).ToJSONString());
        }

        public void SavePluginsJSON()
        {
            System.IO.File.WriteAllText(Consts.AppData + "DWARFAppMan\\Plugins.json", new DJSValue(InstalledPlugins).ToJSONString());
        }

        public void InstallApplication(int ID)
        {
            new Thread(delegate()
            {
                new ApplicationInstaller(ID, () =>
                {
                    WSListener.instance.OnApplicationInstallCancelled(ID);
                })
                {
                    OnInstalled = AppInst =>
                    {
                        WSListener.instance.OnApplicationInstalled(ID);
                        InstalledApplications.Add(ID, AppInst);
                        SaveApplicationsJSON();
                    },
                    OnProgressUpdate = Progress => 
                    {
                        WSListener.instance.OnApplicationInstallProgressUpdate(ID, Progress);
                    }
                };
            })
            {
                ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void RemoveApplication(int ID)
        {
            new Thread(delegate()
            {
                new ApplicationRemover(ID, () =>
                {
                    WSListener.instance.OnApplicationRemoved(ID);
                    InstalledApplications.Remove(ID);
                    SaveApplicationsJSON();
                }, () =>
                {
                    WSListener.instance.OnApplicationRemoveCancelled(ID);
                });
            })
            {
                ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void UpdateApplication(int ID)
        {
            new Thread(delegate()
            {
                new ApplicationUpdater(ID, () =>
                {
                    WSListener.instance.OnApplicationUpdateCancelled(ID);
                })
                {
                    OnUpdated = AppInst =>
                    {
                        WSListener.instance.OnApplicationUpdated(ID);
                        InstalledApplications[ID] = AppInst;
                        SaveApplicationsJSON();
                    },
                    OnProgressUpdate = Progress =>
                    {
                        WSListener.instance.OnApplicationUpdateProgressUpdate(ID, Progress);
                    }
                };
            })
            {
                ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void InstallPlugin(int ID, bool force)
        {
            new Thread(delegate()
            {
                new PluginInstaller(ID, () =>
                {
                    WSListener.instance.OnPluginInstallCancelled(ID);
                }, force)
                {
                    OnInstalled = PluginInst =>
                    {
                        if (!force)
                        {
                            WSListener.instance.OnPluginInstalled(ID);
                        }
                        InstalledPlugins.Add(ID, PluginInst);
                        SavePluginsJSON();
                        
                    },
                    OnProgressUpdate = Progress =>
                    {
                        if (!force)
                        {
                            WSListener.instance.OnPluginInstallProgressUpdate(ID, Progress);
                        }
                    }
                };
            })
            {
                 ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void RemovePlugin(int ID)
        {
            new Thread(delegate()
            {
                new PluginRemover(ID, () =>
                {
                    WSListener.instance.OnPluginRemoved(ID);
                    InstalledPlugins.Remove(ID);
                    SavePluginsJSON();
                }, () =>
                {
                    WSListener.instance.OnPluginRemoveCancelled(ID);
                });
            })
            {
                ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void UpdatePlugin(int ID)
        {
            new Thread(delegate()
            {
                new PluginUpdater(ID, () =>
                {
                    WSListener.instance.OnPluginUpdateCancelled(ID);
                })
                {
                    OnUpdated = PluginInst =>
                    {
                        WSListener.instance.OnPluginUpdated(ID);
                        InstalledPlugins[ID] = PluginInst;
                        SavePluginsJSON();
                    },
                    OnProgressUpdate = Progress =>
                    {
                        WSListener.instance.OnPluginUpdateProgressUpdate(ID, Progress);
                    }
                };
            })
            {
                ApartmentState = ApartmentState.STA
            }.Start();
        }

        public void CheckForUpdates()
        {
            foreach (KeyValuePair<int, Application> pair in InstalledApplications)
            {
                new Thread(delegate()
                {
                    new ApplicationUpdateChecker(pair.Value, NewVersion =>
                    {
                        WSListener.instance.OnApplicationUpdateAvailable(pair.Key, NewVersion);
                    });
                })
                {
                    ApartmentState = ApartmentState.STA
                }.Start();
            }

            foreach (KeyValuePair<int, Plugin> pair in InstalledPlugins)
            {
                new Thread(delegate()
                {
                    new PluginUpdateChecker(pair.Value, NewVersion =>
                    {
                        WSListener.instance.OnPluginUpdateAvailable(pair.Key, NewVersion);
                    });
                })
                {
                    ApartmentState = ApartmentState.STA
                }.Start();
            }
        }

        public string GetPluginPermissions(int ID)
        {
            string perms = "";

            WebClient wc = new WebClient();

            dynamic plugininfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.PLUGIN_INFO_BASIC(ID)));

            foreach (string perm in plugininfo.permissions)
            {
                perms += " - " + perm + "\n";
            }

            return perms;
        }
        
        public string GetApplicationPermissions(int ID)
        {
            string perms = "";

            List<int> plugins = GetNotInstalledApplicationPlugins(ID);

            foreach (int plugin in plugins)
            {
                perms += NotInstalledPlugins[plugin].Name + ": \n" + GetPluginPermissions(plugin).Replace(" - ", "    - ") + "\n";
            }

            return perms;
        }

        public List<int> GetInstalledApplicationPlugins(string Name, bool assumePluginInstalled)
        {
            dynamic _appinfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Applications\\" + Name + "\\app_info.dwarf"));

            List<int> plugins = new List<int>();

            foreach (string plugin in _appinfo.plugins)
            {
                if (assumePluginInstalled)
                {
                    try
                    {
                        plugins.Add(InstalledPlugins.Where(value => value.Value.Name == plugin).First().Value.ID);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Error!  Please install the plugin '" + plugin + "' then restart DWARF Application Manager.");
                    }
                }
                else
                {
                    plugins.Add(NotInstalledPlugins.Where(value => value.Value.Name == plugin).First().Value.ID);
                }
            }

            return plugins;
        }

        public List<int> GetNotInstalledApplicationPlugins(int ID)
        {
            WebClient wc = new WebClient();

            dynamic _appinfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.APP_INFO(ID)));

            List<int> plugins = new List<int>();

            foreach (int plugin in _appinfo.plugins)
            {
                plugins.Add(plugin);
            }

            return plugins;
        }

        public List<int> GetNotInstalledApplicationPlugins(dynamic plugins)
        {
            List<int> pluginslst = new List<int>();

            foreach (int plugin in plugins)
            {
                pluginslst.Add(plugin);
            }

            return pluginslst;
        }

        public void MakeAppShortcut(string Name)
        {
            dynamic _appinfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Applications\\" + Name + "\\app_info.dwarf"));

            if (_appinfo.IsDefined("desktopicon"))
            {
                string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                WshShell shell = (WshShell)Activator.CreateInstance(System.Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(deskDir + "\\" + (string)_appinfo.name + ".lnk");
                shortcut.TargetPath = Consts.GetInstallLocation() + "\\DWARF.exe";
                shortcut.Arguments = "\"" + Consts.AppData + "\\Applications\\" + (string)_appinfo.name + "\\app_info.dwarf" + "\"";
                shortcut.Description = "Launch " + (string)_appinfo.name;
                shortcut.IconLocation = Consts.AppData + "\\Applications\\" + (string)_appinfo.name + "\\" + _appinfo.desktopicon;
                shortcut.Save();
            }
        }

        public void RemoveAppShortcut(string Name)
        {
            dynamic _appinfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Applications\\" + Name + "\\app_info.dwarf"));

            if (_appinfo.IsDefined("desktopicon"))
            {
                System.IO.File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + (string)_appinfo.name + ".lnk");
            }
        }

        public bool AppHasShortcut(string Name)
        {
            dynamic _appinfo = DynamicJson.Parse(System.IO.File.ReadAllText(Consts.AppData + "\\Applications\\" + Name + "\\app_info.dwarf"));

            if (_appinfo.IsDefined("desktopicon"))
            {
                return true;
            }
            else
                return false;
        }
    }
}
