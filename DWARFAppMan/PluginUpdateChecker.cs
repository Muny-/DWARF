using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWARFLib;
using System.Net;

namespace DWARFAppMan
{
    public class PluginUpdateChecker
    {
        public Plugin Plug;

        public PluginUpdateChecker(Plugin Plug, Action<double> OnUpdateAvailable)
        {
            this.Plug = Plug;

            WebClient wc = new WebClient();

            dynamic _plug = DynamicJson.Parse(wc.DownloadString(Consts.URLs.PLUGIN_INFO(Plug.ID)));

            double current_v = (double)_plug.version;

            if (current_v > Plug.Version)
                OnUpdateAvailable(current_v);
        }
    }
}
