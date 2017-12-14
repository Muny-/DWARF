using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core;

namespace DWARF
{
    class PluginHelper
    {
        public static void CloseApp()
        {
            DWARFLoader.instance.CloseApp();
        }

        private static void CreateGlobalObject(string Name)
        {
            foreach (KeyValuePair<string, ApplicationForm> pair in DWARFLoader.instance.ChildForms)
            {
                pair.Value.webView.CreateGlobalJavascriptObject(Name);
            }
        }
    }
}
