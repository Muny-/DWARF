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
    public class PluginUpdater
    {
        WebClient wc = new WebClient();
        public int ID;

        dynamic _plugininfo;

        public PluginUpdater(int ID, Action OnCancelled)
        {
            this.ID = ID;

            if (!ApplicationManager.instance.InstalledPlugins.ContainsKey(ID))
            {
                OnCancelled();
            }
            else
            {
                _plugininfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.PLUGIN_INFO(ID)));

                var asi = new AskAction("Update", "Plugin", (string)_plugininfo.name, "Now, this plugin can:", ApplicationManager.instance.GetPluginPermissions(ID));
                asi.Visibility = System.Windows.Visibility.Visible;
                if (asi.ShowDialog().Value)
                {
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(Consts.URLs.PLUGIN_DOWNLOAD(ID)), Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");
                }
                else
                {
                    OnCancelled();
                }
            }
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(Consts.AppData + "\\Plugins\\" + (string)_plugininfo.name);
            FileSystemInfo[] fis = di.GetFileSystemInfos();

            foreach (FileSystemInfo fi in fis)
            {
                if (fi.GetType() == typeof(FileInfo))
                {
                    File.Delete(fi.FullName);
                }
                else if (fi.Name != "data")
                {
                    Directory.Delete(fi.FullName, true);
                }
            }

            ZipArchive ziparch = ZipFile.OpenRead(Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");
            ziparch.ExtractToDirectory(Consts.AppData + "\\Plugins", true);
            ziparch.Dispose();
            File.Delete(Consts.AppData + "\\Temp\\" + ID + ".plugin.zip");

            OnUpdated(new Plugin((string)_plugininfo.name, (string)_plugininfo.author, this.ID, (double)_plugininfo.version));
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressUpdate(e.ProgressPercentage);
        }

        public Action<Plugin> OnUpdated;
        public Action<int> OnProgressUpdate;
    }
}
