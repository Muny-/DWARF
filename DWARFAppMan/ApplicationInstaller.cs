using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.IO.Compression;
using DWARFLib;
using IWshRuntimeLibrary;

namespace DWARFAppMan
{
    public class ApplicationInstaller
    {
        WebClient wc = new WebClient();
        public int ID;

        dynamic _appinfo;

        public ApplicationInstaller(int ID, Action OnCancelled)
        {
            this.OnCancelled = OnCancelled;
            if (ApplicationManager.instance.InstalledApplications.ContainsKey(ID))
            {
                OnCancelled();
            }
            else
            {
                _appinfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.APP_INFO(ID)));

                var asi = new AskAction("Install", "Application", _appinfo.name, "This application's plugins can:", ApplicationManager.instance.GetApplicationPermissions(ID));
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
            if (e.Cancelled || e.Error != null)
                OnCancelled();
            else
            {
                ZipArchive ziparch = ZipFile.OpenRead(Consts.AppData + "\\Temp\\" + ID + ".zip");
                ziparch.ExtractToDirectory(Consts.AppData + "\\Applications");
                ziparch.Dispose();
                System.IO.File.Delete(Consts.AppData + "\\Temp\\" + ID + ".zip");

                List<int> plugins = ApplicationManager.instance.GetInstalledApplicationPlugins((string)_appinfo.name, false);

                foreach (int plugin in plugins)
                {
                    if (!ApplicationManager.instance.InstalledPlugins.ContainsKey(plugin))
                        ApplicationManager.instance.InstallPlugin(plugin, true);
                }

                ApplicationManager.instance.MakeAppShortcut((string)_appinfo.name);

                OnInstalled(new Application((string)_appinfo.name, (string)_appinfo.author, this.ID, (double)_appinfo.version, plugins, ApplicationManager.instance.AppHasShortcut((string)_appinfo.name)));
            }
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressUpdate(e.ProgressPercentage);
        }

        public Action<Application> OnInstalled;
        public Action<int> OnProgressUpdate;
        public Action OnCancelled;
    }
}
