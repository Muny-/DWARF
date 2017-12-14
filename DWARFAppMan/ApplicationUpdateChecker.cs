using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWARFLib;
using System.Net;

namespace DWARFAppMan
{
    public class ApplicationUpdateChecker
    {
        public Application App;

        public ApplicationUpdateChecker(Application App, Action<double> OnUpdateAvailable)
        {
            this.App = App;

            WebClient wc = new WebClient();

            dynamic _appinfo = DynamicJson.Parse(wc.DownloadString(Consts.URLs.APP_INFO(App.ID)));

            double current_v = (double)_appinfo.version;

            if (current_v > App.Version)
                OnUpdateAvailable(current_v);
        }
    }
}
