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
    public class ApplicationUpdater
    {
        WebClient wc = new WebClient();
        public int ID;
        dynamic _appinfo;

        public ApplicationUpdater(int ID, Action OnCancelled)
        {
            if (!ApplicationManager.instance.InstalledApplications.ContainsKey(ID))
            {
                OnCancelled();
            }
            else
            {
                _appinfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.APP_INFO(ID)));

                var asi = new AskAction("Update", "Application", (string)_appinfo.name, "Now, this application's plugins can:", ApplicationManager.instance.GetApplicationPermissions(ID));
                asi.Visibility = System.Windows.Visibility.Visible;
                if (asi.ShowDialog().Value)
                {
                    this.ID = ID;
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(Consts.URLs.APP_DOWNLOAD(ID)), Consts.AppData + "\\Temp\\" + ID + ".zip");
                }
                else
                {
                    OnCancelled();
                }
            }
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ApplicationManager.instance.RemoveAppShortcut((string)_appinfo.name);
            DirectoryInfo di = new DirectoryInfo(Consts.AppData + "\\Applications\\" + (string)_appinfo.name);
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

            ZipArchive ziparch = ZipFile.OpenRead(Consts.AppData + "\\Temp\\" + ID + ".zip");
            ziparch.ExtractToDirectory(Consts.AppData + "\\Applications", true);
            ziparch.Dispose();
            File.Delete(Consts.AppData + "\\Temp\\" + ID + ".zip");

            List<int> plugins = ApplicationManager.instance.GetInstalledApplicationPlugins((string)_appinfo.name, false);

            foreach (int plugin in plugins)
            {
                if (!ApplicationManager.instance.InstalledPlugins.ContainsKey(plugin))
                    ApplicationManager.instance.InstallPlugin(plugin, true);
            }

            ApplicationManager.instance.MakeAppShortcut((string)_appinfo.name);

            OnUpdated(new Application((string)_appinfo.name, (string)_appinfo.author, this.ID, (double)_appinfo.version, plugins, ApplicationManager.instance.AppHasShortcut((string)_appinfo.name)));
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressUpdate(e.ProgressPercentage);
        }

        public Action<Application> OnUpdated;
        public Action<int> OnProgressUpdate;
    }
}
