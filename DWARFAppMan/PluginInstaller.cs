using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.IO.Compression;
using DWARFLib;

namespace DWARFAppMan
{
    public class PluginInstaller
    {
        WebClient wc = new WebClient();
        public int ID;
        dynamic _plugininfo;

        public PluginInstaller(int ID, Action OnCancelled, bool force)
        {
            this.ID = ID;

            if (ApplicationManager.instance.InstalledPlugins.ContainsKey(ID) && !force)
            {
                OnCancelled();
            }
            else
            {
                _plugininfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.PLUGIN_INFO(ID)));

                if (force)
                    work();
                else
                {
                    var asi = new AskAction("Install", "Plugin", _plugininfo.name, "This plugin can:", ApplicationManager.instance.GetPluginPermissions(ID));
                    asi.Visibility = System.Windows.Visibility.Visible;
                    if (asi.ShowDialog().Value)
                    {
                        work();
                    }
                    else
                    {
                        OnCancelled();
                    }
                }
            }
        }

        void work()
        {
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
            wc.DownloadFileAsync(new Uri(Consts.URLs.PLUGIN_DOWNLOAD(ID)), Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ZipArchive ziparch = ZipFile.OpenRead(Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");
            ziparch.ExtractToDirectory(Consts.AppData + "\\Plugins");
            ziparch.Dispose();
            File.Delete(Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");
            OnInstalled(new Plugin((string)_plugininfo.name, (string)_plugininfo.author, this.ID, (double)_plugininfo.version));
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressUpdate(e.ProgressPercentage);
        }

        public Action<Plugin> OnInstalled;
        public Action<int> OnProgressUpdate;
    }
}
