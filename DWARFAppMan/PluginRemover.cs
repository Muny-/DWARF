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
    public class PluginRemover
    {
        public int ID;

        public PluginRemover(int ID, Action OnRemoved, Action OnCancelled)
        {
            this.ID = ID;

            if (!ApplicationManager.instance.InstalledPlugins.ContainsKey(ID))
            {
                OnCancelled();
            }
            else
            {
                WebClient wc = new WebClient();
                dynamic _plugininfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.PLUGIN_INFO(ID)));

                var asi = new AskAction("Remove", "Plugin", _plugininfo.name, "", "Removing this plugin will cause all applications that utilize it to stop working.");
                asi.Visibility = System.Windows.Visibility.Visible;
                if (asi.ShowDialog().Value)
                {
                    Directory.Delete(Consts.AppData + "\\Plugins\\" + (string)_plugininfo.name, true);
                    OnRemoved();
                }
                else
                {
                    OnCancelled();
                }
            }
        }
    }
}
